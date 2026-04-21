namespace cookie.Cheats
{
    public interface IValueCheat
    {
        bool IsDirty { get; }
        object ToSerializableObject();
        void MartAsDirty();
    }
}