using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Reflection;
using System.Xml;
using BS2000Common.Delegates;
using System.Collections.Generic;

namespace SmartConfig
{
    public class SmartConfigSectionHandler : IConfigurationSectionHandler
    {
        public static T GetSection<T>(string sectionPath, CultureInfo culture, Func<string, string, bool> overrideFilter = null ) where T : new()
        {
            XmlNode sectionNode = ConfigurationManager.GetSection(sectionPath) as XmlNode;

            if (sectionNode == null)
            {
                throw new SmartConfigException("Specified section cannot be found in config: {0}", sectionPath);
            }

            return GetSection<T>(sectionNode, culture, overrideFilter);
        }

        private static T GetSection<T>(XmlNode sectionNode, CultureInfo culture, Func<string, string, bool> overrideFilter = null) where T : new()
        {
            return (T)GetSection(sectionNode, typeof(T), culture, overrideFilter);
        }

        private static object GetSection(XmlNode sectionNode, Type elementType, CultureInfo culture, Func<string, string, bool> overrideFilter = null)
        {
            object result = Activator.CreateInstance(elementType);
            SettingSetupAttribute sectionSettingsSetup = GetAttribute<SettingSetupAttribute>(result);
            foreach (MemberInfo fieldsInfo in 
                result.GetType().FindMembers(MemberTypes.Property | MemberTypes.Field, 
                                             BindingFlags.Instance | BindingFlags.Public, 
                                             Type.FilterName, "*"))
            {
                SettingValueAttribute settingValue = GetAttribute<SettingValueAttribute>(fieldsInfo);
                if (settingValue != null)
                {
                    ReadSimpleValueFromAttributes(sectionNode, result, settingValue, fieldsInfo,
                                                  sectionSettingsSetup, overrideFilter, culture);
                }

                SettingClassAttribute settingClass = GetAttribute<SettingClassAttribute>(fieldsInfo);
                if (settingClass != null)
                {
                    ReadComplexTypeFromValue(sectionNode, result, settingClass, fieldsInfo, culture,
                                             overrideFilter);
                }

                SettingListAttribute listSettings = GetAttribute<SettingListAttribute>(fieldsInfo);
                if (listSettings != null)
                {
                    ReadList(result, sectionNode, listSettings, fieldsInfo, culture, overrideFilter);
                }
            }

            return result;
        }

        private static void ReadComplexTypeFromValue(XmlNode sectionNode, 
            object result, 
            SettingClassAttribute settingClass, 
            MemberInfo fieldsInfo, 
            CultureInfo culture,
            Func<string, string, bool> overrideFilter)
        {
            foreach (XmlNode node in sectionNode.ChildNodes)
            {
                if (!node.Name.Equals(settingClass.Name))
                {
                    continue;
                }
                object innerValue = GetSection(node, settingClass.Type, culture, overrideFilter);
                SetFieldValue(result, fieldsInfo, innerValue);
                return;
            }
            if (settingClass.IsRequired)
            {
                throw new SmartConfigException("Required tag is missing in config: {0}",
                               settingClass.Name);

            }
        }

        private static void ReadList(object target, 
            XmlNode sectionNode, 
            SettingListAttribute listSettings, 
            MemberInfo fieldsInfo, 
            CultureInfo culture,
            Func<string, string, bool> overrideFilter = null)
        {
            foreach (XmlNode childNode in sectionNode.ChildNodes)
            {
                if (!childNode.Name.Equals(listSettings.ListName))
                    continue;

                ArrayList resultList = new ArrayList();
                foreach (XmlNode itemNode in childNode.ChildNodes)
                {
                    if (itemNode.NodeType == XmlNodeType.Comment)
                        continue;
                    if (!itemNode.Name.Equals(listSettings.NodeName) )
                    {
                        throw new SmartConfigException("Unexpected item in the list: {0}/{1}", itemNode.Name, listSettings.NodeName);
                    }

                    object section = GetSection(itemNode, listSettings.ElementType, culture, overrideFilter );
                    resultList.Add(section);
                    SetFieldValue(target, fieldsInfo, resultList);
                }
            }
        }

        private static T GetAttribute<T>(object section) where T : Attribute
        {
            return GetAttribute<T>(section.GetType());
        }

        private static T GetAttribute<T>(MemberInfo memberInfo) where T : Attribute
        {
            object[] customAttributes = memberInfo.GetCustomAttributes(typeof(T), true);
            if (customAttributes.Length > 0)
                return (T)customAttributes[0];
            return null;
        }

