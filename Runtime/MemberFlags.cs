using System;

namespace cookie.Cheats
{
    [Flags]
    public enum MemberFlags : byte
    {
        None          = 0,
        IsNumeric     = 1 << 0, // 0000 0001
        IsWholeNumber = 1 << 1, // 0000 0010
        CanRead       = 1 << 2, // 0000 0100
        CanWrite      = 1 << 3, // 0000 1000
        IsEnum        = 1 << 4 , // 0001 0000
        IsMultipleValue = 1 << 5,// 0010 000
    }
}