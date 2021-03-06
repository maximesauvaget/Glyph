﻿using System;
using System.Collections.Generic;
using Diese.Collections;
using Glyph.Composition;
using Simulacra;

namespace Glyph.Core.Tracking
{
    public class Factory<T> : Tracker<T>, ICreator<T>
        where T : class, IGlyphComponent
    {
        private readonly IGlyphCompositeResolver _parent;
        public event Action<T> ComponentCreated;

        public Factory(IGlyphCompositeResolver parent)
        {
            _parent = parent;
        }

        public void Init(IEnumerable<T> items)
        {
            Clear();

            foreach (T item in items)
                Register(item);
        }

        public T Create()
        {
            var item = _parent.Add<T>();
            ComponentCreated?.Invoke(item);

            base.Register(item);
            return item;
        }

        public override void Register(T item)
        {
            _parent.Add(item);
            base.Register(item);
        }

        public override bool Unregister(T item)
        {
            _parent.Remove(item);
            return base.Unregister(item);
        }

        public override void Clear()
        {
            foreach (T item in this)
                _parent.Remove(item);

            base.Clear();
        }
    }
}