        private static void ReadSimpleValueFromAttributes(XmlNode source,
                                                                           object target,
                                                                           SettingValueAttribute settingValue,
                                                                           MemberInfo fieldToPopulate,
                                                                           SettingSetupAttribute settingSetup,
                                                                           Func<string, string, bool> overrideFilter,
                                                                           CultureInfo culture)
        {
            //check if section supports overrides
            var overriddenValue = GetOverriddenValue(source, settingValue, settingSetup, overrideFilter);

            string valueFromSource;
            if (overriddenValue != null) //found overriden value. Enough for us
            {
                valueFromSource = overriddenValue.Value;
            }
            else //no override found
            {
                XmlNode originalValueNode = source.Attributes.GetNamedItem(settingValue.Name, string.Empty);
                if (originalValueNode != null) //found original node value. Enough at the moment
                {
                    valueFromSource = originalValueNode.Value;
                }
                else //no original value found in config
                {
                    if (settingValue.DefaultValue != null) //default value specified in section definition. Use it
                    {
                        valueFromSource = settingValue.DefaultValue;
                    }
                    else  //no override, original and default value found. Sounds like required attribute is missing. Bad
                    {
                        throw new SmartConfigException("Required attribute is missing in config: {0}",
                                                       settingValue.Name);
                    }
                }
            }

            SetFieldValue(target, fieldToPopulate, valueFromSource, culture);
        }

        private static XmlNode GetOverriddenValue(XmlNode source,
                                                    SettingValueAttribute settingValue,
                                                     SettingSetupAttribute settingSetup,
                                                    Func<string, string, bool> overrideFilter)
        {
            if (settingSetup == null)
                return null;
            if (overrideFilter == null)
                return null;
            XmlNode overriddenValue = null;

            if (!string.IsNullOrEmpty(settingSetup.OverrideToken))
            {
                foreach (XmlNode overrideNode in source.ChildNodes)
                {
                    if (!overrideNode.Name.Equals(settingSetup.OverrideToken))
                        continue;
                    if (overrideNode.Attributes == null)
                        continue;
                    XmlNode overrideFilterNode = overrideNode.Attributes.GetNamedItem(settingSetup.OverrideAttributeName,
                                                                                      string.Empty);
                    if (overrideFilterNode == null)
                        continue;

                    if (overrideFilter(settingSetup.OverrideToken, overrideFilterNode.Value))
                    {
                        overriddenValue = overrideNode.Attributes.GetNamedItem(settingValue.Name, string.Empty);
                    }
                }
            }
            return overriddenValue;
        }

        protected static void SetFieldValue(object target, MemberInfo fieldInfo, object value)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            if (fieldInfo.MemberType == MemberTypes.Property)
            {
                flags |= BindingFlags.SetProperty;
            }

            try
            {
                target.GetType().InvokeMember(fieldInfo.Name, flags, null, target, new[] { value });
            }
            catch (Exception ex)
            {
                throw new SmartConfigException("Cannot assign value to property/method", ex);
            }
        }

        public static void SetFieldValue(object target, MemberInfo fieldInfo, string settingValue, CultureInfo culture)
        {
            object fieldValue = ConvertSettingToFieldValue(settingValue, ((PropertyInfo)fieldInfo).PropertyType, culture);
            SetFieldValue(target, fieldInfo, fieldValue);           
        }

        private static object ConvertSettingToFieldValue(string settingValue, Type memberType, CultureInfo culture )
        {
            object fieldValue;
            // special case for Type property/field
            if (memberType == typeof(Type))
            {
                fieldValue = Type.GetType(settingValue, false);
                if (fieldValue == null)
                {
                    throw new SmartConfigException("Cannot convert from '{0}' to Type", settingValue);
                }
            }
            else
            {
                TypeConverter typeConverter = TypeDescriptor.GetConverter(memberType);
                if (!typeConverter.CanConvertFrom(typeof(string)))
                {
                    throw new SmartConfigException("Cannot convert value '{0}' to type {1}", settingValue, memberType);
                }

                fieldValue = typeConverter.ConvertFromString(null, culture, settingValue);
            }
            return fieldValue;
        } 


        public object Create(object parent, object configContext, XmlNode section)
        {
            return section;
        }
    }
}