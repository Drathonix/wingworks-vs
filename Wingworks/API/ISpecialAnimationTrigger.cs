using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common.Entities;

namespace Wingworks.API;

/// <summary>
/// An AnimationTrigger that overrides the default animation trigger activation logic allowing animations to be triggered by non-controls
/// </summary>
public interface ISpecialAnimationTrigger 
{
    /// <summary>
    /// Whether the entity has triggered the animation.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    bool Matches(Entity entity, int controls);

    public virtual bool ShouldDoDefaultChecksAdditionally()
    {
        return false;
    }
}
