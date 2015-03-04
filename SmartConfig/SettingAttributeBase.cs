using System;

namespace SmartConfig
{
    public class SettingAttributeBase : Attribute
    {
        public SettingAttributeBase(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the name of attribute in config section.
        /// </summary>
        /// <value>The name or source attribute.</value>
        public string Name { get; set; }

    }
}