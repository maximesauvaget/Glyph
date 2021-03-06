﻿using Glyph.Messaging;

namespace Glyph.Scripting
{
    public abstract class TriggerMessageBase : HandlerMessage<object>
    {
        public Trigger Trigger { get; private set; }
        public Actor Actor { get; private set; }

        protected TriggerMessageBase(Trigger trigger, Actor actor)
        {
            Trigger = trigger;
            Actor = actor;
        }
    }
}