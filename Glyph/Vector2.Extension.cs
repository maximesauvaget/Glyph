﻿using System;
using Microsoft.Xna.Framework;

namespace Glyph
{
    static public class Vector2Extension
    {
        static public Vector2 Normalized(this Vector2 value)
        {
            if (value == Vector2.Zero)
                return value;

            Vector2 temp = value;
            temp.Normalize();
            return temp;
        }

        static public Vector3 ToVector3(this Vector2 value)
        {
            return new Vector3(value.X, value.Y, 0);
        }

        static public float ToRotation(this Vector2 value)
        {
            Vector2 norm = Vector2.Normalize(value);
            double result = System.Math.Atan2(norm.Y, norm.X);

            result += MathHelper.TwoPi;
            result %= MathHelper.TwoPi;

            return (float)result;
        }

        static public Vector2 SetX(this Vector2 value, float x)
        {
            return new Vector2(x, value.Y);
        }

        static public Vector2 SetY(this Vector2 value, float y)
        {
            return new Vector2(value.X, y);
        }

        static public Vector2 VectorX(this Vector2 value)
        {
            return new Vector2(value.X, 0);
        }

        static public Vector2 VectorY(this Vector2 value)
        {
            return new Vector2(0, value.Y);
        }

        static public Vector2 Add(this Vector2 value, float v)
        {
            return new Vector2(value.X + v, value.Y + v);
        }

        static public Vector2 Add(this Vector2 value, float x, float y)
        {
            return new Vector2(value.X + x, value.Y + y);
        }

        static public Vector2 Substract(this Vector2 value, float v)
        {
            return new Vector2(value.X - v, value.Y - v);
        }

        static public Vector2 Substract(this Vector2 value, float x, float y)
        {
            return new Vector2(value.X - x, value.Y - y);
        }

        static public Vector2 Multiply(this Vector2 value, float x, float y)
        {
            return new Vector2(value.X * x, value.Y * y);
        }

        static public Vector2 Divide(this Vector2 value, float x, float y)
        {
            return new Vector2(value.X / x, value.Y / y);
        }

        static public Vector2 Inverse(this Vector2 value)
        {
            return new Vector2(-value.X, -value.Y);
        }

        static public Vector2 RotateToward(this Vector2 value, Vector2 directionGoal, float angularSpeed, float deltaTime)
        {
            float angle = value.ToRotation();
            float angleGoal = directionGoal.ToRotation();

            float newAngle = angle.Transition(angleGoal, angularSpeed, deltaTime);

            return new Vector2((float)System.Math.Cos(newAngle), (float)System.Math.Sin(newAngle));
        }

        static public Point Discretize(this Vector2 value, Vector2 localPoint)
        {
            return new Point((int)(localPoint.X / value.X), (int)(localPoint.Y / value.Y));
        }

        static public Vector2 Integrate(this Vector2 value, Point gridPoint)
        {
            return new Vector2(gridPoint.X * value.X, gridPoint.Y * value.Y);
        }
    }
}