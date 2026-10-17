EMBERLOOP - FINAL SUBMISSION PASS

This package intentionally removes the previous experimental polish builders.

1. Open Assets/Scenes/MainLevel.unity.
2. Wait until Unity finishes compiling.
3. If there are red compile errors, stop and fix them before continuing.
4. Run: Emberloop > FINAL Submission Pass
5. Save MainLevel (Ctrl+S).
6. Play-test once from start to finish.

Key design change:
- After Puzzle 1, the player picks up a portable Ember.
- Static puzzle fires no longer make shadows physical.
- Carry the Ember near a shadow when it reaches an ORANGE plate / fire-bound anchor.
- Non-fire plates glow BLUE. Fire-required plates glow ORANGE.

This makes FIRE an active gameplay tool instead of a decorative light.

The pass also:
- removes ALL scene ParticleSystems to eliminate particle curve spam;
- uses safe crossed flame cards instead of ParticleSystems;
- uses short one-shot footsteps;
- creates complete room/corridor ceilings with support beams;
- replaces vertical/portcullis doors with double swing dungeon doors;
- reduces light/shadow load;
- removes old generated polish clutter;
- seals the final rear arch intentionally;
- sets loop duration to 18 seconds.

Do not run any older polish/emergency builder after this pass.

EMBER IGNITION UPDATE
---------------------
Fire-bound orange plates now use a persistent per-loop ignition mechanic:
1. Pick up the Ember after Puzzle 1.
2. During the next loop, approach/touch the previous echo while carrying the Ember.
3. The echo turns orange and stays ignited until the loop resets.
4. That ignited echo can hold orange fire-bound plates while the player runs to the door.
5. Resetting the loop removes ignition, so each required echo must be ignited again on the loop where it matters.

Blue plates do not require ignition. Orange plates/anchors do.
