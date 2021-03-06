﻿using System;
using System.Collections;
using System.Linq;
using Diese.Collections;
using Niddle;
using Niddle.Base;
using Glyph.Composition;
using Glyph.Composition.Exceptions;
using Glyph.Injection;

namespace Glyph.Core.Injection
{
    public class GlyphCompositeInjector : DependencyInjectorBase
    {
        private readonly GlyphComposite _composite;

        public RegistryInjector Global { get; }
        public LocalDependencyInjector Local { get; }

        internal GlyphObject Parent
        {
            set => Local.Parent = value?.Injector.Local;
        }

        public GlyphCompositeInjector(GlyphComposite composite, GlyphInjectionContext context)
        {
            _composite = composite;

            Global = context.GlobalInjector;
            Local = new LocalDependencyInjector(context.LocalRegistry, context.LocalInjectorParent);
            Local.Registry.RegisterInstance(Local);

            foreach (Type nestedType in _composite.GetType().GetNestedTypes())
                Local.Registry.Register(nestedType);
        }

        public override object Resolve(Type type, InjectableAttributeBase injectableAttribute = null, object serviceKey = null, InstanceOrigins instanceOrigins = InstanceOrigins.All, IDependencyInjector dependencyInjector = null)
        {
            GlyphInjectableTargets targets = GetTargets(type, injectableAttribute);

            if (serviceKey == null && (instanceOrigins & InstanceOrigins.Registration) != 0)
            {
                if ((targets & GlyphInjectableTargets.Parent) != 0 && type.IsInstanceOfType(_composite))
                    return _composite;

                if ((targets & GlyphInjectableTargets.Fraternal) != 0 && TryResolveManyFamily(out IEnumerable objs, type))
                    return objs.First();
            }

            if ((targets & GlyphInjectableTargets.Local) != 0 && Local.TryResolve(out object obj, type, injectableAttribute, serviceKey, instanceOrigins, dependencyInjector ?? this))
                return obj;

            if (targets == GlyphInjectableTargets.Local)
                throw new InvalidOperationException();

            if ((targets & GlyphInjectableTargets.Global) != 0)
                return Global.Resolve(type, injectableAttribute, serviceKey, instanceOrigins, dependencyInjector ?? this);

            throw new InvalidOperationException();
        }

        public override bool TryResolve(out object obj, Type type, InjectableAttributeBase injectableAttribute = null, object serviceKey = null, InstanceOrigins instanceOrigins = InstanceOrigins.All, IDependencyInjector dependencyInjector = null)
        {
            GlyphInjectableTargets targets = GetTargets(type, injectableAttribute);

            if (serviceKey == null && (instanceOrigins & InstanceOrigins.Registration) != 0)
            {
                if ((targets & GlyphInjectableTargets.Parent) != 0 && type.IsInstanceOfType(_composite))
                {
                    obj = _composite;
                    return true;
                }

                if ((targets & GlyphInjectableTargets.Fraternal) != 0 && TryResolveManyFamily(out IEnumerable objs, type))
                {
                    obj = objs.FirstOrDefault();
                    return true;
                }
            }

            if ((targets & GlyphInjectableTargets.Local) != 0 && Local.TryResolve(out obj, type, injectableAttribute, serviceKey, instanceOrigins, dependencyInjector ?? this))
                return true;

            if (targets == GlyphInjectableTargets.Local)
            {
                obj = null;
                return false;
            }

            if ((targets & GlyphInjectableTargets.Global) != 0)
                return Global.TryResolve(out obj, type, injectableAttribute, serviceKey, instanceOrigins, dependencyInjector ?? this);

            obj = null;
            return false;
        }

