using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Reflection;
using System.Xml;

namespace SmartConfig
{
    public class SmartConfigSectionHandler : IConfigurationSectionHandler
    {
        public static T GetSection<T>(string sectionPath, CultureInfo culture, Delegates.Func<string, string, bool> overrideFilter = null) where T : new()
        {
            XmlNode sectionNode = ConfigurationManager.GetSection(sectionPath) as XmlNode;

            if (sectionNode == null)
            {
                throw new SmartConfigException("Specified section cannot be found in config: {0}", sectionPath);
            }

            return GetSection<T>(sectionNode, culture, overrideFilter);
        }

        private static T GetSection<T>(XmlNode sectionNode, CultureInfo culture, Delegates.Func<string, string, bool> overrideFilter = null) where T : new()
        {
            return (T)GetSection(sectionNode, typeof(T), culture, overrideFilter);
        }

        private static object GetSection(XmlNode sectionNode, Type elementType, CultureInfo culture, Delegates.Func<string, string, bool> overrideFilter = null)
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
                    ReadComplexTypeFromValue(sectionNode, result, settingClass, sectionSettingsSetup, fieldsInfo, culture,
                                             overrideFilter);
                }

                SettingListAttribute listSettings = GetAttribute<SettingListAttribute>(fieldsInfo);
                if (listSettings != null)
                {
                    ReadList(result, sectionNode, listSettings, sectionSettingsSetup, fieldsInfo, culture, overrideFilter);
                }
            }

            return result;
        }

        private static void ReadComplexTypeFromValue(XmlNode sectionNode,
            object result,
            SettingClassAttribute settingClass,
            SettingSetupAttribute settingSetup,
            MemberInfo fieldsInfo,
            CultureInfo culture,
            Delegates.Func<string, string, bool> overrideFilter)
        {
            //Try overriden value
            XmlNode overrideNode = GetOverrideNode(sectionNode, settingSetup, overrideFilter);
            if (overrideNode != null)
            {
                if (SetComplexTypeFromNode(overrideNode, settingClass, result, fieldsInfo, culture, overrideFilter))
                    return;
            }

            //Try original value
            if (SetComplexTypeFromNode(sectionNode, settingClass, result, fieldsInfo, culture, overrideFilter))
                return;

            //Default scenario
            if (settingClass.IsRequired)
            {
                throw new SmartConfigException("Required tag is missing in config: {0}",
                               settingClass.Name);

            }
        }

        private static bool SetComplexTypeFromNode(XmlNode sectionNode, SettingClassAttribute settingClass, object result, MemberInfo memberInfo, CultureInfo culture, Delegates.Func<string, string, bool> overrideFilter)
        {
            foreach (XmlNode node in sectionNode.ChildNodes)
            {
                if (!node.Name.Equals(settingClass.Name))
                {
                    continue;
                }
                object innerValue = GetSection(node, settingClass.Type, culture, overrideFilter);
                SetFieldValue(result, memberInfo, innerValue);
                return true;
            }
            return false;
        }

        private static void ReadList(object target,
                                     XmlNode sectionNode,
                                     SettingListAttribute listSettings,
                                     SettingSetupAttribute settingsSetup,
                                     MemberInfo fieldsInfo,
                                     CultureInfo culture,
                                     Delegates.Func<string, string, bool> overrideFilter = null)
        {
            var overrideNode = GetOverrideNode(sectionNode, settingsSetup, overrideFilter);
            if (overrideNode != null)
            {
                if (ReadListFromNode(target, overrideNode, listSettings, fieldsInfo, culture, overrideFilter))
                    return;
            }
            if (ReadListFromNode(target, sectionNode, listSettings, fieldsInfo, culture, overrideFilter))
                return;

            if(listSettings.IsRequired)
                throw new SmartConfigException("Required tag is missing in config: {0}", listSettings.ListName);
        }

        private static bool ReadListFromNode(object target,
            XmlNode sectionNode,
            SettingListAttribute listSettings,
            MemberInfo fieldsInfo,
            CultureInfo culture,
            Delegates.Func<string, string, bool> overrideFilter = null)
        {
            if (!string.IsNullOrEmpty(listSettings.ListName))
            {
                foreach (XmlNode childNode in sectionNode.ChildNodes)
                {
                    if (childNode.Name.Equals(listSettings.ListName))
                    {
                        return ReadListFromNode(target, listSettings, fieldsInfo, culture, overrideFilter, childNode,
                                                true);
                    }
                }
            }
            else
            {
                return ReadListFromNode(target, listSettings, fieldsInfo, culture, overrideFilter, sectionNode, false);
            }
            return false;
        }

        private static bool ReadListFromNode(object target, SettingListAttribute listSettings, MemberInfo fieldsInfo,
                                             CultureInfo culture, Delegates.Func<string, string, bool> overrideFilter, XmlNode childNode,
                                             bool checkUnexpectedItems)
        {
            IList resultList = (IList) Activator.CreateInstance(((PropertyInfo) fieldsInfo).PropertyType);
            foreach (XmlNode itemNode in childNode.ChildNodes)
            {
                if (itemNode.NodeType == XmlNodeType.Comment)
                    continue;
                if (!itemNode.Name.Equals(listSettings.NodeName))
                {
                    if (checkUnexpectedItems)
                    {
                        throw new SmartConfigException("Unexpected item in the list: {0}/{1}", itemNode.Name,
                                                       listSettings.NodeName);
                    }
                    continue;
                }

                object section = GetSection(itemNode, listSettings.ElementType, culture, overrideFilter);
                resultList.Add(section);
            }
            SetFieldValue(target, fieldsInfo, resultList);
            return true;
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
                                                                           Delegates.Func<string, string, bool> overrideFilter,
                                                                           CultureInfo culture)
        {
            //check if section supports overrides
            var overrideNode = GetOverrideNode(source, settingSetup, overrideFilter);

            if (overrideNode != null) //found overriden value. Enough for us
            {
                if (ReadSimpleValueFromNode(overrideNode, target, settingValue, fieldToPopulate, culture)) return;
            }
            if (ReadSimpleValueFromNode(source, target, settingValue, fieldToPopulate, culture)) return;

            if (settingValue.DefaultValueSpecified) //default value specified in section definition. Use it
            {
                SetFieldValue(target, fieldToPopulate, settingValue.DefaultValue, culture);
                return;
            }

            throw new SmartConfigException("Required attribute is missing in config: {0}",
                                           settingValue.Name);
        }

        private static bool ReadSimpleValueFromNode(XmlNode source, object target, SettingValueAttribute settingValue,
                                     MemberInfo fieldToPopulate, CultureInfo culture)
        {
            XmlNode originalValueNode = source.Attributes.GetNamedItem(settingValue.Name, string.Empty);
            if (originalValueNode != null) //found original node value. Enough at the moment
            {
                string valueFromSource = originalValueNode.Value;
                SetFieldValue(target, fieldToPopulate, valueFromSource, culture);
                return true;
            }
            return false;
        }


        private static XmlNode GetOverrideNode(XmlNode source,
                                                    SettingSetupAttribute settingSetup,
                                                    Delegates.Func<string, string, bool> overrideFilter)
        {
            if (settingSetup == null)
                return null;
            if (overrideFilter == null)
                return null;

            if (!string.IsNullOrEmpty(settingSetup.OverrideToken))
            {
                foreach (XmlNode overrideNode in source.ChildNodes)
                {
                    if (overrideNode.Name.Equals(settingSetup.OverrideToken))
                    {
                        if (overrideNode.Attributes != null)
                        {
                            XmlNode overrideAttribute = overrideNode.Attributes.GetNamedItem(
                                                                settingSetup.OverrideAttributeName, string.Empty);
                            if (overrideAttribute != null)
                            {
                                if (overrideFilter(settingSetup.OverrideToken, overrideAttribute.Value))
                                {
                                    return overrideNode;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        protected static void SetFieldValue(object target, MemberInfo fieldInfo, object value)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            if (fieldInfo.MemberType == MemberTypes.Property)
            {
                flags |= BindingFlags.SetProperty;
            }

            if (value is string && value != null)
            {
                value = Environment.ExpandEnvironmentVariables(value as string);
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

        private static object ConvertSettingToFieldValue(string settingValue, Type memberType, CultureInfo culture)
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

                if (settingValue == null && CanAssignNullTo(memberType))
                {
                    fieldValue = null;
                }
                else
                {
                    fieldValue = typeConverter.ConvertFromString(null, culture, settingValue);
                }
            }
            return fieldValue;
        }

        private static bool CanAssignNullTo(Type memberType)
        {
            return !memberType.IsValueType || Nullable.GetUnderlyingType(memberType) != null;
        }
        
        public object Create(object parent, object configContext, XmlNode section)
        {
            if (section.Name == "EncryptedData")
            {
                section = section.SelectSingleNode(section.FirstChild.Name);
            }

            return section;
        }
    }
}