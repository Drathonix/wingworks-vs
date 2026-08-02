using System;
using System.Reflection;
using ConfigLib;
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
