﻿using Diese.Injection;

namespace Glyph.Composition.Injection
{
    public class GlyphInjectableAttribute : InjectableAttribute
    {
        public GlyphInjectableTargets Targets { get; set; }

        public GlyphInjectableAttribute(GlyphInjectableTargets targets = GlyphInjectableTargets.All)
        {
            Targets = targets;
        }
    }
}