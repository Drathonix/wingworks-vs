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
            //TODO player args
            if (wings.GetFloat("flap") > 9f / 30f)
            {
                // Bonus velocity when looking up at the cost of greater hunger drain.
                var pitchVerticalCoefficient = Math.Max(1, 1+Math.PI - entity.Pos.Pitch);
                WingworksStats.OnDefaultedStat(entity.Stats, "ww_flap_vertical_acceleration", ModConfig.Instance.FlapVerticalBoost, (gainTickY) =>
                {
                    WingworksStats.OnDefaultedStat(entity.Stats, "ww_pitch_vertical_multiplier", 0.5F, (val) =>
                    {
                        pos.Motion.Y += gainTickY / 21F * dt * pitchVerticalCoefficient * val;
                    });
                });
                WingworksStats.OnDefaultedStat(entity.Stats, "ww_flap_forward_acceleration", ModConfig.Instance.FlapForwardBoost, (gainTickY) =>
                {
                    WingworksStats.OnDefaultedStat(entity.Stats, "ww_pitch_forward_multiplier", 1, (val) =>
                    {
                        controls.GlideSpeed += (gainTickY / 21F * val) * dt * pitchVerticalCoefficient;
                    });
                });
            }
            // TODO figure out rolling.
            // Yes I have no idea what I'm doing, I'm bad at math! If someone else knows how to roll the character around its own x acis please let me know! All the results are for unity games and this isn't one of those.
            /*if (controls.Right)
            {
                //if(entity is EntityPlayer player){
                //    player.CameraPos
                //}
                *var mat = Matrix4x4.CreateFromYawPitchRoll(entity.Pos.Yaw, entity.Pos.Pitch, entity.Pos.Roll);
                mat = mat*Matrix4x4.CreateRotationX(-0.01F);
                Matrix4x4.Decompose(mat,out var xyz,out var quat,out var scale);
                entity.Pos.Roll = quat.X;
                entity.Pos.Yaw = quat.Y;
                entity.Pos.Pitch = quat.Z;
                //entity.Pos.Roll = Math.Min(entity.Pos.Roll, 2F);
            }
            if (controls.Left)
            {
                entity.Pos.Roll += 0.01F;
                //entity.Pos.Roll = Math.Min(entity.Pos.Roll, 2F);
            }
            if (!controls.Left && !controls.Right)
            {
                //if()
                // entity.Pos.Roll += 0.01F;
            }*/
        }
        return true;
    }
}

//grounded: no flap anim, no cooldown.
//glide: no flap anim, no cooldown.
