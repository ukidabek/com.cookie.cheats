using System;

namespace cookie.Cheats
{
    public interface IValueCheat
    {
        bool IsDirty { get; }
        public Type ValueType { get; }
        object ToSerializableObject();
        void MartAsDirty();
    }
}