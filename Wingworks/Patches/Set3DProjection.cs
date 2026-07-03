using System.Collections.Generic;
using System.Reflection.Emit;
using Vintagestory.Client.NoObf;

namespace Wingworks.Patches;

[HarmonyPatch(typeof(ClientMain), nameof(ClientMain.Set3DProjection), typeof(float), typeof(float))]
public class Patch_Set3DProjection
{
    internal static float FovScale = 1f;
    
    public static float AdjustFov(float fov)
    {
        return fov * FovScale;
    }

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var output = new List<CodeInstruction>
        {
            // fov = AdjustFov(fov);
            // fov is arg2 (arg0 is this, arg1 is zfar)
            new(OpCodes.Ldarg_2),
            new(
                OpCodes.Call,
                AccessTools.Method(typeof(Patch_Set3DProjection), nameof(AdjustFov))
            ),
            new(OpCodes.Starg_S, 2)
        };

        output.AddRange(instructions);
        return output;
    }
}