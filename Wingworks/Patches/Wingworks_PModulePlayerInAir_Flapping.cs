using ConfigLib;
using Wingworks.API;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Vintagestory;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.Server;
using System.Diagnostics;

namespace Wingworks.Patches;

[HarmonyPatch(typeof(PModulePlayerInAir), nameof(PModulePlayerInAir.ApplyFlying)), HarmonyPriority(401)]
public class Wingworks_PModulePlayerInAir_Flapping
{
    internal static bool Prefix(PModulePlayerInAir __instance, float dt, Entity entity, EntityPos pos, EntityControls controls)
    {
        if (entity is EntityPlayer player)
        {
            if (!WingworksStats.CanFly(entity.Stats) || !controls.Gliding)
            {
                return true;
            }
            ITreeAttribute wings = entity.WatchedAttributes.GetOrAddTreeAttribute("wingworks");
            var config = ModConfig.Instance;
            WingPosition position = WingPositionHelper.GetPosition(wings);
            if (position == WingPosition.BRAKING)
            {
                WingworksStats.OnDefaultedStat(entity.Stats, "ww_brake_decceleration", 0.065F, (gainTick) =>
                {
                    controls.GlideSpeed -= gainTick * dt;
                });
            }

            Console.WriteLine(controls.GlideSpeed);
            if (wings.GetFloat("flap") > 9f / 24f)
            {
                // Bonus velocity when looking up at the cost of greater hunger drain.
                var pitchVerticalCoefficient = 1 - Math.Min(0f, WingworksStats.GetPitchFrac(pos));
                var pitchForwardMultiplier = Math.Clamp(float.Pow(pitchVerticalCoefficient,0.3f/(float)controls.GlideSpeed*2F)*1.5F,1F,5.5F);
                WingworksStats.OnDefaultedStat(entity.Stats, "ww_flap_vertical_acceleration", ModConfig.Instance.FlapVerticalBoost, (gainTickY) =>
                {
                    WingworksStats.OnDefaultedStat(entity.Stats, "ww_pitch_vertical_multiplier", 0.5F, (val) =>
                    {
                        pos.Motion.Y += gainTickY / 15F * dt * val;// * pitchVerticalCoefficient * val;
                    });
                });
                WingworksStats.OnDefaultedStat(entity.Stats, "ww_flap_forward_acceleration", ModConfig.Instance.FlapForwardBoost, (gainTickF) =>
                {
                    WingworksStats.OnDefaultedStat(entity.Stats, "ww_pitch_forward_multiplier", 1, (val) =>
                    {
                        controls.GlideSpeed += (gainTickF / 15F) * dt * pitchForwardMultiplier * val;
                    });
                });
            }
        }
        return true; //return WingworksPModuleFlight.ApplyFlying(dt,entity,pos,controls);
    }
}

//grounded: no flap anim, no cooldown.
//glide: no flap anim, no cooldown.
