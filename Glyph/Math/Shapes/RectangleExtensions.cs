﻿using Microsoft.Xna.Framework;

namespace Glyph.Math.Shapes
{
    static public class RectangleExtensions
    {
        static public Vector2 NormalizedCoordinates(this Vector2 point, TopLeftRectangle rectangle)
        {
            return (point - rectangle.Position) / rectangle.Size;
        }

        static public Vector2 Scale(this Vector2 point, TopLeftRectangle rectangle)
        {
            return point * rectangle.Size + rectangle.Position;
        }

        static public Vector2 Rescale(this Vector2 point, TopLeftRectangle oldRectangle, TopLeftRectangle newRectangle)
        {
            return point.NormalizedCoordinates(oldRectangle).Scale(newRectangle);
        }

        static public CenteredRectangle Scale(this CenteredRectangle rectangle, float scale)
        {
            return new CenteredRectangle(rectangle.Center, rectangle.Size * scale);
        }

        static public Vector2 ClampToRectangle(this Vector2 point, TopLeftRectangle rectangle)
        {
            if (point.X < rectangle.Left)
                point.X = rectangle.Left;
            if (point.X > rectangle.Right)
                point.X = rectangle.Right;
            if (point.Y < rectangle.Top)
                point.Y = rectangle.Top;
            if (point.Y > rectangle.Bottom)
                point.Y = rectangle.Bottom;

            return point;
        }

        static public TopLeftRectangle ClampToRectangle(this TopLeftRectangle inner, TopLeftRectangle outer)
        {
            if (inner.Left < outer.Left)
            {
                inner.Width -= outer.Left - inner.Left;
                inner.Left = outer.Left;
            }
            if (inner.Right > outer.Right)
                inner.Width -= inner.Right - outer.Right;

            if (inner.Top < outer.Top)
            {
                inner.Height -= outer.Top - inner.Top;
                inner.Top = outer.Top;
            }
            if (inner.Bottom > outer.Bottom)
                inner.Height -= inner.Bottom - outer.Bottom;

            return inner;
        }

        static public TopLeftRectangle EncaseRectangle(this TopLeftRectangle inner, TopLeftRectangle outer)
        {
            if (inner.Left < outer.Left)
                inner.Left += outer.Left - inner.Left;
            else if (inner.Right > outer.Right)
                inner.Left -= inner.Right - outer.Right;

            if (inner.Top < outer.Top)
                inner.Top += outer.Top - inner.Top;
            else if (inner.Bottom > outer.Bottom)
                inner.Top -= inner.Bottom - outer.Bottom;

            return inner;
        }
    }
}