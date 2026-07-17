using Cairo.Freetype;
using ConfigLib;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Wingworks.API;

namespace Wingworks;

public class WingworksPModuleFlight
{


    public const float Gravity = 0.05f / 4f;
    public const float FlapFrameCount = 30f;
    public const float FlapPhysicsStart = 9f;

    /// <summary>
    /// Gravity adds a downward vector to the entity's motion. Gravity's effect can be increased or reduced by modifying the "ww_gravity_coefficient" stat.
    /// Calculated using the player's pitch with aiming peripendicular to gravity's vector having the most significant reduction and parallel having 0 reduction.
    /// Gravity will always be negative or zero.
    /// TODO: Add support for atmospheric density in calculations (higher heights = lower gravity reduction, also maybe someone will add new dimensions with unique atmospheres)
    /// </summary>
    /// <param name="dt">Time in seconds</param>
    /// <param name="entity"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static float GravityAcceleration(Entity entity, EntityPos pos, WingPosition position)
    {
        /*var gravityMult = WingworksStats.GetOrDefault(entity.Stats, "ww_gravity_coefficient",1.0F);
        // [0.05-1.0]
        // Gravity reduction based on player orientation and wing position.
        var liftCoefficient = Math.Min(((Math.Abs(GetPitchFrac(pos))/float.Pi)+0.05F)*WingPositionHelper.GetGravityCoefficient(position),1F);
        //                 VVV I'm doing this for readability, it will be a standard everywhere. The compiler optimizes this sort of thing away, do not worry.
        return Math.Min(0F,-1*gravityMult*Gravity*liftCoefficient);*/
        return 0f;
    }

    public static float GetGravityFrac(Entity entity, EntityPos pos, WingPosition position)
    {
        //-1-1
        var frac = GetPitchFrac(pos);
        if(frac < 0)
        {
            return 0;
        }
        frac = frac/WingPositionHelper.GetGravityCoefficient(position);
        return frac;
    }
    /// <summary>
    /// Drag reduces speed due to air resistance. Moving faster increases this value. Additionally, wing position should effect this with diving having a reduced drag.
    /// TODO: Add support for atmospheric density in calculations (higher heights = lower gravity reduction, also maybe someone will add new dimensions with unique atmospheres)
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="entity"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static float DragFrac(Entity entity, EntityPos pos, EntityControls controls, WingPosition position)
    {
        var dragMult = WingworksStats.GetOrDefault(entity.Stats, "ww_drag_coefficient", ModConfig.Instance.DragCoefficiency);
        var terminalVelocity = Math.Max(0.00001F,WingworksStats.GetOrDefault(entity.Stats, "ww_max_speed", ModConfig.Instance.TerminalVelocityMs));
        var velocityFrac = (float)((Math.Pow(controls.GlideSpeed,2F) / (terminalVelocity*10F))/terminalVelocity);
        return Math.Max(0F,1-(dragMult*WingPositionHelper.GetDragCoefficient(position)*velocityFrac));
    }

    /// <summary>
    /// Returns [0-pi] when aimed down and [-pi-0] when aimed up.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static float GetPitchFrac(EntityPos pos)
    {
        // Down is 2pi, Up is 0.
        return (float)(pos.Pitch - float.Pi);
    }

    /*// From: https://math.stackexchange.com/questions/2975109/how-to-convert-euler-angles-to-quaternions-and-get-the-same-euler-angles-back-fr
    public static float[] EulerToQuaternion(float yaw, float pitch, float roll) {
        float qx = (float)(Math.Sin(roll / 2) * Math.Cos(pitch / 2) * Math.Cos(yaw / 2) - Math.Cos(roll / 2) * Math.Sin(pitch / 2) * Math.Sin(yaw / 2));
        float qy = (float)(Math.Cos(roll / 2) * Math.Sin(pitch / 2) * Math.Cos(yaw / 2) + Math.Sin(roll / 2) * Math.Cos(pitch / 2) * Math.Sin(yaw / 2));
        float qz = (float)(Math.Cos(roll / 2) * Math.Cos(pitch / 2) * Math.Sin(yaw / 2) - Math.Sin(roll / 2) * Math.Sin(pitch / 2) * Math.Cos(yaw / 2));
        float qw = (float)(Math.Cos(roll / 2) * Math.Cos(pitch / 2) * Math.Cos(yaw / 2) + Math.Sin(roll / 2) * Math.Sin(pitch / 2) * Math.Sin(yaw / 2));
        return [qx, qy, qz, qw];
    }

    public static void ApplyRoll(Entity entity, EntityPos pos, EntityControls controls)
    {
        var view = pos.GetViewVector();
        pos.Roll = float.Pi/4;
        pos.Pitch = float.Pi/4;
        pos.Yaw = float.Pi/4;*/
        /*Console.WriteLine(view);
        float[] q = EulerToQuaternion(pos.Yaw, pos.Pitch, pos.Roll);

        float[] matrix = Mat4f.Create();
        Mat4f.FromQuat(matrix,q);

        Mat4f.Rotate(matrix, matrix, float.Pi / 180F, [0f,0f,1f]);
        float[] eulers = new float[3];
        Mat4f.ExtractEulerAngles(matrix,ref eulers[0], ref eulers[1], ref eulers[2]);
        Console.WriteLine(eulers[0] + ", " + eulers[1] + ", " + eulers[2]);
        //pos.Roll = eulers[0];
        //pos.Yaw = eulers[1];
        //pos.Pitch = eulers[2];

    }*/

