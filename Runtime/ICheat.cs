namespace cookie.Cheats
{
    public interface ICheat
    {
        int ID { get; }
        string Name { get; }
        bool IsDirty { get; }
        CheatAttributeData[] Attributes { get; }
        CheatData ToDataTransferObject();
    }
}