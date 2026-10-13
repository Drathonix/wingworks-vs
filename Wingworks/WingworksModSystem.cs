using ConfigLib;
using ImGuizmoNET;
using System;
using System.Reflection;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.Server;

namespace Wingworks;

public sealed class WingworksModSystem : ModSystem
{
    private const string HarmonyId = "wingworks";
    private const string ConfigLibId = "configlib";

    private Harmony _harmony;
    private static ICoreClientAPI capi;
    public override void Start(ICoreAPI api)
    {
        WingworksEventHandler.Init();
        if (api.ModLoader.IsModEnabled(ConfigLibId))
        {
            SubscribeToConfigChange(api);
        }
        _harmony = new Harmony(HarmonyId);
        _harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        capi = api;
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);
        var wwServerCommand = api.ChatCommands.Create("wingworks");
        wwServerCommand
            .WithDescription("The Wingworks Root commmand.")
            .RequiresPrivilege(Privilege.chat)
            .BeginSubCommand("modifyflightability")   
            .WithDescription("Modifies the player flight stats. Ignores entity model traits. Setting this value to -1000 and below disables flight permanently. Setting this to a value above 1000 enables it permanently. Set to 0 to enable default behavior. Provide no number to see current setting.")
            .RequiresPrivilege(Privilege.gamemode)
            .WithArgs(api.ChatCommands.Parsers.OnlinePlayer("player"), api.ChatCommands.Parsers.OptionalFloat("value",-0.536237655765089341376982236778F))
            .HandleWith((args) =>
            {
                var player = args[0] as IPlayer;
                var value = (args.ArgCount > 1 ? (float)args[1] : 0F);
                var entity = player.Entity;
                if (value == -0.536237655765089341376982236778F)
                {
                    
                    EntityFloatStats stat = entity.Stats["canGlide"];
                    if (stat == null || !stat.ValuesByKey.ContainsKey("ww_force_flight"))
                    {
                        return TextCommandResult.Success($"{player.PlayerName}'s force flight setting is set to 0 (Defaults to model traits)");
                    }
                    else
                    {
                        var val = stat.ValuesByKey["ww_force_flight"].Value;
                        var state1 = val <= -1F ? "Disabled" : val >= 1 ? "Enabled" : "Defaults to model traits";
                        return TextCommandResult.Success($"{player.PlayerName}'s force flight setting is set to {val} ({state1}) (blended value of {stat.GetBlended()})");
                    }
                }
                EntityFloatStats canGlide = entity.Stats["canGlide"];
                if (canGlide == null)
                {
                    entity.Stats.Set("canGlide", "trait", 0,true);
                    canGlide = entity.Stats["canGlide"];
                }
                canGlide.Set("ww_force_flight", (float)value,true);

                EntityFloatStats flight = player.Entity.Stats["ww_can_fly"];
                if (flight == null)
                {
                    player.Entity.Stats.Set("ww_can_fly", "trait", 0,true);
                    flight = player.Entity.Stats["ww_can_fly"];
                }
                flight.Set("ww_force_flight", (float)value,true);
                entity.Stats.ToTreeAttributes(entity.WatchedAttributes, forClient: true);
                entity.WatchedAttributes.MarkPathDirty("stats");
                var state = value <= -1F ? "Disabled" : value >= 1 ? "Enabled" : "Defaults to model traits";
                return TextCommandResult.Success($"{player.PlayerName}'s force flight setting is set to {value} ({state}) (blended value of {canGlide.GetBlended()})");
            }).EndSubCommand();
    }


    public override void Dispose()
    {
        base.Dispose();

        _harmony?.UnpatchAll(HarmonyId);
    }

    private static void SubscribeToConfigChange(ICoreAPI api)
    {
        var system = api.ModLoader.GetModSystem<ConfigLibModSystem>();

        system.SettingChanged += (domain, _, setting) =>
        {
            if (domain != HarmonyId)
            {
                return;
            }

            setting.AssignSettingValue(ModConfig.Instance);
        };
        
        system.ConfigsLoaded += () =>
        {
            system.GetConfig(HarmonyId)?.AssignSettingsValues(ModConfig.Instance);
        };
    }

    public static bool DoCalculations(EntityAgent instance)
    {
        if (instance is EntityPlayer p) {
            return capi == null || capi.World?.Player?.PlayerUID == p.PlayerUID;
        }
        return true;
    }
}
