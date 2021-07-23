# OwO, What's This?
Mjolnir Forge Editor is a tool *(based off [Halo Reach Forge Assistant](https://github.com/Lookenpeepers/Halo-Reach-Forge-Assistant))*, for the popular free 3D software [Blender](https://www.blender.org/), which modifies Halo Reach's RAM to edit forge maps using a dll.
The currently loaded forge map is imported into Blender, edited, and then exported back to Reach. After a round restart, changes take effect and can be saved back to the forge map variant.

*Currently only Forge World is supported.*

*Halo: The Master Chief Collection © Microsoft Corporation. Mjolnir Forge Editor was created under Microsoft's ["Game Content Usage Rules"](https://www.xbox.com/en-US/developers/rules) using assets from Halo: Reach, and it is not endorsed by or affiliated with Microsoft.*

# Installation & Setup
1. Install [Blender](https://www.blender.org/download/) (2.9 or newer)
2. Download the latest release from [Releases](https://github.com/Waffle1434/Halo-Reach-Forge-Assistant/releases).
3. Open `forge.blend` with Blender, open the `Scripting` tab, and hit Play on the `forge.py` script
![image](https://user-images.githubusercontent.com/8021358/129303262-65851004-4ec2-4dfb-9c58-84ed89c22d3e.png)
5. Run Halo Master Chief Collection (with Easy Anti-Cheat disabled) and load a map into Forge
6. In Blender, switch to the `Layout` tab for a fullscreen view, and `File -> Import -> Forge Objects`
![image](https://user-images.githubusercontent.com/8021358/129300385-b9ef6e55-0ad7-4071-91e0-5493372adfc8.png)
7. To export your forge map from Blender: `File -> Export -> Forge Objects`, and restart the round ingame.
![image](https://user-images.githubusercontent.com/8021358/129300600-a327e1f7-e7dd-4fcf-9b95-1250dadabaed.png)

# Quick Guide
Forge object properties can be edited in the `Forge` sidebar tab. To edit settings for *multiple objects*, hold down `Alt` while making changes.
![image](https://user-images.githubusercontent.com/8021358/129308087-e7cf3c25-c332-4e23-9eb6-790f178bbbdb.png)

Toggle Forge World Map for reference:

![image](https://user-images.githubusercontent.com/8021358/129307617-6085fe40-8792-4006-804d-adf1deb0fb34.png)


# Compilation [Optional]
1. Open the `ForgeAssistant.sln` file with [Visual Studio](https://visualstudio.microsoft.com/).
2. Hit the "Start" button, or `Debug -> Start With Debugging`.
3. Install the [Microsoft Build Tools 2015](https://www.microsoft.com/en-us/download/details.aspx?id=48159)
