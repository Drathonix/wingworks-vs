using System.Runtime.CompilerServices;
using Vintagestory.API.Common.Entities;

namespace Wingworks.Patches;

[HarmonyPatch(typeof(PModuleInAir), "ApplyFlying"), UsedImplicitly]
public sealed class GliderRevamp_PModuleInAir_ApplyFlying
{
    [HarmonyReversePatch, MethodImpl(MethodImplOptions.NoInlining), UsedImplicitly]
    public static void ApplyFlying(PModuleInAir __instance, float dt, Entity entity, EntityPos pos, EntityControls controls) { }
}
