# Modded / All Maps edition
This fork reads what it needs straight from the `.map` file MCC has loaded, so **File > Import > Forge Objects** works on any map, stock or modded, in one click:

- **Palette from the map.** Forge object IDs are resolved from the loaded map's own Forge palette instead of built-in tables, so modded maps with added or reordered items (e.g. green screen mods) import and export with the right objects.
- **Models extracted automatically.** Objects with no mesh in `forge.blend` (vehicles, modded props) are built from the map's render models, including variant attachments and vehicle weapons. They also appear under Add > Forge Object.
- **Textures.** Extracted models and levels get materials from the map's shaders: base map with tint, normal map, self-illumination, alpha. Images are packed into the .blend.
- **Every map's level.** The first import on a map builds its textured level geometry into its own scene; your working scene shows it as the "Map" backdrop, the same way Forge World works. Outdated builds are rebuilt automatically.
- **Fixes:** Anvil scale convention now really defaults to 47X; scaled objects no longer import at 100% when something is selected; Anvil yellow-team and scale-zone rules; readable errors when ForgeBridge.dll fails.

Settings are under Properties > Scene > Forge Scene > Map Palette. If a model faces the wrong way (some ported models), right-click > **Rotate Model 90°** fixes every copy.

`mjolnir_mapreader.py` also runs on its own (Python 3, no Blender needed) to inspect a map:
```
python mjolnir_mapreader.py path\to\map.map                       # palette with IDs and tag paths
python mjolnir_mapreader.py path\to\map.map --obj out --textures tex   # palette models as .obj + textures as .png
python mjolnir_mapreader.py path\to\map.map --bsp level             # level geometry as .obj
```

Map file format knowledge is ported from [Reclaimer](https://github.com/Gravemind2401/Reclaimer) (GPL-3.0) and [Assembly](https://github.com/XboxChaos/Assembly)'s plugin definitions.

# OwO, What's This?
Mjolnir Forge Editor is a tool *(based off [Halo Reach Forge Assistant](https://github.com/Lookenpeepers/Halo-Reach-Forge-Assistant))*, for the popular free 3D software [Blender](https://www.blender.org/), which modifies Halo Reach's RAM to do more advanced edits to map variants which would otherwise be very tedious or even impossible ingame (using a dll).
The currently loaded forge map is imported into Blender, edited, and then exported back to Reach. After a round restart, changes take effect and can be saved back to the forge map variant.

![](https://raw.githubusercontent.com/Waffle1434/Mjolnir-Forge-Editor/master/Wiki/rotate.gif)
![image](https://user-images.githubusercontent.com/8021358/131237927-73a4cc06-10fc-4464-b3f6-7fe21c5d5c1c.png)
![image](https://user-images.githubusercontent.com/8021358/131237982-65f77671-6130-4ad8-9697-92793bd5b101.png)

*Halo: The Master Chief Collection © Microsoft Corporation. Mjolnir Forge Editor was created under Microsoft's ["Game Content Usage Rules"](https://www.xbox.com/en-US/developers/rules) using assets from Halo: Reach, and it is not endorsed by or affiliated with Microsoft.*

# Installation & Setup
Please follow the [Wiki Guide](https://github.com/Waffle1434/Mjolnir-Forge-Editor/wiki/1.-Getting-Started).


# Compilation [Optional]
1. Open the `Mjolnir.sln` file with [Visual Studio](https://visualstudio.microsoft.com/).
2. Hit the "Start" button, or `Debug -> Start With Debugging`.
3. Install the [Microsoft Build Tools 2015](https://www.microsoft.com/en-us/download/details.aspx?id=48159)
