using System.Reflection;
using ConfigLib;
using GliderRevamp.HudElements;

namespace GliderRevamp;

public sealed class WingworksModSystem : ModSystem
{
    private const string HarmonyId = "wingworks";
    private const string ConfigLibId = "configlib";

    private Harmony _harmony;
    private GliderSpeedHudElement _gliderSpeedHud;

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        
        if (api.ModLoader.IsModEnabled(ConfigLibId))
        {
            SubscribeToConfigChange(api);
        }
        
        _harmony = new Harmony(HarmonyId);
        _harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    public override void StartClientSide(ICoreClientAPI capi)
    {
        base.StartClientSide(capi);
        
        _gliderSpeedHud = new GliderSpeedHudElement(capi);
        capi.Gui.RegisterDialog(_gliderSpeedHud);
    }

    public override void Dispose()
    {
        base.Dispose();

        _harmony?.UnpatchAll(HarmonyId);
        _gliderSpeedHud?.Dispose();
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
}
