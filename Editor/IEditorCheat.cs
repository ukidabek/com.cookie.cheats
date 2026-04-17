using System;
using cookie.Cheats.Server;

namespace cookie.Cheats
{
    public interface IEditorCheat : ICheat
    {
        event Action<CheatPayload> Update;
        void OnGUI();
        void SetValue(object value);
    }
}