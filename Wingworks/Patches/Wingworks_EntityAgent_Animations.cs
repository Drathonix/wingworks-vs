using ConfigLib;
using Wingworks.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;

namespace Wingworks.Patches;

[HarmonyPatch(typeof(EntityAgent), nameof(EntityAgent.OnGameTick)), HarmonyPriority(401)]
public class Wingworks_EntityAgent_Animations
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        /*var found = false;
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(f_someField))
            {
                yield return new CodeInstruction(OpCodes.Call, m_MyExtraMethod);
                found = true;
            }
            yield return instruction;
        }
        if (found is false)
            ReportError("Cannot find <Stdfld someField> in OriginalType.OriginalMethod");
        */
        return Transpilers.MethodReplacer(instructions,
            typeof(AnimationMetaData).GetMethod("Matches"), typeof(Wingworks_EntityAgent_Animations).GetMethod("SpecMatch"));
    }

    public static bool SpecMatch(AnimationMetaData __instance, int controls)
    {
        if(__instance.TriggeredBy is ISpecialAnimationTrigger trigger)
        {
            if (trigger.ShouldDoDefaultChecksAdditionally() && !__instance.Matches(controls))
            {
                return false;
            }
            return trigger.Matches(cachedInst,controls);
        }
        return __instance.Matches(controls);
    }

    private static EntityAgent cachedInst;

    internal static bool Prefix(EntityAgent __instance, float dt)
    {
        cachedInst = __instance;
        if (WingworksStats.CanFly(__instance.Stats)) {
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
                if (__instance.Controls.Backward) {
                    WingPositionHelper.SetPosition(wings,WingPosition.BRAKING);
                    blockFlap = true;
                }
                else if (WingworksStats.IsDiving(__instance.Pos))
                {
                    WingPositionHelper.SetPosition(wings,WingPosition.DIVING);
                }
                else
                {
                    WingPositionHelper.SetPosition(wings, WingPosition.EXPANDED);
                }
                if (ft >= 1.25f)
                {
                    //TODO: redo this
                    //var pitchVerticalCoefficient = (float)Math.Max(1, 1+Math.PI - __instance.Pos.Pitch);
                    wings.SetFloat("time", 0);
                    WingworksStats.OnDefaultedStat(__instance.Stats, "ww_flight_hunger", ModConfig.Instance.FlightHunger, (hungerDrain) =>
                    {
                        EntityBehaviorHunger hunger = __instance.GetBehavior<EntityBehaviorHunger>();
                        if ((__instance is EntityPlayer player && WingworksStats.ShouldUseHunger(player)) && hunger != null)
                        {
                            hunger.ConsumeSaturation(hungerDrain);//* pitchVerticalCoefficient);
                        }
                    });
                }

                else
                {
                    wings.SetFloat("time", ft);
                }
                if (t > ModConfig.Instance.FlapCooldown)
                {
                    wings.SetFloat("flap", -1);
                }
                if (t < 0 && __instance.Controls.Jump && !blockFlap)
                {
                    wings.SetFloat("flap", 0);
                    WingworksStats.OnDefaultedStat(__instance.Stats, "ww_flap_hunger", ModConfig.Instance.FlapHunger, (hungerDrain) =>
                    {
                        EntityBehaviorHunger hunger = __instance.GetBehavior<EntityBehaviorHunger>();
                        if ((__instance is EntityPlayer player && WingworksStats.ShouldUseHunger(player)) && hunger != null)
                        {
                            hunger.ConsumeSaturation(hungerDrain);
                        }
                    });
                }
            } else
            {
                wings.SetFloat("flap", -1);
            }
        }
        /*cachedInst = __instance;
        AnimationMetaData[] animations = __instance.Properties.Client.Animations;
        for (int index = 0; animations != null && index < animations.Length; ++index)
        {
            AnimationMetaData animationMetaData = animations[index];
            bool wasActive = __instance.AnimManager.IsAnimationActive(animationMetaData.Animation);
            bool flag3 = animationMetaData != null && (animationMetaData.TriggeredBy != null ? animationMetaData.TriggeredBy.DefaultAnim : false);
            bool nowActive = animationMetaData.Matches((int)__instance.CurrentControls) || flag3 && __instance.CurrentControls == EnumEntityActivity.Idle;
            //Console.WriteLine(animationMetaData.Code);



            if (animationMetaData.Code.Equals("ww_flap"))
            {
                Console.WriteLine(__instance.CurrentControls.ToString());
                if (nowActive) {
                    Console.WriteLine("Activated");
                }
            }
        }*/
        return true;
    }
}
