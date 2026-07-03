# Wingworks

Wingworks is a mod which grants players with the "ww_can_fly" trait the ability to gain speed and height at the cost of hunger, originally intended as an addon for Kestrelcrow's Dragonfolk mod.
Wingworks is ALSO a fork of Glider Revamp which improves the glider usability substantially which is what the dragon glide trait uses.

This mod only really exists because of the following the original glider implementation sucks and I thought it was silly that we give our player models wings and they can't even be flapped. We're throwing so hard~

## Dependencies
This mod depends on [Player Model lib](https://mods.vintagestory.at/show/mod/23112),
                    [Json Patches lib](https://mods.vintagestory.at/jsonpatcheslib),
                    [ConfigLib](https://mods.vintagestory.at/configlib)
In addition it already has compatibility built in with [KCs Dragon Player!](https://mods.vintagestory.at/kcsdragons)
This mod effectively disables [KCs Dragon Gliding's](https://mods.vintagestory.at/show/mod/57153), having it installed will do nothing bad!

### Traits
Most of these traits override some value within the mod config, use them to buff or debuff traits for fliers.

1. "ww_can_fly" (0 or 1): Grants the ability to flap wings. This is 
2. "ww_flap_hunger" [-1-infinity]: The hunger drained per flap. -1 to disable
3. "ww_flight_hunger" [-1-infinity]: The hunger drained per second flying. -1 to disable.
4. "ww_flap_vertical_acceleration" [0-infinity]: Vertical acceleration per flap. Applied over 21 animation frames. Default: 4
5. "ww_flap_forward_acceleration" [0-infinity]: Forward acceleration. Applied over 21 animation frames. Default 4/19
6. "ww_pitch_hunger_multiplier" [0.0-infinity]: Multiplies the hunger consumed when the player flaps while aiming up.
7. "ww_pitch_vertical_multiplier" [0.0-infinity]: Multiplies the vertical acceleration granted when the player flaps while aiming up. (these help you gain fight gravity at a significant cost)
8. "ww_pitch_forward_multiplier" [0.0-infinity]: Multiplies forward acceleration granted when the player flaps while aiming up.
9. "ww_top_speed" [0.0-infinity]: The top speed the player can reach while flying in any direction.
10. "ww_start_speed" [-2-infinity]: The minimum required speed to start flight. Keep above stall speed. Set to -2 to disable. (Higher speeds require falling further distances to start, lower allows lift off without a falling start)
11. "ww_stall_speed" [-2-infinity]: The minimum required speed to maintain flight. Set to -2 to disable. (Mechanic introduced by glider revamp that doesn't really fit in for natural fliers honestly)
12. "ww_climb_coefficient" [0.0-infinity]: The amount of speed lost per meter of height gained.
13. "ww_turn_rate" [0.0-infinity]: The degrees per second you can turn.
14. "ww_drag_coefficient" [0.0-1.0]: %Speed lost to drag.

I have already modified the traits of KC's dragons in this mod. You can patch the patches or fork my project if you wish to nerf them.

### Animations
I'm listing unimplemented code animations in case someone wants to add them in the future, it will definitely incentivise me to add these animations codewise if someone handles the actual animating part.

1. ww_flap: called when the player is gliding and presses the jump button off cooldown.
2. ww_dive (NOT YET IMPLEMENTED CODEWISE): called when the player aims towards the direction of gravity.
3. ww_dive_flap (NOT YET IMPLEMENTED CODEWISE): called while in a dive and the jump button.
4. ww_ascend (NOT YET IMPLEMENTED CODEWISE): called while the player aims against from the direction of gravity.
5. ww_ascend_flap (NOT YET IMPLEMENTED CODEWISE): called while ascending and the jump button is pressed.
6. ww_brake (NOT YET IMPLEMENTED CODEWISE): called when the player presses the backwards key in flight.

### Plans
There are plans to improve this mod in the future. I have a lot that needs fixing and improvement here's a list:

1. More animations: we need a better animation for when the player is ascending and diving (determined by vertical and horizontal speed).
2. Redo how animations are handled? the flap animation is made by myself, I am not usually an animator. Animating the wings was a NIGHTMARE. For some reason the animation software would play the animation completely differently from in game, I had to manually modify the JSON to get them to where they are now and even then there's still bugs. This is a mod issue I'm preety sure, has nothing to do with VSMC2.
3. Rolling: Mentioned by the Glider revamp dev as well, I want the character to be able to turn sharply by rolling left and right, also I want barrel rolls. Downside is that I SUCK at math and that's going to be a lot of rotation code.
4. Separate this mod from Glider Revamp (includes moving to a non-fork repo). Possibly move my kcsdragons animation patches to the mod's actual shape jsons (up to KC if she wants to do this).
5. Restructure the project, possibly move the animation trigger system I'm using to some public lib.

### Contributing
Everyone is welcome!

# [Glider Revamp](https://github.com/Hunter404/vs-glider-revamp)
I forked glider revamp as it was not updated to the most recent version of the game by the original maintainer and its mechanics are better than the original even if a little buggy.

## Core functionality

An attempt to enhance the glider physics system.
The mod replaces Vintage Story's vanilla glider mechanics with a simple and probably funner flight model that gives players more control and realistic gliding behavior. While decreasing the incentive of building tower freeways.

## Key Features

### Core Mechanics
- **Speed-Based Flight Dynamics**: Players must maintain a minimum speed to keep gliding. Below the stall speed, the glider deactivates and the player free falls, preventing passive hovering.
- **Energy Management System**: Climbing, turning, and air resistance all drain speed. This prevents infinite flying and makes gliding a skill-based activity requiring careful energy management.
- **Terminal Velocity Cap**: Maximum achievable speed prevents unrealistic velocities and balances gameplay.

### Customizable Settings

**Climb Coefficiency** (default: 0.2)
- Controls energy loss when climbing upward
- Example: 0.2 means climbing 1 meter costs 0.2 m/s of speed, and descending 1 meter adds 0.2 m/s of speed.
- Higher values make climbing and descending more impactful on speed, while lower values allow for more forgiving altitude changes and smooth glide.

**Turn Rate** (default: 90 degrees/second)
- Controls how quickly the glider can change the direction
- Example: 90°/s means a 180° turn takes 2 seconds
- Lower values create slower, more graceful turns; higher values allow snappier maneuvers

**Drag Coefficiency** (default: 0.1)
- Air resistance that slows the glider over time
- Example: 0.1 means 10% of speed² is lost to drag
- Balances maximum achievable distances and prevents tower highways
- Higher values make it harder to maintain speed

**Stall Speed** (default: 4.0 m/s)
- Minimum speed required to maintain a glide
- Below this speed, the glider automatically deactivates and the player free falls
- Sets the baseline for minimum speed management

**Activation Speed** (default: 8.0 m/s)
- Minimum speed required to initially activate the glider
- Prevents accidental glider activation during normal jumping or falling
- Typically, set higher than stall speed to provide a buffer

**Terminal Velocity** (default: 40.0 m/s)
- Maximum speed the glider can achieve
- Prevents players from reaching unrealistic velocities
- Creates a natural speed ceiling for balanced gameplay

### Quality of Life Features
- **Speed Display**: Optional HUD element showing current glide speed in m/s while flying
- **Activation Gate**: Glider only activates when moving fast enough, preventing awkward early-flight situations
- **Fully Configurable**: All physics parameters can be adjusted without code changes through ConfigLib

### Roadmap

- [x] Implement FOV changes based on speed for enhanced immersion
- [x] Add sound effects that vary with speed and maneuvers
- [ ] Add roll animation and make the player face flight trajectory for visual feedback
