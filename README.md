# SmartConfig

A library to read complex config structures from app.config 

Show me the code.

1. Config (app.config)
Add section to your app.config as usual
<configuration>
  <configSections>
    <section name="TestSettings" type="SmartConfig.SmartConfigSectionHandler, SmartConfig" />
  </configSections>
  <TestSettings integerSetting="314"
                boolSetting="true"
                decimalSetting="3.14"
                testEnum="EnumValue1"
                type="System.String">
    <TestOverride override="1" decimalSetting="1.27" />
    <InternalObject someField="Original">
      <TestOverride override="1" someField="Overridden" />
    </InternalObject>
    <InternalObjectsList>
      <InternalObject someField="One" />
      <InternalObject someField="Two" />
      <InternalObject someField="Three" >
        <TestOverride override="1" someField="Four" />
      </InternalObject>
    </InternalObjectsList>
  </TestSettings>
</configuration>

2. Describe your settings class in code
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
            get {foreach (InternalObject obj in InternalObjectsImp){ yield return obj; }}
        }
    }
    
3. Read your settings anywhere in the code
            TestSettings testSettings = SmartConfigSectionHandler.GetSection<TestSettings>("TestSettings",
                new CultureInfo("en-GB"),
                (overrideToken, overrideValue) => overrideToken=="TestOverride" && overrideValue == "1");

Key features:
1. Automatic support of type conversion
2. Easy support of complex fields and lists of objects
3. Support of overrides system (i.e. if you want to use different settings values based on OS Version or machine name or _any_ other settings)

                
Please see tests for more details
