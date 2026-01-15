using System.Text.RegularExpressions;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace BasicFaceitServer.Utils;

public class MyHelper(BasicFaceitServer core)
{
    public void PrintToChatPlayer(CCSPlayerController player, string message)
    {
        if (player.Team is CsTeam.Spectator or CsTeam.None)
            return;

        var coloredText = $"{{green}}[{core.Config.Host}]{{white}}: {message}";
        player.PrintToChat(GetColoredText(coloredText));
    }

    public void PrintToCenterPlayer(CCSPlayerController player, string message, float delay = 0.0f)
    {
        if (player.Team is CsTeam.Spectator or CsTeam.None)
            return;

        if (delay > 0.0f)
            core.AddTimer(delay, () => player.PrintToCenter(message));
        else
            player.PrintToCenter(message);
    }

    public void PrintToChatAll(string message)
    {
        var coloredText = $"{{green}}[{core.Config.Host}]{{white}}: {message}";
        var players = GetPlayers();
        foreach (var player in players)
            player.PrintToChat(GetColoredText(coloredText));
    }

    public void PrintToCenterAll(string message)
    {
        var players = GetPlayers();
        foreach (var player in players)
            player.PrintToCenter(message);
    }

    public List<CCSPlayerController> GetPlayers(CsTeam? includeTeam = null, bool includeSpec = false)
    {
        MyLogger.Info("Get players (CT, T)");

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

    public void PreparePlayerForKnifeRound(CCSPlayerController player)
    {
        RemovePlayerWeapon(player);
        SetPlayerAccount(player, 0);
        GivePlayerArmor(player);
        GivePlayerKnife(player);
    }

    private void RemoveWeapon(CCSPlayerController player, string weaponName)
    {
        MyLogger.Info("Weapon design name: " + weaponName);
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
            // player.DropActiveWeapon();
            // Server.NextFrame(() => { weaponEntity.AddEntityIOEvent("Kill", weaponEntity, null, "", 0.1f); });
        }
        catch (Exception ex)
        {
            MyLogger.Error($"Error while Refreshing Weapon via className: {ex.Message}");
        }
    }

    private void RemovePlayerWeapon(CCSPlayerController player)
    {
        if (!player.IsValid || player.PlayerPawn.Value == null) return;
        RemoveWeapon(player, "weapon_c4");
        player.RemoveWeapons();
    }

    public void SetPlayerAccount(CCSPlayerController player, int amount)
    {
        MyLogger.Info($"Set player money to {amount}");
        var playerMoney = player.InGameMoneyServices;
        if (playerMoney is null) return;

        playerMoney.Account = amount;
        Utilities.SetStateChanged(player, "CCSPlayerController_InGameMoneyServices", "m_iAccount");
    }

    private void GivePlayerArmor(CCSPlayerController player)
    {
        MyLogger.Info("Give player armor");
        player.GiveNamedItem("item_kevlar");
    }

    private void GivePlayerKnife(CCSPlayerController player)
    {
        MyLogger.Info("Give player knife");
        var knifeDesignName = player.Team == CsTeam.CounterTerrorist
            ? "weapon_knife"
            : "weapon_knife_t";

        player.GiveNamedItem(knifeDesignName);
    }

    public CCSGameRules? GetGameRules()
    {
        return Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
            .First()
            .GameRules;
    }

    public CsTeam GetKnifeWinnerTeam()
    {
        return core.GameController.KnifeWinnerTeam;
    }

    private string GetColoredText(string message)
    {
        Dictionary<string, int> colorMap = new()
        {
            { "{default}", 1 },
            { "{white}", 1 },
            { "{darkred}", 2 },
            { "{purple}", 3 },
            { "{green}", 4 },
            { "{lightgreen}", 5 },
            { "{slimegreen}", 6 },
            { "{red}", 7 },
            { "{grey}", 8 },
            { "{yellow}", 9 },
            { "{invisible}", 10 },
            { "{lightblue}", 11 },
            { "{blue}", 12 },
            { "{lightpurple}", 13 },
            { "{pink}", 14 },
            { "{fadedred}", 15 },
            { "{gold}", 16 },
            // No more colors are mapped to CS2
        };

        const string pattern = "{(\\w+)}";
        var replaced = Regex.Replace(message, pattern, match =>
        {
            var colorCode = match.Groups[1].Value;
            return colorMap.TryGetValue("{" + colorCode + "}", out var replacement)
                ? Convert.ToChar(replacement).ToString()
                : match.Value;
        });

        return $"\u200B{replaced}";
    }
}