using BasicFaceitServer.Configs;
using BasicFaceitServer.Services.Interfaces;
using BasicFaceitServer.Infrastructure;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace BasicFaceitServer.Services;

public class PlayerService : IPlayerInterface
{
    private readonly BasicFaceitServer _plugin;
    private readonly PluginConfig _configs;

    public PlayerService(BasicFaceitServer plugin, PluginConfig config)
    {
        _plugin = plugin;
        _configs = config;
    }

    public CsTeam GetPlayerTeam(CCSPlayerController player)
    {
        PluginLogger.Info("Get client team (CT, T or Spectator)");
        var playerIp = player.IpAddress?.Split(":")[0];

        if (string.IsNullOrEmpty(playerIp))
            return CsTeam.Spectator;

        var cabin = _configs.IpByTeams?.FirstOrDefault(c => c.IpAddresses.Contains(playerIp));
        if (cabin == null)
            return CsTeam.None;

        var defaultTeam = cabin.Side switch
        {
            "CT" => CsTeam.CounterTerrorist,
            "T" => CsTeam.Terrorist,
            _ => CsTeam.Spectator
        };

        return defaultTeam;
    }

    public void PlayerJoinTeam(CCSPlayerController player, CsTeam playerTeam)
    {
        PluginLogger.Info($"Player team - {playerTeam.ToString()}");

        _plugin.AddTimer(0.1f, () =>
        {
            player.ChangeTeam(CsTeam.Spectator);

            if (playerTeam == CsTeam.Spectator) return;

            player.Respawn();
            _plugin.AddTimer(0.1f, () => { player.ChangeTeam(playerTeam); });
        });
    }

    public void PreparePlayerForKnifeRound(CCSPlayerController player)
    {
        RemovePlayerWeapon(player);
        SetPlayerAccount(player, 0);
        GivePlayerArmor(player);
        GivePlayerKnife(player);
    }

    private static void RemovePlayerWeapon(CCSPlayerController player)
    {
        if (!player.IsValid || player.PlayerPawn.Value == null) return;
        RemoveWeaponByName(player, "weapon_c4");
        player.RemoveWeapons();
    }
    
    private static void RemoveWeaponByName(CCSPlayerController player, string weaponName)
    {
        PluginLogger.Info("Weapon design name: " + weaponName);
        var weaponServices = player.PlayerPawn.Value?.WeaponServices;

        if (weaponServices == null)
            return;

        var matchedWeapon = weaponServices.MyWeapons
            .FirstOrDefault(w => w.IsValid && w.Value != null && w.Value.DesignerName == weaponName);

        try
        {
            if (matchedWeapon?.IsValid != true) return;
            weaponServices.ActiveWeapon.Raw = matchedWeapon.Raw;

            var weaponEntity = weaponServices.ActiveWeapon.Value?.As<CBaseEntity>();
            if (weaponEntity == null || !weaponEntity.IsValid)
                return;

            weaponEntity.Remove();
        }
        catch (Exception ex)
        {
            PluginLogger.Error($"Error while Refreshing Weapon via className: {ex.Message}");
        }
    }
    
    private static void SetPlayerAccount(CCSPlayerController player, int amount)
    {
        PluginLogger.Info($"Set player money to {amount}");
        var playerMoney = player.InGameMoneyServices;
        if (playerMoney is null) return;

        playerMoney.Account = amount;
        Utilities.SetStateChanged(player, "CCSPlayerController_InGameMoneyServices", "m_iAccount");
    }

    private static void GivePlayerArmor(CCSPlayerController player)
    {
        PluginLogger.Info("Give player armor");
        player.GiveNamedItem("item_kevlar");
    }

    private static void GivePlayerKnife(CCSPlayerController player)
    {
        PluginLogger.Info("Give player knife");
        var knifeDesignName = player.Team == CsTeam.CounterTerrorist
            ? "weapon_knife"
            : "weapon_knife_t";

        player.GiveNamedItem(knifeDesignName);
    }
    
    public List<CCSPlayerController> GetPlayersList(CsTeam? includeTeam = null, bool includeSpec = false)
    {
        PluginLogger.Info("Get players (CT, T)");

        var playerList = Utilities
            .GetPlayers()
            .Where(player =>
                player is { IsValid: true, IsBot: false, IsHLTV: false}
                && (includeTeam == null || player.Team == includeTeam)
                && player.Team != CsTeam.None
                && (includeSpec || player.Team != CsTeam.Spectator)
            )
            .ToList();

        return playerList;
    }
}
