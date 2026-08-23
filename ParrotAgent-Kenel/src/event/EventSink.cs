using System;

namespace ParrotAgent.Kenel
{
    public class EventSink
    {
        public IEventChannel Input { get; } = new EventChannel();
        public IEventChannel Output { get; } = new EventChannel();
    }
}
