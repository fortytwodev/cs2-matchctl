using System.Text.Json.Serialization;

namespace BasicFaceitServer.Configs;

public class Cabin
{
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("side")] public string Side { get; set; }
    [JsonPropertyName("ip_addresses")] public string[] IpAddresses { get; set; }
}

public class MyConfigs
{
    [JsonPropertyName("host")] public string Host { get; set; } = "Kings";
    [JsonPropertyName("friendly_fire_shot")] public bool IsFriendlyFireShotOn { get; set; } = true;
    [JsonPropertyName("knife_round")] public bool KnifeRoundEnabled { get; set; } = false;
    [JsonPropertyName("pre_warmup_time")] public int PreWarmupTime { get; set; } = 420;
    [JsonPropertyName("post_warmup_time")] public int PostWarmupTime { get; set; } = 60;
    [JsonPropertyName("min_player_to_start")] public int MinPlayerToStart { get; set; } = 10;
    [JsonPropertyName("record_demo")] public bool RecordGameDemo { get; set; } = true;
    [JsonPropertyName("cabins")] public Cabin[] Cabins { get; set; } = [];
}