    private static Vec3d RotateTowards(Vec3d fromDir, Vec3d toDir, double maxRadians)
    {
        fromDir = fromDir.Normalize();
        toDir = toDir.Normalize();

        var dot = GameMath.Clamp(fromDir.Dot(toDir), -1, 1);
        var angle = Math.Acos(dot);
        if (angle < 1e-6) return toDir;

        var t = Math.Min(1.0, maxRadians / angle);

        // Slerp on the unit sphere
        var sinAngle = Math.Sin(angle);
        var a = Math.Sin((1 - t) * angle) / sinAngle;
        var b = Math.Sin(t * angle) / sinAngle;

        var blended = fromDir * a + toDir * b;
        return blended.Normalize();
    }

    public static void ApplyTurning(Entity entity, EntityPos pos, EntityControls controls)
    {
        WingworksStats.OnDefaultedStat(entity.Stats, "ww_turn_rate", ModConfig.Instance.TurnRate, (deg) =>
        {
            var spd = pos.Motion.Length();
            var g = pos.Motion.Y;
            var view = pos.GetViewVector().ToVec3d();
            pos.Motion.Y = 0F;
            view.Y = 0F;
            pos.Motion = RotateTowards(pos.Motion, view, deg * float.Pi / 180F);
            pos.Motion *= spd;
            pos.Motion.Y = g;
            controls.GlideSpeed = pos.Motion.Length();
        });
    }

    public static void ApplyPoweredPhysics(Entity entity, EntityPos pos, EntityControls controls) {

    }
    public static void ApplyGlidePhysics(float dt, Entity entity, EntityPos pos, EntityControls controls)
    {
        ITreeAttribute wings = entity.WatchedAttributes.GetOrAddTreeAttribute("wingworks");
        WingPosition wingPosition = WingPositionHelper.GetPosition(wings);
        var drag = DragFrac(entity, pos, controls,wingPosition);
        // Apply Drag
        //pos.Motion *= drag;
        // Apply Gravity
        //var gravity = GravityAcceleration(entity, pos, wingPosition);
        //pos.Motion.Y += gravity;
        var forwardMotionPercentage = GetGravityFrac(entity, pos, wingPosition);
        Console.WriteLine(forwardMotionPercentage);
        var forwardMotion = (-pos.Motion.Y * forwardMotionPercentage)*dt;

        var speed = pos.Motion.Length();

        //Console.WriteLine(forwardMotion);

        pos.Motion.Y += forwardMotion;
        pos.Motion = pos.Motion.Normalize();

        var view = pos.GetViewVector().ToVec3d();
        view = view.Normalize();

        //Console.WriteLine(forwardMotion + " : " + view + " : " + view*forwardMotion);
        pos.Motion = (pos.Motion * speed)+(view*forwardMotion);

        controls.GlideSpeed = pos.Motion.Length();
    }

    public static bool ApplyFlying(float dt, Entity entity, EntityPos pos, EntityControls controls)
    {
        if (entity is EntityPlayer player)
        {
            if (!WingworksStats.CanFly(entity.Stats) || !controls.Gliding)
            {
                return true;
            }
            ApplyGlidePhysics(dt, entity, pos, controls);
            ApplyTurning(entity, pos, controls);
            //ApplyPoweredPhysics(entity, pos, controls);
            return false;
            /*ITreeAttribute wings = entity.WatchedAttributes.GetOrAddTreeAttribute("wingworks");
            var config = ModConfig.Instance;
            //TODO player args
            if (wings.GetFloat("flap") > 9f / 30f)
            {
                if (wings.GetBool("brake"))
                {

                }
                else
                {
                    // Bonus velocity when looking up at the cost of greater hunger drain.
                    var pitchVerticalCoefficient = Math.Max(1, 1 + Math.PI - entity.Pos.Pitch);
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
            }*/
        }
        return true;
    }

}
