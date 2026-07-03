using System;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;

namespace Wingworks;

public class WingFlapTrigger(AnimationTrigger original) : API.OverrideAnimationTrigger(original)
{

    public static bool DoFlapAnimation(Entity entity, float threshold = 1)
    {
        ITreeAttribute wings = entity.WatchedAttributes.GetOrAddTreeAttribute("wingworks");
        float t = wings.GetFloat("flap");
        return t > 0 && t < threshold;
    }
    public override bool Matches(Entity entity, int controls)
    {
        //TODO: this is a bandaid                  VVVV          The animation continues to attempt to run when grounded so I'm manually disabling it if the check fails.
        return DoFlapAnimation(entity) && !entity.OnGround;
    }

    public class NoGlide(AnimationTrigger original) : API.OverrideAnimationTrigger(original)
    {
        public override bool Matches(Entity entity, int controls)
        {
            //Console.WriteLine(entity.WatchedAttributes.GetFloat("flapDuration"));
            return !DoFlapAnimation(entity,0.9F) && !entity.OnGround;
        }

        public override bool ShouldDoDefaultChecksAdditionally()
        {
            return true;
        }
    }
}
