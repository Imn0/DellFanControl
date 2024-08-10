namespace DellFanControl.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {

    }

    [Test]
    public void TestStringToFanSpeed()
    {
        var defaultSpeed = FanSpeed.MEDIUM;
        Assert.Multiple(() =>
        {
            Assert.That(Config.FanSpeedFromString("Off", defaultSpeed), Is.EqualTo(FanSpeed.OFF));
            Assert.That(Config.FanSpeedFromString("VeryLow", defaultSpeed), Is.EqualTo(FanSpeed.VERY_LOW));
            Assert.That(Config.FanSpeedFromString("Low", defaultSpeed), Is.EqualTo(FanSpeed.LOW));
            Assert.That(Config.FanSpeedFromString("Medium", defaultSpeed), Is.EqualTo(FanSpeed.MEDIUM));
            Assert.That(Config.FanSpeedFromString("High", defaultSpeed), Is.EqualTo(FanSpeed.HIGH));
            Assert.That(Config.FanSpeedFromString(null, defaultSpeed), Is.EqualTo(defaultSpeed));
            Assert.That(Config.FanSpeedFromString("Unknown", defaultSpeed), Is.EqualTo(defaultSpeed));
        });
    }

    [Test]
    public void TestTemperature()
    {
        string jsonString = @"{
            ""DefaultState"": ""Off"",
            ""ChargingState"": ""Low"",
            ""PlugedInState"": ""VeryLow"",
            ""Docked"": ""Low"",
            ""TemperatureRanges"": [
                { ""MaxTemp"": 60, ""Speed"": ""Off"" },
                { ""MaxTemp"": 75, ""Speed"": ""VeryLow"" },
                { ""MaxTemp"": 86, ""Speed"": ""Low"" },
                { ""MaxTemp"": 999, ""Speed"": ""Medium"" }
            ]
            }";
        Config c = Config.FromJsonString(jsonString);
         Assert.Multiple(() =>
        {
            Assert.That(c.GetSpeedForTemp(56), Is.EqualTo(FanSpeed.OFF));
            Assert.That(c.GetSpeedForTemp(61), Is.EqualTo(FanSpeed.VERY_LOW));
        });
    }
}