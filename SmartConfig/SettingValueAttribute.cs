using System;

namespace SmartConfig
{
    /// <summary>
    /// Attribute class for identifying the setting element holder for <see cref="SmartConfigSectionHandler"/>. Every field or property which is reflected in config should have this attribute specified to be handled by <see cref="SingleTagConfigSection"/> class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SettingValueAttribute : SettingAttributeBase
    {
        private string _defaultValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingValueAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of config section attribute which should be parsed.</param>
        public SettingValueAttribute(string name) : base(name)
        {
        }

        /// <summary>
        /// Optional value which should be used as a default (i.e. when config section does not have such an attribute). Its value should be convertible from <see cref="string"/> to destination type.
        /// </summary>
        /// <value>The default value for property or field.</value>
        public string DefaultValue
        {
            get { return _defaultValue; }
            set
            {
                _defaultValue = value;
                DefaultValueSpecified = true;
            }
        }

        internal bool DefaultValueSpecified { get; set; }
    }
}