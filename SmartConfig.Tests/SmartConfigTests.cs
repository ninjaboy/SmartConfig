
using System.Globalization;
using BS2000Common;
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
            Assert.AreEqual(3, testSettings.InternalObjectsImp.Count);
            Assert.AreEqual("One", ((InternalObject)testSettings.InternalObjectsImp[0]).SomeField);
            Assert.AreEqual("Three", ((InternalObject)testSettings.InternalObjectsImp[2]).SomeField);
            Assert.AreEqual(1985, testSettings.DefaultValueSetting, "Default value setup test failed");
            Assert.AreEqual(TestEnum.EnumValue1, testSettings.TestEnum, "Enum conversion failed");
        }

        [Test]
        public void SimpleObjectWithOverrideReadTest()
        {
            TestSettings testSettings = SmartConfigSectionHandler.GetSection<TestSettings>("TestSettings",
                new CultureInfo("en-GB"),
                (overrideToken, overrideValue) => overrideToken=="TestOverride" && overrideValue == "1");
            Assert.AreEqual(314, testSettings.IntSetting, "Integer conversion failed");
            Assert.AreEqual(1.27, testSettings.DecimalSetting, "Decimal conversion failed");
            Assert.AreEqual(true, testSettings.BoolSetting, "Bool conversion failed");
            Assert.AreEqual(typeof(string), testSettings.Type, "Type conversion failed");
            Assert.IsNotNull(testSettings.InternalObject, "Internal object reading failed");
            Assert.AreEqual("Overridden", testSettings.InternalObject.SomeField);
            Assert.AreEqual(3, testSettings.InternalObjectsImp.Count);
            Assert.AreEqual("One", ((InternalObject)testSettings.InternalObjectsImp[0]).SomeField);
            Assert.AreEqual("Four", ((InternalObject)testSettings.InternalObjectsImp[2]).SomeField);
        }

        [Test]
        [ExpectedException(typeof(SmartConfigException))]
        public void RequiredValueMissingTest()
        {
            TestSettingsWithMissigRequiredField testSettings = SmartConfigSectionHandler.GetSection<TestSettingsWithMissigRequiredField>("TestSettings",
                new CultureInfo("en-GB"),
                (overrideToken, overrideValue) => false);
        }

        [Test]
        [ExpectedException(typeof(SmartConfigException))]
        public void RequiredClassMissingTest()
        {
            TestSettings testSettings = SmartConfigSectionHandler.GetSection<TestSettings>("TestSettingsMissingClass",
                new CultureInfo("en-GB"));
        }

        [Test]
        [ExpectedException(typeof(SmartConfigException))]
        public void FailIfMethodIsMarkedAsSetting()
        {
            TestSettings testSettings = SmartConfigSectionHandler.GetSection<TestSettings>("TestSettingsMissingClass",
                new CultureInfo("en-GB"));
        }

        [Test]
        [ExpectedException(typeof(SmartConfigException))]
        public void FailIfSectionIsMissing()
        {
            TestSettings testSettings = SmartConfigSectionHandler.GetSection<TestSettings>("MissingSection",
                new CultureInfo("en-GB"));
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
        [ExpectedException(typeof(SmartConfigException))]
        public void InconvertibleTypeException()
        {
            InconvertibleType testSettings = SmartConfigSectionHandler.GetSection<InconvertibleType>("InconvertibleType",
                new CultureInfo("en-GB"));
            Assert.AreEqual(typeof(string), testSettings.Type);
        }

        [Test]
        [ExpectedException(typeof(SmartConfigException))]
        public void InconvertiblefromString()
        {
            InconvertibleFromString testSettings = SmartConfigSectionHandler.GetSection<InconvertibleFromString>("InconvertibleFromString",
                new CultureInfo("en-GB"));
        }

        [Test]
        [ExpectedException(typeof(SmartConfigException))]
        public void UnexpectedItemInListTest()
        {
            UnexpectedItemInList testSettings = SmartConfigSectionHandler.GetSection<UnexpectedItemInList>("UnexpectedItemInList",
                new CultureInfo("en-GB"));
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
    }
}
