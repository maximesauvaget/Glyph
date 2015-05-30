﻿using System;
using Microsoft.Xna.Framework;

namespace Glyph.Input.Handlers.Cursors
{
    public abstract class CursorHandler : SensitiveHandler<Vector2>
    {
        private Vector2 _previousState;
        public CursorSpace CursorSpace { get; set; }
        public override Vector2 Value { get; protected set; }

        protected CursorHandler(string name, CursorSpace cursorSpace, InputActivity desiredActivity)
            : base(name, desiredActivity)
        {
            CursorSpace = cursorSpace;
        }

        protected abstract Vector2 GetState(InputStates inputStates);

        protected override void HandleInput(InputStates inputStates)
        {
            Vector2 state = GetState(inputStates);

            Vector2 position;
            switch (CursorSpace)
            {
                case CursorSpace.Window:
                    position = state;
                    break;
                case CursorSpace.Screen:
                    position = state - Resolution.WindowMargin;
                    break;
                case CursorSpace.VirtualScreen:
                    position = (state - Resolution.WindowMargin) / Resolution.ScaleRatio;
                    break;
                case CursorSpace.Scene:
                    position = (state - Resolution.WindowMargin) / (Resolution.ScaleRatio * Camera.Zoom)
                               + Camera.VectorPosition.XY();
                    break;
                default:
                    throw new NotImplementedException();
            }

            Value = position;
            IsActivated = state != _previousState;

            _previousState = state;
        }
    }
}