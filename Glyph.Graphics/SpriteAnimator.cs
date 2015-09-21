﻿using System.Collections.Generic;
using System.Linq;
using Glyph.Composition;
using Microsoft.Xna.Framework.Graphics;

namespace Glyph.Graphics
{
    [SinglePerParent]
    public class SpriteAnimator : GlyphComponent, ISpriteSource, IEnableable, IUpdate
    {
        private readonly ISpriteSheet _spriteSheet;
        private readonly SpriteTransformer _transformer;
        private readonly Period _period;
        private readonly Queue<object> _keysQueue;
        public bool Enabled { get; set; }
        public bool Ended { get; set; }
        public Dictionary<object, SpriteAnimation> Animations { get; private set; }
        public SpriteAnimation CurrentAnimation { get; private set; }
        public object CurrentKey { get; private set; }
        public int CurrentStep { get; private set; }
        public int CurrentFrame { get; private set; }

        public Texture2D Texture
        {
            get { return _spriteSheet.GetFrameTexture(CurrentFrame); }
        }

        public IReadOnlyCollection<object> KeysQueue
        {
            get { return _keysQueue.ToArray(); }
        }

        public SpriteAnimator(ISpriteSheet spriteSheet, SpriteTransformer transformer)
        {
            _spriteSheet = spriteSheet;
            _transformer = transformer;

            Animations = new Dictionary<object, SpriteAnimation>();
            _keysQueue = new Queue<object>();
            _period = new Period();
        }

        public void ChangeAnimation(object key, SpriteAnimatorTransition transition = SpriteAnimatorTransition.Queued)
        {
            if (!Animations.ContainsKey(key))
                throw new KeyNotFoundException();

            switch (transition)
            {
                case SpriteAnimatorTransition.Instant:
                    ChangeAnimationInstant(key);
                    break;
                case SpriteAnimatorTransition.Queued:
                    _keysQueue.Enqueue(key);
                    break;
            }
        }

        private void ChangeAnimationInstant(object key)
        {
            CurrentKey = key;
            CurrentAnimation = Animations[key];

            CurrentStep = 0;
            RefreshStep();

            Ended = false;
        }

        private void RefreshStep()
        {
            CurrentFrame = CurrentAnimation.Frames[CurrentStep];
            _period.Interval = CurrentAnimation.Intervals[CurrentStep];
            _transformer.SourceRectangle = _spriteSheet[CurrentFrame];
        }

        public void Update(ElapsedTime elapsedTime)
        {
            if (!Enabled || Ended)
                return;

            if (!_period.Update(elapsedTime.GameTime))
                return;

            CurrentStep++;

            if (CurrentStep >= CurrentAnimation.StepsCount)
            {
                if (_keysQueue.Any())
                    ChangeAnimationInstant(_keysQueue.Dequeue());
                else if (CurrentAnimation.Loop)
                    CurrentStep = 0;
                else
                {
                    CurrentStep--;
                    Ended = true;
                    return;
                }
            }

            RefreshStep();
        }
    }
}