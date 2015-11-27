﻿using Glyph.Animation;
using Glyph.Math;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Glyph.Graphics
{
    public class FillingRenderer : Renderer
    {
        private readonly FillingRectangle _fillingRectangle;

        public FillingRenderer(FillingRectangle fillingRectangle, ISpriteSource source, SceneNode sceneNode)
            : base(source, sceneNode)
        {
            _fillingRectangle = fillingRectangle;
        }

        protected override void Render(SpriteBatch spriteBatch)
        {
            if (SpriteTransformer != null)
                spriteBatch.Draw(Source.Texture, _fillingRectangle.Rectangle.ToStruct(), Source.Rectangle, SpriteTransformer.Color,
                    SceneNode.Rotation, SpriteTransformer.Origin, SpriteTransformer.Effects, 0);
            else
                spriteBatch.Draw(Source.Texture, _fillingRectangle.Rectangle.ToStruct(), Source.Rectangle, Color.White,
                    SceneNode.Rotation, Vector2.Zero, SpriteEffects.None, 0);
        }
    }
}