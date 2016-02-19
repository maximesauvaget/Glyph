﻿using System;
using System.Collections.Generic;
using Glyph.Math;
using Microsoft.Xna.Framework;

namespace Glyph.Composition
{
    [SinglePerParent]
    public class SceneNode : GlyphComponent, ISceneNode
    {
        private readonly List<ISceneNode> _childrenNodes;
        private readonly IReadOnlyList<ISceneNode> _readOnlyChildrenNodes;
        private ISceneNode _parentNode;
        private Transformation _transformation;
        private Vector2 _position;
        private float _rotation;
        private float _scale;
        private float _localDepth;
        private float _depth;
        public ISceneNode ParentNode { get; private set; }
        public Matrix3X3 Matrix { get; private set; }

        public IEnumerable<ISceneNode> Children
        {
            get { return _readOnlyChildrenNodes; }
        }

        public Transformation Transformation
        {
            get { return _transformation; }
            set
            {
                _transformation = value;
                Refresh(Referential.Local);
            }
        }

        public Vector2 LocalPosition
        {
            get { return Transformation.Translation; }
            set
            {
                Transformation.Translation = value;
                Refresh(Referential.Local);
            }
        }

        public float LocalRotation
        {
            get { return Transformation.Rotation; }
            set
            {
                Transformation.Rotation = value;
                Refresh(Referential.Local);
            }
        }

        public float LocalScale
        {
            get { return Transformation.Scale; }
            set
            {
                Transformation.Scale = value;
                Refresh(Referential.Local);
            }
        }

        public float LocalDepth
        {
            get { return _localDepth; }
            set
            {
                _localDepth = value;
                Refresh(Referential.Local);
            }
        }

        public Matrix3X3 LocalMatrix
        {
            get { return Transformation.Matrix; }
        }

        public Vector2 Position
        {
            get { return _position; }
            set
            {
                if (ParentNode != null)
                    LocalPosition = ParentNode.Matrix.Inverse * value;
                else
                    LocalPosition = value;
            }
        }

        public float Rotation
        {
            get { return _rotation; }
            set
            {
                if (ParentNode != null)
                    LocalRotation = value - ParentNode.Rotation;
                else
                    LocalRotation = value;
            }
        }

        public float Scale
        {
            get { return _scale; }
            set
            {
                if (ParentNode != null)
                    LocalScale = value / ParentNode.Scale;
                else
                    LocalScale = value;
            }
        }

        public float Depth
        {
            get { return _depth; }
            set
            {
                if (ParentNode != null)
                    LocalDepth = value - ParentNode.Depth;
                else
                    LocalDepth = value;
            }
        }

        public event Action<SceneNode> Refreshed;

        public SceneNode()
        {
            _childrenNodes = new List<ISceneNode>();
            _readOnlyChildrenNodes = _childrenNodes.AsReadOnly();

            Transformation = Transformation.Identity;
        }

        public SceneNode(ISceneNode parentNode)
            : this()
        {
            ParentNode = parentNode;
        }

        public override void Initialize()
        {
            if (Parent != null && Parent.Parent != null)
                ParentNode = Parent.Parent.GetComponent<SceneNode>();
        }

        public override void Dispose()
        {
            foreach (ISceneNode childNode in _childrenNodes)
                childNode.SetParent(null);
        }

        public void SetParent(ISceneNode parent, Referential childStaticReferential = Referential.World)
        {
            if (_parentNode != null)
                _parentNode.UnlinkChild(this);

            _parentNode = parent;

            if (_parentNode != null)
                _parentNode.LinkChild(this);

            Refresh(childStaticReferential);
        }

        void ISceneNode.LinkChild(ISceneNode child, Referential childStaticReferential = Referential.World)
        {
            if (_childrenNodes.Contains(child))
                throw new InvalidOperationException("Parent already have this SceneNode as a child !");

            if (child.ParentNode != this)
                child.SetParent(this, childStaticReferential);
            else
                _childrenNodes.Add(child);
        }

        void ISceneNode.UnlinkChild(ISceneNode child, Referential childStaticReferential = Referential.World)
        {
            _childrenNodes.Remove(child);
        }

        private void Refresh(Referential childStaticReferential)
        {
            if (ParentNode == null)
            {
                _position = LocalPosition;
                _rotation = LocalRotation;
                _scale = LocalScale;
                _depth = LocalDepth;
                Matrix = LocalMatrix;
            }
            else
            {
                if (childStaticReferential == Referential.Local)
                {
                    _position = ParentNode.Transformation.Matrix * LocalPosition;
                    _rotation = ParentNode.Rotation + LocalRotation;
                    _scale = ParentNode.Scale * LocalScale;
                    _depth = ParentNode.Depth + LocalDepth;
                }
                else if (childStaticReferential == Referential.World)
                {
                    _transformation = new Transformation(ParentNode.Transformation.Matrix.Inverse * _position, _rotation - ParentNode.Rotation, _scale / ParentNode.Scale);
                    _localDepth = _depth - ParentNode.Depth;
                }
                Matrix = ParentNode.Matrix * LocalMatrix;
            }

            foreach (ISceneNode childNode in _childrenNodes)
                childNode.Refresh();

            if (Refreshed != null)
                Refreshed.Invoke(this);
        }

        void ISceneNode.Refresh()
        {
            Refresh(Referential.Local);
        }
    }
}