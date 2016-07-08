﻿using Stave;

namespace Glyph.Composition
{
    public interface IGlyphComposite : IGlyphComposite<IGlyphComponent>
    {
    }

    public interface IGlyphComposite<TComponent> : IComposite<IGlyphComponent, IGlyphParent, TComponent>, IGlyphParent
        where TComponent : class, IGlyphComponent
    {
    }
}