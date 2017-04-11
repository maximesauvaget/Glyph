﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Diese;
using Diese.Collections;
using Diese.Debug;
using Diese.Injection;
using Glyph.Composition;
using Glyph.Composition.Delegates;
using Glyph.Composition.Exceptions;
using Glyph.Composition.Messaging;
using Glyph.Core.Injection;
using Glyph.Core.Scheduler;
using Glyph.Injection;
using Glyph.Math;
using Glyph.Messaging;
using Stave;

namespace Glyph.Core
{
    public class GlyphObject : GlyphComposite, IGlyphCompositeResolver, IEnableable, ILoadContent, IUpdate, IDraw, IBoxedComponent
    {
        static public readonly WatchTree UpdateWatchTree = new WatchTree();

        private bool _initialized;
        private bool _contentLoaded;
        private readonly NewComponentBatchTree _newComponents;
        protected readonly SchedulerHandler Schedulers;
        protected internal readonly GlyphCompositeInjector Injector;
        public bool Enabled { get; set; }
        public bool Visible { get; set; }

        public GlyphObject(IDependencyInjector injector)
        {
            Enabled = true;
            Visible = true;

            var compositeInjector = new GlyphCompositeInjector(this, injector.Resolve<IDependencyRegistry>(), injector.Resolve<IDependencyRegistry>(InjectionScope.Local));
            Injector = compositeInjector;

            Schedulers = new SchedulerHandler(injector);
            _newComponents = new NewComponentBatchTree(this);

            foreach (Type type in GetType().GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ILocalInterpreter<>)))
                Injector.LocalRegistry.Register(typeof(IRouter<>).MakeGenericType(type.GetGenericArguments()), typeof(LocalRouter<>).MakeGenericType(type.GetGenericArguments()), Subsistence.Singleton);

            foreach (Type type in GetType().GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IInterpreter<>)))
                Add(typeof(Receiver<>).MakeGenericType(type.GetGenericArguments()));
        }

        public T Add<T>()
            where T : IGlyphComponent
        {
            return Injector.Add<T>();
        }

        public IGlyphComponent Add(Type componentType)
        {
            return Injector.Add(componentType) as IGlyphComponent;
        }

        // TODO : Handle injection on changing children & parents
        public override sealed void Add(IGlyphComponent item)
        {
            if (_newComponents.Batch(item))
                return;

            if (Contains(item))
                throw new ArgumentException("Component provided is already contained by this entity !");

            Type type = item.GetType();
            if (Components.Any(type) && type.GetCustomAttributes(typeof(SinglePerParentAttribute)).Any())
                throw new SingleComponentException(type);

            base.Add(item);

            foreach (IGlyphComponent component in Components)
                foreach (InjectablePropertyInfo injectable in InstanceManager.GetInfo(component.GetType()).InjectableProperties)
                    if (injectable.InjectableTargets.HasFlag(GlyphInjectableTargets.Fraternal)
                        && item.GetType().IsInstanceOfType(injectable.PropertyInfo.PropertyType))
                        injectable.PropertyInfo.SetValue(component, item);

            var glyphObject = item as GlyphObject;
            if (glyphObject != null)
            {
                Schedulers.AssignComponent(glyphObject);
                glyphObject.Injector.Parent = this;
            }
            else
                Schedulers.AssignComponent(item);

            if (_initialized)
                item.Initialize();

            if (_contentLoaded)
                (item as ILoadContent)?.LoadContent(Injector.Resolve<ContentLibrary>());
        }

        public override sealed bool Remove(IGlyphComponent item)
        {
            if (!Contains(item) || !base.Remove(item))
                return false;

            var glyphObject = item as GlyphObject;
            if (glyphObject != null)
                Schedulers.RemoveComponent(glyphObject);
            else
                Schedulers.RemoveComponent(item);

            return true;
        }

        public override sealed void Clear()
        {
            base.Clear();

            Schedulers.ClearComponents();
        }

        public override sealed void Initialize()
        {
            using (_newComponents.BeginBatch())
            {
                foreach (InitializeDelegate initialize in Schedulers.Initialize.Planning)
                    initialize();

                _initialized = true;
            }
        }

        public void LoadContent(ContentLibrary contentLibrary)
        {
            using (_newComponents.BeginBatch())
            {
                foreach (LoadContentDelegate loadContent in Schedulers.LoadContent.Planning)
                    loadContent(contentLibrary);

                _contentLoaded = true;
            }
        }

        public void Update(ElapsedTime elapsedTime)
        {
            using (_newComponents.BeginBatch())
            {
                foreach (UpdateDelegate update in Schedulers.Update.Planning)
                {
                    if (!Enabled)
                        return;

                    using (UpdateWatchTree.Start($"{update.Target?.GetType().GetDisplayName()} -- {update.Method.Name}"))
                        update(elapsedTime);
                }
            }
        }
        
        public void Draw(IDrawer drawer)
        {
            if (!Visible)
                return;

            foreach (DrawDelegate draw in Schedulers.Draw.Planning)
                draw(drawer);
        }

        protected void SendMessage<TMessage>(TMessage message)
            where TMessage : Message
        {
            var router = Injector.Resolve<IRouter<TMessage>>();
            router.Send(message);
        }

        ISceneNode IBoxedComponent.SceneNode => Components.FirstOrDefault<ISceneNode>() ?? this.ParentQueue().SelectMany(x => x.Components).First<ISceneNode>();
        IArea IBoxedComponent.Area => MathUtils.GetBoundingBox(Components.OfType<IBoxedComponent>().Select(x => x.Area));

        private sealed class NewComponentBatchTree : BatchTree<IGlyphComponent>
        {
            private readonly GlyphObject _owner;

            public NewComponentBatchTree(GlyphObject owner)
            {
                _owner = owner;
            }

            protected override void OnNodeEnded(Queue<IGlyphComponent> queue, int depth)
            {
                while (queue.Count != 0)
                    _owner.Add(queue.Dequeue());
            }
        }

        public class SchedulerHandler : GlyphSchedulerHandler
        {
            public GlyphScheduler<IGlyphComponent, InitializeDelegate> Initialize { get; private set; }
            public GlyphScheduler<ILoadContent, LoadContentDelegate> LoadContent { get; private set; }
            public GlyphScheduler<IUpdate, UpdateDelegate> Update { get; private set; }
            public GlyphScheduler<IDraw, DrawDelegate> Draw { get; private set; }

            public SchedulerHandler(IDependencyInjector injector)
                : base(injector)
            {
                Initialize = AddScheduler<IGlyphComponent, InitializeDelegate>();
                LoadContent = AddScheduler<ILoadContent, LoadContentDelegate>();
                Update = AddScheduler<IUpdate, UpdateDelegate>();
                Draw = AddScheduler<IDraw, DrawDelegate>();
            }
        }
    }
}