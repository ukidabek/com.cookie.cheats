namespace cookie.Cheats
{
    public interface IValueCheat
    {
        bool IsDirty { get; }
        object GetSerialized();
    }
}