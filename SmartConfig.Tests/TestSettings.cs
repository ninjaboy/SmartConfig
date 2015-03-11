using System;
using System.Collections;

namespace SmartConfig.Tests
{
    public class TestSettingsWithMissigRequiredField : TestSettings
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

        [SettingList(ListName = "InternalObjectsList",
            NodeName = "InternalObject",
            ElementType = typeof(InternalObject))]
        public ArrayList InternalObjectsImp { get; set; }

        [SettingValue("type")]
        public Type Type { get; set; }

        public System.Collections.Generic.IEnumerable<InternalObject> InternalObjects
        {
            get
            {
                foreach (InternalObject obj in InternalObjectsImp)
                {
                    yield return obj;
                }
            }
        }
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
}