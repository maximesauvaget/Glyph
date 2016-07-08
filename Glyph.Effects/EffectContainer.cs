﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stave;

namespace Glyph.Effects
{
    public class EffectContainer : EffectContainer<IEffectComponent>, IEffectContainer
    {
        protected EffectContainer()
        {
        }
    }

    public class EffectContainer<TComponent> : Container<IEffectComponent, IEffectParent, TComponent>, IEffectContainer<TComponent>
        where TComponent : class, IEffectComponent
    {
        public bool Enabled { get; set; }

        protected EffectContainer()
        {
        }

        public void Initialize()
        {
            foreach (TComponent component in this)
                component.Initialize();
        }

        public void LoadContent(ContentLibrary contentLibrary, GraphicsDevice graphicsDevice)
        {
            foreach (TComponent component in this)
                component.LoadContent(contentLibrary, graphicsDevice);
        }

        public void Update(GameTime gameTime)
        {
            foreach (TComponent component in this)
                component.Update(gameTime);
        }

        public void Prepare(IDrawer drawer)
        {
            foreach (TComponent component in this)
                component.Prepare(drawer);
        }

        public void Apply(IDrawer drawer)
        {
            throw new InvalidOperationException();
        }

        public void Dispose()
        {
            foreach (TComponent component in this)
                component.Dispose();
        }
    }
}