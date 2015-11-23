
using System;
using System.Globalization;
using NUnit.Framework;

namespace SmartConfig.Tests
{
    [TestFixture]
    public class SmartConfigTests
    {
        [Test]
        public void SimpleObjectReadTest()
        {
            TestSettings testSettings = SmartConfigSectionHandler.GetSection<TestSettings>("TestSettings",
                new CultureInfo("en-GB"),
                (overrideToken, overrideValue) => false);
            Assert.AreEqual(314, testSettings.IntSetting, "Integer conversion failed");
            Assert.AreEqual(3.14, testSettings.DecimalSetting, "Decimal conversion failed");
            Assert.AreEqual(true, testSettings.BoolSetting, "Bool conversion failed");
            Assert.IsNotNull(testSettings.InternalObject, "Internal object reading failed");
            Assert.AreEqual("Original", testSettings.InternalObject.SomeField);
            Assert.AreEqual(3, testSettings.InternalObjectsArrayList.Count);
            Assert.AreEqual("One", ((InternalObject)testSettings.InternalObjectsArrayList[0]).SomeField);
            Assert.AreEqual("Three", ((InternalObject)testSettings.InternalObjectsArrayList[2]).SomeField);
            Assert.AreEqual("Two", testSettings.InternalObjectsList[1].SomeField);
            Assert.AreEqual("Three", testSettings.InternalObjectsList[2].SomeField);
            Assert.AreEqual(1985, testSettings.DefaultValueSetting, "Default value setup test failed");
            Assert.AreEqual(TestEnum.EnumValue1, testSettings.TestEnum, "Enum conversion failed");
        }

        [Test]
        public void SimpleObjectWithOverrideReadTest()
        {
            TestSettings testSettings = SmartConfigSectionHandler.GetSection<TestSettings>("TestSettings",
                new CultureInfo("en-GB"),
                (overrideToken, overrideValue) => overrideToken == "TestOverride" && overrideValue == "1");
            Assert.AreEqual(314, testSettings.IntSetting, "Integer conversion failed");
            Assert.AreEqual(1.27, testSettings.DecimalSetting, "Decimal conversion failed");
            Assert.AreEqual(true, testSettings.BoolSetting, "Bool conversion failed");
            Assert.AreEqual(typeof(string), testSettings.Type, "Type conversion failed");
            Assert.IsNotNull(testSettings.InternalObject, "Internal object reading failed");
            Assert.AreEqual("Overridden", testSettings.InternalObject.SomeField);
            Assert.AreEqual(3, testSettings.InternalObjectsArrayList.Count);
            Assert.AreEqual("One", ((InternalObject)testSettings.InternalObjectsArrayList[0]).SomeField);
            Assert.AreEqual("Four", ((InternalObject)testSettings.InternalObjectsArrayList[2]).SomeField);
            Assert.AreEqual("Two", testSettings.InternalObjectsList[1].SomeField);
            Assert.AreEqual("Six", testSettings.InternalObjectsList[2].SomeField);
        }

        [Test]
        public void RequiredValueMissingTest()
        {
            Assert.Throws<SmartConfigException>(() =>
                {
                    TestSettingsWithMissingRequiredField testSettings = SmartConfigSectionHandler.GetSection<TestSettingsWithMissingRequiredField>("TestSettings",
                        new CultureInfo("en-GB"),
                        (overrideToken, overrideValue) => false);
                });
        }

        [Test]
        public void RequiredClassMissingTest()
        {
            Assert.Throws<SmartConfigException>(() =>
                {
                    TestSettings testSettings = SmartConfigSectionHandler.GetSection<TestSettings>("TestSettingsMissingClass",
                        new CultureInfo("en-GB"));
                });
        }

        [Test]
        public void FailIfMethodIsMarkedAsSetting()
        {
            Assert.Throws<SmartConfigException>(() =>
                {
                    TestSettings testSettings = SmartConfigSectionHandler.GetSection<TestSettings>("TestSettingsMissingClass",
                        new CultureInfo("en-GB"));

                });
        }

        [Test]
        public void FailIfSectionIsMissing()
        {
            Assert.Throws<SmartConfigException>(() =>
                {
                    TestSettings testSettings = SmartConfigSectionHandler.GetSection<TestSettings>("MissingSection",
                        new CultureInfo("en-GB"));

                });
        }

        [Test]
        public void NoSettingSetupIsOk()
        {
            NoSettingsSetupIsAlsoOk testSettings = SmartConfigSectionHandler.GetSection<NoSettingsSetupIsAlsoOk>("NoSetup",
                new CultureInfo("en-GB"));
            Assert.AreEqual("1", testSettings.SomeField);
        }