        public override IEnumerable ResolveMany(Type type, InjectableAttributeBase injectableAttribute = null, object serviceKey = null, InstanceOrigins instanceOrigins = InstanceOrigins.All, IDependencyInjector dependencyInjector = null)
        {
            if (!(injectableAttribute is IGlyphInjectableAttribute glyphInjectableAttribute))
                return base.ResolveMany(type, injectableAttribute, serviceKey, instanceOrigins, dependencyInjector ?? this);

            GlyphInjectableTargets targets = glyphInjectableAttribute.Targets;
            if (targets == 0)
                throw new InvalidOperationException();

            if (serviceKey != null)
                return Enumerable.Empty<object>();

            return ResolveManyFamily(type);
        }

        public override bool TryResolveMany(out IEnumerable objs, Type type, InjectableAttributeBase injectableAttribute = null, object serviceKey = null, InstanceOrigins instanceOrigins = InstanceOrigins.All, IDependencyInjector dependencyInjector = null)
        {
            if (!(injectableAttribute is IGlyphInjectableAttribute glyphInjectableAttribute))
                return base.TryResolveMany(out objs, type, injectableAttribute, serviceKey, instanceOrigins, dependencyInjector ?? this);

            GlyphInjectableTargets targets = glyphInjectableAttribute.Targets;
            if (targets == 0)
                throw new InvalidOperationException();

            if (serviceKey != null)
            {
                objs = Enumerable.Empty<object>();
                return false;
            }

            return TryResolveManyFamily(out objs, type);
        }

        static private GlyphInjectableTargets GetTargets(Type type, InjectableAttributeBase injectableAttribute)
        {
            GlyphInjectableTargets targets;
            if (injectableAttribute is IGlyphInjectableAttribute glyphInjectableAttribute)
            {
                targets = glyphInjectableAttribute.Targets;
                if (targets == 0)
                    throw new InvalidOperationException();
            }
            else
            {
                targets = GlyphInjectableTargets.Global | GlyphInjectableTargets.Local | GlyphInjectableTargets.Parent;
                if (typeof(IGlyphComponent).IsAssignableFrom(type))
                    targets |= GlyphInjectableTargets.Fraternal;
            }

            return targets;
        }

        private IEnumerable ResolveManyFamily(Type type)
        {
            bool any = false;

            foreach (IGlyphComponent component in _composite.Components.OfType(type))
            {
                yield return component;

                any = true;
            }

            if (!any)
                throw new InvalidOperationException();
        }

        private bool TryResolveManyFamily(out IEnumerable objs, Type type)
        {
            objs = Enumerable.Empty<object>();
            bool any = false;

            foreach (IGlyphComponent component in _composite.Components.OfType(type))
            {
                objs = objs.Concat(component);
                any = true;
            }

            return any;
        }

        public T ResolveLocal<T>(object serviceKey = null, InstanceOrigins instanceOrigins = InstanceOrigins.All, IDependencyInjector dependencyInjector = null)
        {
            return Resolve<T>(new GlyphInjectableAttribute(GlyphInjectableTargets.Local), serviceKey, instanceOrigins, dependencyInjector);
        }

        public object ResolveLocal(Type type, object serviceKey = null, InstanceOrigins instanceOrigins = InstanceOrigins.All, IDependencyInjector dependencyInjector = null)
        {
            return Resolve(type, new GlyphInjectableAttribute(GlyphInjectableTargets.Local), serviceKey, instanceOrigins, dependencyInjector);
        }

        internal T Add<T>()
            where T : IGlyphComponent
        {
            var component = this.WithInstance(Local).Resolve<T>(instanceOrigins: InstanceOrigins.Instantiation);
            _composite.Add(component);

            return component;
        }

        internal object Add(Type type)
        {
            if (!typeof(IGlyphComponent).IsAssignableFrom(type))
                throw new InvalidCastException($"Type must implements {typeof(IGlyphComponent)} !");

            var component = (IGlyphComponent)this.WithInstance(Local).Resolve(type, instanceOrigins: InstanceOrigins.Instantiation);
            _composite.Add(component);

            return component;
        }
    }
}