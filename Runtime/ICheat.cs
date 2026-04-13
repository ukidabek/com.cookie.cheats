namespace cookie.Cheats
{
    public interface ICheat
    {
        int ID { get; }
        string Name { get; }
        public CheatData[] Attributes { get; }
    }
}