        [Test]
        public void NoOverrideFilterIsOk()
        {
            InternalObject testSettings = SmartConfigSectionHandler.GetSection<InternalObject>("NoSetup",
                new CultureInfo("en-GB"));
            Assert.AreEqual("1", testSettings.SomeField);
        }

        [Test]
        public void InconvertibleTypeException()
        {
            Assert.Throws<SmartConfigException>(() =>
                {
                    InconvertibleType testSettings = SmartConfigSectionHandler.GetSection<InconvertibleType>("InconvertibleType",
                        new CultureInfo("en-GB"));
                    Assert.AreEqual(typeof(string), testSettings.Type);

                });
        }

        [Test]
        public void InconvertiblefromString()
        {
            Assert.Throws<SmartConfigException>(() =>
                {
                    InconvertibleFromString testSettings =
                        SmartConfigSectionHandler.GetSection<InconvertibleFromString>("InconvertibleFromString",
                                                                                      new CultureInfo("en-GB"));
                });
        }

        [Test]
        public void UnexpectedItemInListTest()
        {
            Assert.Throws<SmartConfigException>(() =>
                {
                    UnexpectedItemInList testSettings = SmartConfigSectionHandler.GetSection<UnexpectedItemInList>("UnexpectedItemInList",
                        new CultureInfo("en-GB"));

                });
        }

        [Test]
        public void ComplexObjectOverrideTest()
        {
            SettingsWithList overriden = SmartConfigSectionHandler.
                                           GetSection<SettingsWithList>("TestListOverride",
                                                                    new CultureInfo("en-GB"),
                                                                    (p1, p2) => p2 == "0");
            Assert.AreEqual("case0", overriden.InternalItem.Id);
        }

        [Test]
        public void ListOverrideTest()
        {
            SettingsWithList overriden = SmartConfigSectionHandler.
                GetSection<SettingsWithList>("TestListOverride",
                                             new CultureInfo("en-GB"),
                                             (p1, p2) => p2 == "0");
            Assert.AreEqual(1, overriden.List.Count);
            Assert.AreEqual("2", ((Item)overriden.List[0]).Id);
        }

        [Test]
        public void TestDefaultValues()
        {
            DefaultValues defaultValues = SmartConfigSectionHandler.
                GetSection<DefaultValues>("DefaultValues", new CultureInfo("en-GB"));

            Assert.AreEqual("default", defaultValues.DefaultValue);
            Assert.AreEqual(null, defaultValues.NullStringValue);
            Assert.AreEqual("non-default", defaultValues.NonDefaultValue);
            Assert.AreEqual(null, defaultValues.NullNullableValue);
            Assert.AreEqual(null, defaultValues.NullClassValue);
            Assert.AreEqual(777, defaultValues.NullableIntValue);
            Assert.AreEqual(new TimeSpan(0, 14, 13), defaultValues.NullableTimeSpanValue);
        }

        [Test]
        public void TestNullableValues()
        {
            ClassWithNullableFields values = SmartConfigSectionHandler.
                GetSection<ClassWithNullableFields>("NullableValues", new CultureInfo("en-GB"));

            Assert.AreEqual(TimeSpan.FromSeconds(5), values.Id);
            Assert.AreEqual(null, values.Id1);
        }

        [Test]
        public void NorootListOverrideTest()
        {
            SettingsWithNorootList overriden = SmartConfigSectionHandler.
                                           GetSection<SettingsWithNorootList>("TestNorootListOverride",
                                                                    new CultureInfo("en-GB"),
                                                                    (p1, p2) => p2 == "0");
            Assert.AreEqual(1, overriden.List.Count);
            Assert.AreEqual("2", ((Item)overriden.List[0]).Id);

            SettingsWithNorootList original = SmartConfigSectionHandler.
                               GetSection<SettingsWithNorootList>("TestNorootListOverride",
                                                        new CultureInfo("en-GB"),
                                                        (p1, p2) => p2 == "Zorg");
            Assert.AreEqual(2, original.List.Count);
            Assert.AreEqual("0", ((Item)original.List[0]).Id);

        }

        [Test]
        public void CheckThatOriginalListIsUsedIfNotSpecifiedInOverride()
        {
            SettingsWithList overriden = SmartConfigSectionHandler.
                                           GetSection<SettingsWithList>("TestListOverride",
                                                                    new CultureInfo("en-GB"),
                                                                    (p1, p2) => p2 == "1");
            Assert.AreEqual(2, overriden.List.Count);
            Assert.AreEqual("0", ((Item)overriden.List[0]).Id);
            Assert.AreEqual("1", ((Item)overriden.List[1]).Id);
            Assert.AreEqual("case1", overriden.InternalItem.Id);
        }

    }
}
