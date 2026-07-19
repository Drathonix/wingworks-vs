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
        return t > 0 && t < threshold;
    }

    public static bool IsDiving(Entity entity)
    {
        ITreeAttribute wings = entity.WatchedAttributes.GetOrAddTreeAttribute("wingworks");
        return !DoFlapAnimation(entity, 0.115F) && WingPositionHelper.GetPosition(wings) == WingPosition.DIVING;
    }

    public static bool IsBraking(Entity entity)
    {
        ITreeAttribute wings = entity.WatchedAttributes.GetOrAddTreeAttribute("wingworks");
        return !DoFlapAnimation(entity, 0.115F) && WingPositionHelper.GetPosition(wings) == WingPosition.BRAKING;
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
            return !DoFlapAnimation(entity,0.9F) && !entity.OnGround && !IsDiving(entity);
        }
    }
}
