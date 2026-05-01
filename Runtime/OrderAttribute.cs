using System;

namespace cookie.Cheats
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class)]

    public class OrderAttribute : Attribute
    {
        public readonly int Order;
        
        public OrderAttribute(int order)
        {
            Order = order;
        }
    }
}