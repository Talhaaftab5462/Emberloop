EMBERLOOP — ONE FINAL REBUILD
=============================

This rebuild DOES NOT modify or depend on the old MainLevel layout.
It generates a completely new submission scene from scratch using the working time-loop code, supplied audio, and safe procedural geometry.

HOW TO USE
----------
1. Open this project in Unity 6.
2. Wait for all scripts/assets to compile. There should be no red compile errors.
3. Top menu: Emberloop > ONE FINAL REBUILD
4. Wait for the confirmation dialog.
5. Unity creates and saves: Assets/Scenes/Emberloop_Rebuilt.unity
6. That scene is also set as the only enabled Build Settings scene.
7. Open Emberloop_Rebuilt if it is not already open, then press Play.

DO NOT run any older polish/emergency builders.

CORE DESIGN
-----------
I — THE ECHO CRYPT
Learn Time + Shadow. Record yourself on a blue rune; next loop your echo holds it while you pass the gate.

II — THE EMBER VAULT
Fire becomes active gameplay. Take a Spark and carry it to the dark brazier. The brazier remains lit across time resets. The orange rune does not work for player OR echo until it is inside firelight. Record a route to the rune, then use the echo on the next loop.

III — THE SPLIT SANCTUM
A large chasm room with two routes. Light the right-side brazier. Then program two different echoes onto the blue and firebound orange runes. Both must be active simultaneously to open the north gate.

IV — THE RITUAL OF THREE
A large, tall final ritual hall. Light the final brazier, program three echoes onto three sigils, then step into the central beacon when all three align.

RULES
-----
- R rewinds early.
- Fire lit in a brazier persists through loops.
- A carried Spark DOES NOT itself activate orange runes. It must be placed into a brazier.
- Blue runes work normally.
- Orange runes only work inside the light radius of an ignited brazier.
- Checkpoints clear old echoes without teleporting you on activation.
- Falling into the sanctum pits rewinds the loop instead of soft-locking.

PRESENTATION
------------
- Fully enclosed dungeon: floors, walls, tiled ceilings, corridors, no exterior gaps.
- Custom stone/wood textures and normal maps generated for this rebuild.
- Dungeon doors are double-leaf wooden/iron gates that swing outward.
- Fire uses animated transparent flame cards + flickering point lights: NO particle systems / no particle-velocity console spam.
- Echoes are humanoid silhouettes instead of capsules and change color inside firelight.
- Spatial fire/door/rune audio, footsteps, ambience, reset sound, HUD, intro fade, fog, torches, columns, rubble, pits, final ritual audio and ending.

Suggested full first-time runtime: ~6–10 minutes depending on puzzle discovery and rewinds.
