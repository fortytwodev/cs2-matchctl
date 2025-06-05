using System.Net;
using System.Text.Json.Serialization;

namespace BasicFaceitServer.Configs;

public class Cabin
{
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("ip_addresses")] public string[] IpAddresses { get; set; }
}

public class MyConfigs
{
    [JsonPropertyName("host")] public string Host { get; set; } = "Kings";
    [JsonPropertyName("cabins")] public Cabin[] Cabins { get; set; } = [];
    [JsonPropertyName("pre_warmup_time")] public int PreWarmupTime { get; set; } = 420;
    [JsonPropertyName("post_warmup_time")] public int PostWarmupTime { get; set; } = 60;
    [JsonPropertyName("min_player_to_start")] public int MinPlayerToStart { get; set; } = 10;
    [JsonPropertyName("knife_round")] public bool IsKnifeRoundIncluded { get; set; } = false;
    [JsonPropertyName("friendly_fire_shot")] public bool IsFriendlyFireShotOn { get; set; } = true;
}