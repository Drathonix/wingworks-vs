# Wingworks 1.1.3 - Multiplayer Fixes
1. Updated to Glider Revamp V1.0.3 - Fixes all multiplayer animation issues.
2. Made some server-side code also execute on the client.

# Wingworks 1.1.0 - The Official Release
Heya! This update took multiple weeks of debugging and tweaking to get out, I hope you all enjoy it.

## Features
1. Vertical flight speed gain is actually possible now, you can go up without having to dive first, that being said it still helps!
2. Added a bonus flap multiplier when speed is low enough to improve your ability to reach speed faster. This is modified by the new "ww_flap_min_speed" trait.
3. Added a braking mechanic, press your backwards key to slow down! This also has custom animation support.
4. Added diving animation support for when aiming down.
5. Tweaked the Draconlet's stats (included in KCDF)

Massive shoutout to kestrelcrow for all her work on the new animations in this update!

## Fixes
1. Fixed ALL glide and flap animation stutters on the client side. In addition dive and brake animations will not stutter either. Unfortunately MP animations are still broken because of a vanilla bug.
2. Fixed a vanilla specific issue that caused occassional glide animation stutters (the patch for this is in my Glider Revamp fork)
3. Fixed some incorrect food usage calculations (flap vertical cost multiplier was being applied to the passive flight hunger drain instead of flap hunger drain).
4. No longer computing flight and animation timings on the client side at all.
5. Incorrect pitch multiplier calculations.

## Removed
1. Poorly implemented traits "ww_pitch_vertical_multiplier" and "ww_pitch_forward_multiplier", these really had no good effect on the speed gain math and I had to take them out.

## Codebase Improvements
1. Vastly improved internal structure. Many patches have either been simplified, moved, or removed.
2. Moved all glider revamp code to my own fork and made it a required dependency. I chose to do this as a way to separate credit for both projects, that doesn't mean that the fork I've created doesn't have some work put into it though, it is an improvement to the original and provides this mod necessary integration features.
3. Removed all json patches applied to kestrelcrow's mods, they're now integrated in KCDF directly!

## Primary Objective
At the moment my main objective is to fix the multiplayer animation desync issue with flight animations. Creativefly and gliding animations are both unsynced in vanilla.