using System;
using System.Collections.Generic;
using UnityEngine;

namespace cookie.Cheats
{
    public static class TypeGroups
    {
        public static readonly IReadOnlyDictionary<Type, int> AxisCountDictionary = new Dictionary<Type, int>(new KeyValuePair<Type, int>[]
        {
            new KeyValuePair<Type, int>(typeof(Vector2), 2),
            new KeyValuePair<Type, int>(typeof(Vector2Int), 2),
            new KeyValuePair<Type, int>(typeof(Vector3), 3),
            new KeyValuePair<Type, int>(typeof(Vector3Int), 3),
            new KeyValuePair<Type, int>(typeof(Vector4), 4),
        });
        
        public static readonly HashSet<Type> NumericTypes = new()
        {
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal)
        };
        
        public static readonly HashSet<Type> WholeNumberTypes = new()
        {
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong)
        };
        
        public static readonly HashSet<Type> VectorTypes = new()
        {
            typeof(Vector2),
            typeof(Vector2Int),
            typeof(Vector3),
            typeof(Vector3Int),
            typeof(Vector4),
        };
        
        public static readonly HashSet<Type> WholeNumberVectorTypes = new()
        {
            typeof(Vector2Int),
            typeof(Vector3Int),
        };
        
        public static readonly HashSet<Type> MultiValueTypes = new()
        {
            typeof(Vector2),
            typeof(Vector3),
            typeof(Vector4),
            typeof(Vector2Int),
            typeof(Vector3Int),
            typeof(Quaternion),
            typeof(Color),
            typeof(Color32),
            typeof(Rect),
            typeof(Bounds),
            typeof(BoundsInt),
            typeof(RectInt),
            typeof(Matrix4x4),
            typeof(Plane),
            typeof(Ray),
            typeof(Ray2D),
            typeof(LayerMask),
            typeof(AnimationCurve),
            typeof(Gradient),
            typeof(Keyframe),
            typeof(Resolution),
            typeof(ShadowResolution),
        };
    }
}