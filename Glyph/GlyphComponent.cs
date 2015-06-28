﻿using Diese.Composition;

namespace Glyph
{
    public class GlyphComponent : Component<IGlyphComponent, IGlyphParent>, IGlyphComponent
    {
        public virtual void Initialize()
        {
        }

        public virtual void Dispose()
        {
        }
    }
}