using System;

namespace SmartConfig
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SettingListAttribute : Attribute
    {
        public string NodeName { get; set; }
        public string ListName { get; set; }
        public Type ElementType { get; set; }
    }
}