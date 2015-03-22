﻿using System.Collections;
using System.Collections.Generic;

namespace Glyph.Xml
{
    public class XmlCollection : IEnumerable
    {
        private readonly Dictionary<string, string> _elements;

        public int Count
        {
            get { return _elements.Count; }
        }

        public XmlCollection()
        {
            _elements = new Dictionary<string, string>();
        }

        public string this[string key]
        {
            get { return _elements[key]; }
            set { _elements[key] = value; }
        }

        public void Add(string name, string value)
        {
            _elements.Add(name, value);
        }

        public void Add<T>(string name, T value)
        {
            _elements.Add(name, value.ToString<T>());
        }

        public string Get(string name)
        {
            return _elements[name];
        }

        public T Get<T>(string name)
        {
            try
            {
                return typeof(T).IsEnum ? _elements[name].ParseEnum<T>() : _elements[name].Parse<T>();
            }
            catch (KeyNotFoundException)
            {
                return default(T);
            }
        }

        public void Clear()
        {
            _elements.Clear();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _elements.GetEnumerator();
        }
    }
}