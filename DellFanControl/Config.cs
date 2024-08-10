namespace DellFanControl;

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;


public class ConfigState
{

    [JsonPropertyName("AutoSpoolDownTime")]
    public int? AutoSpoolDownTime { get; set; }

    [JsonPropertyName("DefaultState")]
    public string? DefaultState { get; set; }

    [JsonPropertyName("ChargingState")]
    public string? ChargingState { get; set; }

    [JsonPropertyName("PlugedInState")]
    public string? PlugedInState { get; set; }

    [JsonPropertyName("Docked")]
    public string? Docked { get; set; }

    [JsonPropertyName("TemperatureRanges")]
    public List<TemperatureRange> TemperatureRanges { get; set; } = [];
}

public class TemperatureRange
{
    [JsonPropertyName("MaxTemp")]
    public required int MaxTemp { get; set; }

    [JsonPropertyName("Speed")]
    public required string Speed { get; set; }
}

public class Config
{

    private ConfigState _config;
    public Config(string filePath)
    {
        string jsonString;
        try
        {
            jsonString = File.ReadAllText(filePath);
        }
        catch (Exception e)
        {
#if DEBUG
            Console.WriteLine($"{e.Message}, using default config");
#else 
            File.WriteAllText("error.log", $"error reading config{e.Message}");
#endif
            jsonString = @"{
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
        }
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        _config = JsonSerializer.Deserialize<ConfigState>(jsonString, options) ?? new ConfigState();
    }

    public static Config FromJsonString(string jsonString)
    {
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        var configState = JsonSerializer.Deserialize<ConfigState>(jsonString, options) ?? new ConfigState();
        var config = new Config(string.Empty) { _config = configState };
        return config;
    }

    public static FanSpeed FanSpeedFromString(string? value, FanSpeed defaultSpeed)
    {

        return value switch
        {
            "Off" => FanSpeed.OFF,
            "VeryLow" => FanSpeed.VERY_LOW,
            "Low" => FanSpeed.LOW,
            "Medium" => FanSpeed.MEDIUM,
            "High" => FanSpeed.HIGH,
            null => defaultSpeed,
            _ => defaultSpeed
        };
    }

    public int AutoSpoolDownTime()
    {
        return _config.AutoSpoolDownTime ?? 35;
    }

    public FanSpeed DefaultSpeed()
    {
        return FanSpeedFromString(_config.DefaultState, FanSpeed.OFF);
    }
    public FanSpeed ChargingSpeed()
    {
        return FanSpeedFromString(_config.ChargingState, FanSpeed.LOW);
    }
    public FanSpeed PlugedInSpeed()
    {
        return FanSpeedFromString(_config.PlugedInState, FanSpeed.VERY_LOW);
    }
    public FanSpeed DockedSpeed()
    {
        return FanSpeedFromString(_config.Docked, FanSpeed.LOW);
    }
    public FanSpeed GetSpeedForTemp(float temperature)
    {
        var speedString = _config.TemperatureRanges.FirstOrDefault(range => temperature <= range.MaxTemp)?.Speed;

        return FanSpeedFromString(speedString, FanSpeed.MEDIUM);
    }
}