bl_info = {
    "name" : "Halo: Mjolnir Forge Editor",
    "author" : "Waffle1434",
    "description" : "Import/export Halo Reach Forge map variants.",
    "blender" : (2, 80, 0),
    "version" : (1, 0, 0),
    "location" : "",
    "warning" : "",
    "category" : "Import-Export",
    "doc_url": "https://github.com/Waffle1434/Mjolnir-Forge-Editor"
}

import os, sys

cur_dir = os.path.dirname(__file__)
if cur_dir not in sys.path: sys.path.append(cur_dir)

import mjolnir

# TODO: operators here?

def register():
    #mjolnir.register()
    print("register")

def unregister():
    #mjolnir.unregister()
    print("unregister")