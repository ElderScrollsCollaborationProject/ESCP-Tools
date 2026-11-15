# Legacy code examples

Code from the old ESCP mods is in `Source/ESCPTools/Legacy`, in its original namespaces. It doesn't need HAR or Vanilla Expanded.

The game doesn't load this folder. Copy what you need into your own mod's `Defs` folder and rename the defs.

- `Example_Regeneration.xml`: healing hediff and comp, instant-heal drink, extra butcher items, wild spawns limited by map features
- `Example_Spawners.xml`: tree that releases a creature when chopped, tree that grows one on a timer
- `Example_Skyshard.xml`: use-once building that raises a random skill
- `Example_Thralls.xml`: raising corpses as thralls, with a limit based on skill

Thrall patches only run when a hediff named `ESCP_NecromanticThralls_Enthralled` exists.

Some classes look for fixed def names, listed in the `DefOf` classes under `Legacy`. They're optional, but that feature won't work without them.

The old settings screens are hidden until a content mod uses them, so everything runs on default settings.
