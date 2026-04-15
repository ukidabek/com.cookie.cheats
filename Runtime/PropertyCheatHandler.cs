using System.Reflection;
using UnityEngine.Scripting;

namespace cookie.Cheats.Server
{
    [Preserve]
    public class PropertyCheatHandler : ValueCheatHandler<PropertyCheat, PropertyInfo>
    {
    }
}