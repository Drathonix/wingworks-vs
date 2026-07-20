using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.Client.NoObf;

namespace Wingworks.Patches;

/// <summary>
/// Fixes an unusual stutter with glide animations caused by the "IsFlying" variable briefly returning to false.
/// </summary>
[HarmonyPatch(typeof(SystemPlayerControl),"OnGameTick")]
public class Wingworks_PatchSystemPlayerControl
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return Transpilers.MethodReplacer(instructions,
            AccessTools.Method(typeof(SystemPlayerControl),"SendServerPackets"), typeof(Wingworks_PatchSystemPlayerControl).GetMethod("ModifyState"));
    }

    public static void ModifyState(SystemPlayerControl __instance, EntityControls before, EntityControls now)
    {
        if(before.Gliding && now.Gliding)
        {
            now.IsFlying = true;
        }
        ReversePatchB.SendServerPackets(__instance, before, now);
    }
}

[HarmonyPatch]
public class ReversePatchB
{
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(SystemPlayerControl), "SendServerPackets")]
    public static void SendServerPackets(object instance, EntityControls before, EntityControls now) =>
        // its a stub so it has no initial content
        throw new NotImplementedException("It's a stub");
}
