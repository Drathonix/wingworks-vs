using ConfigLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Wingworks.API;

namespace Wingworks.Patches;

[HarmonyPatch(typeof(EntityAgent), nameof(EntityAgent.OnGameTick)), HarmonyPriority(401)]
public class PatchEntityAgent
{

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var targetMethod = typeof(AnimationMetaData).GetMethod("Matches");
        var replMethod = typeof(PatchEntityAgent).GetMethod("SpecMatch");

        var code = new List<CodeInstruction>(instructions);
        for (int i = 0; i < code.Count; i++)
        {
            var inst = code[i];
            if (inst.Calls(targetMethod))
            {
                code[i] = new CodeInstruction(OpCodes.Ldarg_0);
                code.Insert(i+1,new CodeInstruction(OpCodes.Call,replMethod));
                //hasPatched = true;
            }
        }
        //if (!hasPatched) throw new Exception("Did not find a target method. Wingworks cannot work in this version of VintageStory. Please report immediately with title of \"PatchEntityAgent Transpiler Failure in Most Recent Game Version\". Thank you!");
        return code;

    }

    public static bool SpecMatch(AnimationMetaData __instance, int controls, EntityAgent entity)
    {
        if(__instance.TriggeredBy is ISpecialAnimationTrigger trigger)
        {
            if (trigger.ShouldDoDefaultChecksAdditionally() && !__instance.Matches(controls))
            {
                return false;
            }
            return trigger.Matches(entity,controls);
        }
        return __instance.Matches(controls);
    }


    internal static bool Prefix(EntityAgent __instance, float dt)
    {
        if (WingworksStats.CanFly(__instance.Stats) && WingworksModSystem.DoCalculations(__instance))
        {
            ITreeAttribute wings = __instance.WatchedAttributes.GetOrAddTreeAttribute("wingworks");
            if (__instance.Controls.Gliding)
            {
                var t = wings.GetFloat("flap");
                var ft = wings.GetFloat("time");
                if (t > -1)
                {
                    t += dt;
                    wings.SetFloat("flap", t);
                }
                ft += dt;
                bool blockFlap = false;
                if (__instance.Controls.Backward)
                {
                    WingPositionHelper.SetPosition(wings, WingPosition.BRAKING);
                    blockFlap = true;
                }
                else if (WingworksStats.IsDiving(__instance.Pos))
                {
                    WingPositionHelper.SetPosition(wings, WingPosition.DIVING);
                }
                else
                {
                    WingPositionHelper.SetPosition(wings, WingPosition.EXPANDED);
                }
                if (ft >= 1.25f)
                {
                    wings.SetFloat("time", 0);
                    WingworksStats.OnDefaultedStat(__instance.Stats, "ww_flight_hunger", ModConfig.Instance.FlightHunger, (hungerDrain) =>
                    {
                        EntityBehaviorHunger hunger = __instance.GetBehavior<EntityBehaviorHunger>();
                        if (hunger != null && __instance is EntityPlayer player && WingworksStats.ShouldUseHunger(player))
                        {
                            hunger.ConsumeSaturation(hungerDrain);
                        }
                    });
                }

                else
                {
                    wings.SetFloat("time", ft);
                }
                if (t > 1.25F)
                {
                    if(__instance.Controls.Jump && !blockFlap)
                    {
                        t = t - 1.25F;
                        wings.SetFloat("flap",t);
                    }
                    else
                    {
                        wings.SetFloat("flap", -1);
                        t = -1;
                    }
                }
                if (t < 0 && __instance.Controls.Jump && !blockFlap)
                {
                    wings.SetFloat("flap", 0);
                    var pitchVerticalCoefficient = 1 - Math.Min(0f, WingworksStats.GetPitchFrac(__instance.Pos));
                    WingworksStats.OnDefaultedStat(__instance.Stats, "ww_flap_hunger", ModConfig.Instance.FlapHunger, (hungerDrain) =>
                    {
                        EntityBehaviorHunger hunger = __instance.GetBehavior<EntityBehaviorHunger>();
                        if (hunger != null && __instance is EntityPlayer player && WingworksStats.ShouldUseHunger(player))
                        {
                            hunger.ConsumeSaturation(hungerDrain * pitchVerticalCoefficient);
                        }
                    });
                }
            }
            else
            {
                wings.SetFloat("flap", -1);
            }
            __instance.WatchedAttributes.MarkPathDirty("wingworks");
        }
        return true;
    }
}
