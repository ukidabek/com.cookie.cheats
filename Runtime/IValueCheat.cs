namespace cookie.Cheats
{
    public interface IValueCheat
    {
        bool IsDirty { get; }
        object Get();
        void Set(object value);
    }
}