using System;

namespace SmartConfig
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SettingSetupAttribute : Attribute
    {
        public string OverrideToken { get; set; }
        public string OverrideAttributeName { get; set; }
    }
}