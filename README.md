# Wingworks

Wingworks is an flight mechanics library that introduces flapping mechanics for players who possess the trait "ww_can_fly" as well as animation support for flight actions. Gliding and flying will consume additional hunger as a way to balance the mod somewhat.
## Dependencies
This mod depends on [Glider Revamp (Drathon's Fork)](https://mods.vintagestory.at/gliderrevampdrathon),
                    [ConfigLib](https://mods.vintagestory.at/configlib)

In addition it already has compatibility built in with [KCs Dragon Player!](https://mods.vintagestory.at/kcsdragons)

### Traits
Most of these traits override some value within the mod config, use them to buff or debuff traits for fliers.

1. "ww_can_fly" (0 or 1): Grants the ability to flap wings. This is 
2. "ww_flap_hunger" [-1-infinity]: The hunger drained per flap. -1 to disable
3. "ww_flight_hunger" [-1-infinity]: The hunger drained per second flying. -1 to disable.
4. "ww_flap_vertical_acceleration" [0-infinity]: Vertical acceleration per flap. Applied over 21 animation frames. Default: 4
5. "ww_flap_forward_acceleration" [0-infinity]: Forward acceleration. Applied over 21 animation frames. Default 4/19
6. "ww_pitch_hunger_multiplier" [0.0-infinity]: Multiplies the hunger consumed when the player flaps while aiming up.
7. ---"ww_pitch_vertical_multiplier" [0.0-infinity]: Multiplies the vertical acceleration granted when the player flaps while aiming up. (these help you gain fight gravity at a significant cost)--- Removed for the time being
8. ---"ww_pitch_forward_multiplier" [0.0-infinity]: Multiplies forward acceleration granted when the player flaps while aiming up.--- Removed for the time being
9. "ww_top_speed" [0.0-infinity]: The top speed the player can reach while flying in any direction.
10. "ww_start_speed" [-2-infinity]: The minimum required speed to start flight. Keep above stall speed. Set to -2 to disable. (Higher speeds require falling further distances to start, lower allows lift off without a falling start)
11. "ww_stall_speed" [-2-infinity]: The minimum required speed to maintain flight. Set to -2 to disable. (Mechanic introduced by glider revamp that doesn't really fit in for natural fliers honestly)
12. "ww_climb_coefficient" [0.0-infinity]: The amount of speed lost per meter of height gained.
13. "ww_turn_rate" [0.0-infinity]: The degrees per second you can turn.
14. "ww_drag_coefficient" [0.0-1.0]: %Speed lost to drag.
15. "ww_flap_min_speed" [0-infinity]: Minimum speed where a multiplier should be applied to increase initial flight speed.

I have already modified the traits of KC's dragons in this mod. You can patch the patches or fork my project if you wish to nerf or buff them.

### Animations
I'm listing unimplemented code animations in case someone wants to add them in the future, it will definitely incentivise me to add these animations codewise if someone handles the actual animating part.

1. ww_flap: called when the player is gliding and presses the jump button off cooldown.
2. ww_dive: called when the player aims towards the direction of gravity.
3. ww_dive_flap (NOT YET IMPLEMENTED CODEWISE): called while in a dive and the jump button.
4. ww_ascend (NOT YET IMPLEMENTED CODEWISE): called while the player aims against from the direction of gravity.
5. ww_ascend_flap (NOT YET IMPLEMENTED CODEWISE): called while ascending and the jump button is pressed.
6. ww_brake: called when the player presses the backwards key in flight.

### Plans
There are plans to improve this mod in the future. I have a lot that needs fixing and improvement here's a list:

1. More animations: we need a better animation for when the player is ascending and diving (determined by vertical and horizontal speed).
2. Rolling: Mentioned by the Glider revamp dev as well, I want the character to be able to turn sharply by rolling left and right, also I want barrel rolls. Downside is that I SUCK at math and that's going to be a lot of rotation code.
3. Restructure the project, possibly move the animation trigger system I'm using to some public lib.

### Contributing
Everyone is welcome!