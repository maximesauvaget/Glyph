﻿using System;
using System.Collections;
using System.Collections.Generic;
using Glyph.Math.Shapes;
using Microsoft.Xna.Framework;

namespace Glyph.Space
{
    public class DirtableGrid<T> : IDirtableGrid<T>
        where T : class, IDirtable
    {
        private readonly IWriteableGrid<T> _gridImplementation;
        private readonly List<IGridCase<T>> _dirtiedCases;
        private readonly IReadOnlyList<IGridCase<T>> _readOnlyDirtiedCases;
        private bool _isDirty;

        public bool IsDirty
        {
            get { return _isDirty; }
            private set
            {
                _isDirty = value;

                if (_isDirty && Dirtied != null)
                    Dirtied.Invoke();
            }
        }

        public IEnumerable<IGridCase<T>> DirtiedCases
        {
            get { return _readOnlyDirtiedCases; }
        }

        public IRectangle BoundingBox
        {
            get { return _gridImplementation.BoundingBox; }
        }

        public Vector2 Center
        {
            get { return _gridImplementation.Center; }
            set { _gridImplementation.Center = value; }
        }

        public GridDimension Dimension
        {
            get { return _gridImplementation.Dimension; }
        }

        public Vector2 Delta
        {
            get { return _gridImplementation.Delta; }
        }

        public Func<T> OutOfBoundsValueFactory
        {
            get { return _gridImplementation.OutOfBoundsValueFactory; }
        }

        public bool HasLowEntropy
        {
            get { return _gridImplementation.HasLowEntropy; }
        }

        public IEnumerable<T> Values
        {
            get { return _gridImplementation.Values; }
        }

        public IEnumerable<IGridCase<T>> SignificantCases
        {
            get { return _gridImplementation.SignificantCases; }
        }

        public T this[int i, int j]
        {
            get { return _gridImplementation[i, j]; }
            set
            {
                _gridImplementation[i, j] = value;

                value.Dirtied += () => IsDirty = true;
                _dirtiedCases.Add(new GridCase<T>(i, j, value));
            }
        }

        public T this[Point gridPoint]
        {
            get { return _gridImplementation[gridPoint]; }
            set
            {
                _gridImplementation[gridPoint] = value;

                value.Dirtied += () => IsDirty = true;
                _dirtiedCases.Add(new GridCase<T>(gridPoint, value));
            }
        }

        public T this[Vector2 worldPoint]
        {
            get { return _gridImplementation[worldPoint]; }
            set
            {
                _gridImplementation[worldPoint] = value;

                value.Dirtied += () => IsDirty = true;
                _dirtiedCases.Add(new GridCase<T>(ToGridPoint(worldPoint), value));
            }
        }

        public T this[IGridPositionable gridPositionable]
        {
            get { return _gridImplementation[gridPositionable]; }
            set
            {
                _gridImplementation[gridPositionable] = value;

                value.Dirtied += () => IsDirty = true;
                _dirtiedCases.Add(new GridCase<T>(gridPositionable.GridPosition, value));
            }
        }

        public T[][] ToArray()
        {
            return _gridImplementation.ToArray();
        }

        T IGrid<T>.this[int i, int j]
        {
            get { return ((IGrid<T>)_gridImplementation)[i, j]; }
        }

        T IGrid<T>.this[Point gridPoint]
        {
            get { return ((IGrid<T>)_gridImplementation)[gridPoint]; }
        }

        T IGrid<T>.this[Vector2 worldPoint]
        {
            get { return ((IGrid<T>)_gridImplementation)[worldPoint]; }
        }

        T IGrid<T>.this[IGridPositionable gridPositionable]
        {
            get { return ((IGrid<T>)_gridImplementation)[gridPositionable]; }
        }

        public event Action Dirtied;

        public DirtableGrid(IWriteableGrid<T> gridImplementation)
        {
            _gridImplementation = gridImplementation;

            foreach (Point point in _gridImplementation)
                _gridImplementation[point].Dirtied += () => IsDirty = true;

            _dirtiedCases = new List<IGridCase<T>>();
            _readOnlyDirtiedCases = _dirtiedCases.AsReadOnly();
        }

        public void Clean()
        {
            IsDirty = false;
            _dirtiedCases.Clear();
        }

        public bool ContainsPoint(Vector2 point)
        {
            return _gridImplementation.ContainsPoint(point);
        }

        public bool Intersects(IRectangle rectangle)
        {
            return _gridImplementation.Intersects(rectangle);
        }

        public bool Intersects(ICircle circle)
        {
            return _gridImplementation.Intersects(circle);
        }

        public Vector2 ToWorldPoint(int i, int j)
        {
            return _gridImplementation.ToWorldPoint(i, j);
        }

        public Vector2 ToWorldPoint(Point gridPoint)
        {
            return _gridImplementation.ToWorldPoint(gridPoint);
        }

        public Vector2 ToWorldPoint(IGridPositionable gridPoint)
        {
            return _gridImplementation.ToWorldPoint(gridPoint);
        }

        public Point ToGridPoint(Vector2 worldPoint)
        {
            return _gridImplementation.ToGridPoint(worldPoint);
        }

        public bool ContainsPoint(Point gridPoint)
        {
            return _gridImplementation.ContainsPoint(gridPoint);
        }

        public IEnumerator<Point> GetEnumerator()
        {
            return _gridImplementation.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_gridImplementation).GetEnumerator();
        }
    }
}