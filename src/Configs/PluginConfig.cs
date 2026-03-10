using System.Text.Json.Serialization;

namespace BasicFaceitServer.Configs;

public class IpByTeam(string side, string[] addresses)
{
    [JsonPropertyName("side")] public string Side { get; } = side;

    [JsonPropertyName("ip_addresses")] public string[] IpAddresses { get; } = addresses;
}

public class PluginConfig
{
    [JsonPropertyName("host")] public string Host { get; set; } = "Kings";

    [JsonPropertyName("friendly_fire_shot")]
    public bool IsFriendlyFireShotOn { get; set; } = true;

    [JsonPropertyName("knife_round")] public bool KnifeRoundEnabled { get; set; } = false;

    [JsonPropertyName("pre_warmup_time")] public int PreWarmupTime { get; set; } = 420;

    [JsonPropertyName("post_warmup_time")] public int PostWarmupTime { get; set; } = 60;
    [JsonPropertyName("warmup_message_interval_seconds")] public int WarmupMessageIntervalSeconds { get; set; } = 60;

    [JsonPropertyName("min_player_to_start")]
    public int MinPlayerToStart { get; set; } = 10;

    [JsonPropertyName("record_demo")] public bool RecordGameDemo { get; set; } = true;

    [JsonPropertyName("auto_join_team")] public bool AutoJoinTeam { get; set; } = false;

    // Required to fill if auto_join_team is true. Set correct ip addresses otherwise 
    [JsonPropertyName("team_by_ip")]
    public IpByTeam[] IpByTeams { get; } =
    [
        new("CT", ["192.168.0.104"]),
        new("T", ["192.168.0.101"])
    ];
}