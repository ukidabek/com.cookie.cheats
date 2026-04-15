using System.Reflection;
using UnityEngine.Scripting;

namespace cookie.Cheats.Server
{
    [Preserve]
    public class FieldCheatHandler : ValueCheatHandler<FieldCheat, FieldInfo>
    {
    }
}