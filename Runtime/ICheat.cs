namespace cookie.Cheats
{
    public interface ICheat
    {
        int ID { get; }
        string Name { get; }
        CheatAttributeData[] Attributes { get; }
        CheatData ToDataTransferObject();
    }
}