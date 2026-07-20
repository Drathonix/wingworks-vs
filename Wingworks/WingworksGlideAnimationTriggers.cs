using System;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Wingworks.API;

namespace Wingworks;

public class WingworksGlideAnimationTriggers
{

    public static bool DoFlapAnimation(Entity entity, float threshold = 1.25F)
    {
        ITreeAttribute wings = entity.WatchedAttributes.GetOrAddTreeAttribute("wingworks");
        float t = wings.GetFloat("flap");
        return IsAnimRunning(entity,"ww_flap") || (t > 0 && t < threshold);
    }

    public static bool IsAnimRunning(Entity entity, string code)
    {
        RunningAnimation anim = entity.AnimManager.GetAnimationState(code);
        return anim != null && anim.Running && anim.CurrentFrame <= 24;
    }

    public static bool isExpanded(Entity entity)
    {
        ITreeAttribute wings = entity.WatchedAttributes.GetOrAddTreeAttribute("wingworks");
        return WingPositionHelper.GetPosition(wings) == WingPosition.EXPANDED;
    }

    public static bool IsDiving(Entity entity)
    {
        ITreeAttribute wings = entity.WatchedAttributes.GetOrAddTreeAttribute("wingworks");
        return !DoFlapAnimation(entity) && (IsAnimRunning(entity,"ww_dive") || WingPositionHelper.GetPosition(wings) == WingPosition.DIVING);
    }

    public static bool IsBraking(Entity entity)
    {
        ITreeAttribute wings = entity.WatchedAttributes.GetOrAddTreeAttribute("wingworks");
        return !DoFlapAnimation(entity) && (IsAnimRunning(entity, "ww_brake") || WingPositionHelper.GetPosition(wings) == WingPosition.BRAKING);
    }

    public class Flap(AnimationTrigger original) : API.OverrideAnimationTrigger(original)
    {
        public override bool Matches(Entity entity, int controls)
        {
            return DoFlapAnimation(entity);
        }
    }

    public class Dive(AnimationTrigger original) : API.OverrideAnimationTrigger(original)
    {
        public override bool Matches(Entity entity, int controls)
        {
            return IsDiving(entity);
        }
    }

    public class Brake(AnimationTrigger original) : API.OverrideAnimationTrigger(original)
    {
        public override bool Matches(Entity entity, int controls)
        {
            return IsBraking(entity);
        }
    }
    public class NoGlide(AnimationTrigger original) : API.OverrideAnimationTrigger(original)
    {
        public override bool Matches(Entity entity, int controls)
        {
            //Console.WriteLine(!DoFlapAnimation(entity,1.15F) + " && " + isExpanded(entity));
            return !DoFlapAnimation(entity) && isExpanded(entity);
        }
    }
}
