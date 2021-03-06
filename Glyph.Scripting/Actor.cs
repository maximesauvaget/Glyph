﻿using System.Collections.Generic;
using System.Linq;
using Diese.Collections;
using Glyph.Composition;
using Glyph.Core.Colliders;
using Glyph.Math;
using Glyph.Math.Shapes;
using Microsoft.Xna.Framework;

namespace Glyph.Scripting
{
    public class Actor : GlyphContainer, IUpdate, IArea
    {
        private readonly TriggerManager _triggerManager;
        private readonly List<Trigger> _activatedTriggers;
        private TopLeftRectangle _boudingBox;
        private bool _dirtyBoundingBox = true;
        public List<ICollider> Colliders { get; private set; }
        public bool IsVoid => _boudingBox.IsVoid;

        public TopLeftRectangle BoundingBox
        {
            get
            {
                if (_dirtyBoundingBox)
                    _boudingBox = MathUtils.GetBoundingBox(Colliders);
                return _boudingBox;
            }
        }

        public Actor(TriggerManager triggerManager)
        {
            _triggerManager = triggerManager;

            _activatedTriggers = new List<Trigger>();
            Colliders = new List<ICollider>();
        }

        public void Update(ElapsedTime elapsedTime)
        {
            IEnumerable<Trigger> newActivatedTriggers = _triggerManager.Space.GetAllItemsInRange(BoundingBox).Where(trigger => Colliders.Any(collider => collider.Intersects(trigger.Shape))).ToArray();

            IEnumerable<Trigger> enteredTriggers, leavedTriggers;
            if (!_activatedTriggers.SetDiff(newActivatedTriggers, out enteredTriggers, out leavedTriggers))
                return;

            foreach (Trigger trigger in leavedTriggers)
                trigger.Leave(this);

            foreach (Trigger trigger in enteredTriggers)
                trigger.Enter(this);

            _activatedTriggers.Clear();
            _activatedTriggers.AddRange(newActivatedTriggers);
        }

        public bool ContainsPoint(Vector2 point) => Colliders.Any(x => x.ContainsPoint(point));
        public bool Intersects(Segment segment) => Colliders.Any(x => x.Intersects(segment));
        public bool Intersects<T>(T edgedShape) where T : IEdgedShape => Colliders.Any(x => x.Intersects(edgedShape));
        public bool Intersects(Circle circle) => Colliders.Any(x => x.Intersects(circle));
    }
}