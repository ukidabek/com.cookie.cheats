using System;

namespace cookie.Cheats
{
    public interface IEditorCheatBuilder
    {
        Type Type { get; }
        IEditorCheat Build(CheatData data);
    }
}