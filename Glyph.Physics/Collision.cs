﻿using Glyph.Physics.Colliders;
using Microsoft.Xna.Framework;

namespace Glyph.Physics
{
    public class Collision
    {
        public ICollider Sender { get; set; }
        public ICollider OtherCollider { get; set; }
        public Vector2 Correction { get; set; }
        public Vector2 NewPosition { get; set; }
        public bool IsCancelled { get; private set; }

        public void Cancel()
        {
            IsCancelled = true;
        }
    }
}