using GliderRevamp.API;

namespace GliderRevamp.Patches;

[HarmonyPatch(typeof(ModSystemGliding), "Input_InWorldAction"), UsedImplicitly]
public class GliderRevamp_ModSystemGliding_Input_InWorldAction
{
    private static readonly AccessTools.FieldRef<ModSystemGliding, ICoreClientAPI> CapiRef
        = AccessTools.FieldRefAccess<ModSystemGliding, ICoreClientAPI>("capi");
    
    [UsedImplicitly]
    internal static bool Prefix(ModSystemGliding __instance, EnumEntityAction action, bool on, ref EnumHandling handled)
    {
        if (ModConfig.Instance.DisableGlider)
        {
            return false;
        }

        var entity = CapiRef(__instance)?.World.Player.Entity;
        if (entity == null)
        {
            return false;
        }

        var speedMs = entity.Pos.Motion.Length() * 60f;
        var activationSpeed = WingworksStats.GetOrDefault(entity.Stats, "ww_start_speed", ModConfig.Instance.ActivationSpeedMs); 
        var upwardSpeed = entity.Pos.Motion.Y; // Prevent activation when jumping.
        
        return !(speedMs < activationSpeed || upwardSpeed > -3f / 60f);
    }
}