using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common.Entities;

namespace Wingworks.API;

public abstract class OverrideAnimationTrigger : AnimationTrigger, ISpecialAnimationTrigger
{

    public OverrideAnimationTrigger(AnimationTrigger original)
    {
        if (original != null)
        {
            this.DefaultAnim = original.DefaultAnim;
            this.MatchExact = original.MatchExact;
            this.OnControls = original.OnControls;
        }
    }
    public abstract bool Matches(Entity entity, int controls);

    public virtual bool ShouldDoDefaultChecksAdditionally()
    {
        return true;
    }
}
