using System;

namespace SmartConfig
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SettingClassAttribute : SettingAttributeBase
    {
        public SettingClassAttribute(string name)
            : base(name)
        {
            IsRequired = true;
        }

        public Type Type { get; set; }

        public bool IsRequired { get; set; }
    }
}