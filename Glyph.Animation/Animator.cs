﻿using System.Collections.Generic;
using Glyph.Composition;

namespace Glyph.Animation
{
    public class Animator<T> : GlyphComposite<IAnimationPlayer>, IUpdate, ITimeUnscalable
        where T : class
    {
        private bool _useUnscaledTime;
        public T Animatable { get; private set; }

        public bool UseUnscaledTime
        {
            get { return _useUnscaledTime; }
            set
            {
                _useUnscaledTime = value;
                foreach (IAnimationPlayer player in this)
                    player.UseUnscaledTime = value;
            }
        }

        public Animator(T animatable)
        {
            Animatable = animatable;
        }

        public AnimationPlayer<T> Add(Animation<T> animation)
        {
            var player = new AnimationPlayer<T>(Animatable)
            {
                Animation = animation
            };

            Add(player);
            return player;
        }

        public AnimationPlayer<T> Add(IAnimationBuilder<T> animationBuilder)
        {
            var player = new AnimationPlayer<T>(Animatable)
            {
                Animation = animationBuilder.Create()
            };

            Add(player);
            return player;
        }

        public override void Add(IAnimationPlayer item)
        {
            base.Add(item);
            item.UseUnscaledTime = _useUnscaledTime;
        }

        public void Update(ElapsedTime elapsedTime)
        {
            if (ReadOnlyComponents.Count == 0)
                return;

            var playersToRemove = new List<IAnimationPlayer>();

            foreach (IAnimationPlayer player in this)
            {
                player.Update(elapsedTime);

                if (player.Ended)
                    playersToRemove.Add(player);
            }

            foreach (IAnimationPlayer player in playersToRemove)
                Remove(player);
        }
    }
}