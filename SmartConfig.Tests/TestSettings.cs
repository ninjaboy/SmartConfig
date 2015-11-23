using System;
using System.Collections;
using System.Collections.Generic;

namespace SmartConfig.Tests
{
    public class TestSettingsWithMissingRequiredField : TestSettings
    {
        [SettingValue("missingValue")]
        public bool MissingRequiredSetting { get; set; }
    }


    [SettingSetup(OverrideToken = "TestOverride", OverrideAttributeName = "override")]
    public class TestSettings
    {
        [SettingValue("integerSetting")]
        public int IntSetting { get; set; }

        [SettingValue("boolSetting")]
        public bool BoolSetting { get; set; }

        [SettingValue("decimalSetting")]
        public decimal DecimalSetting { get; set; }

        [SettingValue("defaultValueSetting", DefaultValue = "1985")]
        public int DefaultValueSetting { get; set; }

        [SettingClass("InternalObject", Type = typeof(InternalObject))]
        public InternalObject InternalObject { get; set; }

        [SettingValue("testEnum")]
        public TestEnum TestEnum { get; set; }

        [Obsolete]
        [SettingList(ListName = "InternalObjectsArrayList",
            NodeName = "InternalObject",
            ElementType = typeof(InternalObject))]
        public ArrayList InternalObjectsArrayList { get; set; }

        [SettingList(ListName = "InternalObjectsList",
            NodeName = "InternalObject",
            ElementType = typeof(InternalObject))]
        public List<InternalObject> InternalObjectsList { get; set; }

        [SettingValue("type")]
        public Type Type { get; set; }
    }

    public enum TestEnum
    {
        EnumValue0,
        EnumValue1,
        EnumValue2
    }

    [SettingSetup(OverrideToken = "TestOverride", OverrideAttributeName = "override")]
    public class InternalObject
    {
        [SettingValue("someField")]
        public string SomeField { get; set; }
    }

    public class NoSettingsSetupIsAlsoOk
    {
        [SettingValue("someField")]
        public string SomeField { get; set; }
    }

    public class InconvertibleType
    {
        [SettingValue("type")]
        public Type Type { get; set; }
    }

    public class InconvertibleFromString
    {
        [SettingValue("field")]
        public InternalObject FieldInconvertibleFromString { get; set; }
    }

    public class UnexpectedItemInList
    {
        [SettingList(ListName = "list",
            NodeName = "item",
            ElementType = typeof(InternalObject))]
        public ArrayList List { get; set; }
    }

    public class Item
    {
        [SettingValue("Id")]
        public string Id { get; set; }
    }

    [SettingSetup(OverrideToken = "Override", OverrideAttributeName = "case")]
    public class SettingsWithList
    {
        [SettingClass("InternalItem", Type = typeof(Item))]
        public Item InternalItem { get; set; }

        [SettingList(ListName = "Items", NodeName = "Item", ElementType = typeof(Item))]
        public ArrayList List { get; set; }
    }

    [SettingSetup(OverrideToken = "Override", OverrideAttributeName = "case")]
    public class SettingsWithNorootList
    {
        [SettingClass("InternalItem", Type = typeof(Item))]
        public Item InternalItem { get; set; }

        [SettingList(ListName = "", NodeName = "Item", ElementType = typeof(Item))]
        public ArrayList List { get; set; }
    }

    public class DefaultValues
    {
        [SettingValue("fieldOne", DefaultValue = "default")]
        public string DefaultValue { get; set; }

        [SettingValue("fieldTwo", DefaultValue = null)]
        public string NullStringValue { get; set; }

        [SettingValue("fieldThree")]
        public string NonDefaultValue { get; set; }

        [SettingValue("fieldFour", DefaultValue = null)]
        public int? NullNullableValue { get; set; }

        [SettingClass("fieldFive", Type = typeof(InternalObject), IsRequired = false)]
        public InternalObject NullClassValue { get; set; }

        [SettingValue("fieldSix", DefaultValue = "777")]
        public int? NullableIntValue { get; set; }

        [SettingValue("fieldSeven", DefaultValue = "00:14:13")]
        public TimeSpan? NullableTimeSpanValue { get; set; }
    }

    public class ClassWithNullableFields
    {
        [SettingValue("Id", DefaultValue = "")]
        public TimeSpan? Id { get; set; }

        [SettingValue("Id1", DefaultValue = "")]
        public TimeSpan? Id1 { get; set; }
    }
}