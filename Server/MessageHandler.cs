using System;

namespace cookie.Cheats.Server
{
    public abstract class MessageHandler
    {
        protected readonly CheatServer m_cheatServer;
        public abstract int Id { get; }
        public abstract Message Handle(object payload);

        public MessageHandler(CheatServer cheatServer)
        {
            m_cheatServer = cheatServer;
        }
    }

    public abstract class MessageHandler<T> : MessageHandler
    {
        public MessageHandler(CheatServer cheatServer) : base(cheatServer)
        {
        }

        public override Message Handle(object payload)
        {
            if (payload is T typed)
                return Handle(typed);

            throw new InvalidCastException(
                $"Handler for ID {Id} expected payload of type {typeof(T).Name}, " +
                $"but received {payload?.GetType().Name ?? "null"}."
            );
        }

        protected abstract Message Handle(T payload);
    }
}