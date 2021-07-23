import bpy, enum, time, blf, textwrap
from ctypes import *
from math import *
from mathutils import *
from bpy.props import *

maxObjectCount = 650
dllPath = bpy.path.abspath("//") + "ForgeBridge.dll"
forge = cdll.LoadLibrary(dllPath)

class float3(Structure):
    _fields_ = [ ('x', c_float), ('y', c_float), ('z', c_float) ]
    def cross(self, other): return float3(self.y * other.z - self.z * other.y, self.z * other.x - self.x * other.z, self.x * other.y - self.y * other.x)
    def fromVector(vec): return float3(vec.x,vec.y,vec.z)
    def toVector(self): return Vector((self.x,self.y,self.z))
    def __str__(self): return "(%.2f,%.2f,%.2f)"%(self.x,self.y,self.z)
class ForgeObject(Structure):
    _fields_ = [
        ('show', c_ushort),
        ('itemCategory', c_ushort),
        ('idExt', c_uint),
        ('position', float3),
        ('forward', float3),
        ('up', float3),
        ('spawnRelativeToMapIndex', c_ushort),
        ('itemVariant', c_ubyte),
        ('pad1', c_ubyte),
        ('width', c_float),
        ('length', c_float),
        ('top', c_float),
        ('bottom', c_float),
        ('shape', c_ubyte),
        ('spawnSequence', c_byte),
        ('spawnTime', c_ubyte),
        ('cachedType', c_ubyte),
        ('scriptLabelIndex', c_ushort),
        ('flags', c_ubyte),
        ('team', c_ubyte),
        ('otherInfoA', c_ubyte),
        ('otherInfoB', c_ubyte),
        ('color', c_ubyte),
        ('pad2', c_ubyte),
    ]
class ForgeObjectFlags(enum.IntFlag):
    PhysicsNormal = 0b00000000,
    PhysicsFixed  = 0b01000000,
    PhysicsPhased = 0b11000000,
    PhysicsMask   = 0b11000000,
    GameSpecific  = 0b00100000,
    Asymmetric    = 0b00001000,
    Symmetric     = 0b00000100,
    SymmetryMask  = 0b00001100,
    HideAtStart   = 0b00000010

forge.TrySetConnect.restype = c_bool
forge.GetLastError.restype = c_wchar_p
forge.GetMapName.restype = c_wchar_p
forge.GetObjectPtr.restype = POINTER(ForgeObject)
forge.ForgeObject_GetItemName.restype = c_wchar_p
forge.ItemNameToType.argtypes = [c_wchar_p]
forge.TryGetMonitorPosition.restype = c_bool
forge.TryTeleportMonitor.restype = c_bool

def inverseDict(dict):# TODO: two way dictionary class
    invDict = {}
    for key, val in dict.items(): invDict[val] = key
    return invDict

flagToPhysEnum = {
    ForgeObjectFlags.PhysicsNormal: 'NORMAL',
    ForgeObjectFlags.PhysicsFixed: 'FIXED',
    ForgeObjectFlags.PhysicsPhased: 'PHASED',
}
flagToSymmetry = {
    ForgeObjectFlags.Symmetric: 'SYMMETRIC',
    ForgeObjectFlags.Asymmetric: 'ASYMMETRIC',
    ForgeObjectFlags.Symmetric + ForgeObjectFlags.Asymmetric: 'BOTH',
}
colorNumberToEnum = { 0:'RED', 1:'BLUE', 2:'GREEN', 3:'ORANGE', 4:'PURPLE', 5:'YELLOW', 6:'BROWN', 7:'PINK', 8:'NEUTRAL', 255:'TEAM_COLOR' }
shapeNumberToEnum = { 0:'NONE', 1:'NONE', 2:'CYLINDER', 3:'BOX' }
physEnumToFlag = inverseDict(flagToPhysEnum)
symmetryToFlag = inverseDict(flagToSymmetry)
colorEnumToNumber = inverseDict(colorNumberToEnum)
shapeEnumToNumber = inverseDict(shapeNumberToEnum)

#teleporterTypes = ("Receiver Node","Sender Node","Two-Way Node")
teleporterTypes = (12,13,14)

def lockObject(obj, scale=True, loc=False, rot=False):
    for i_ax in range(0,3):
        obj.lock_scale[i_ax] = scale
        obj.lock_location[i_ax] = loc
        obj.lock_rotation[i_ax] = rot

def createForgeObject(context, itemName, i=None, data=None):
    blobj = bpy.data.objects.new(itemName if i is None else "%d - %s"%(i,itemName), data)
    context.scene.collection.objects.link(blobj)
    blobj.show_instancer_for_viewport = blobj.show_instancer_for_render = False
    blobj['isForgeObject'] = True
    lockObject(blobj)
    
    if (itemName in bpy.data.collections):
        blobj.instance_collection = bpy.data.collections[itemName]
        blobj.instance_type = 'COLLECTION'
    else:
        blobj.empty_display_type = 'ARROWS'
        blobj.empty_display_size = 0.5
        #blobj.show_name = True
    return blobj

def importForgeObjects(context, self=None, createCollections=False):
    if not forge.TrySetConnect(True): return False

    t0 = time.time()

    forge.ReadMemory()
    c = forge.GetObjectCount()
    print("Map: %s, %d Objects"%(forge.GetMapName(), c))

    for i in range(0,maxObjectCount):
        fobj = forge.GetObjectPtr(i).contents
        if fobj.show == 0: continue
        itemName = forge.ForgeObject_GetItemName(i)

        if createCollections and itemName not in bpy.data.collections:
            scene = bpy.data.scenes["Props"]
            collection = bpy.data.collections.new(itemName)
            scene.collection.children.link(collection)

            blobj = bpy.data.objects.new(itemName, None)
            collection.objects.link(blobj)
            blobj.empty_display_type = 'ARROWS'
            blobj.empty_display_size = 0.5
        
        blobj = createForgeObject(context, itemName, i)
        blobj.forge.FromForgeObject(fobj, blobj)

        if itemName == "Initial Loadout Camera":
            cam = bpy.data.objects.new("Initial Loadout Camera", bpy.data.cameras.new("Initial Loadout Camera"))
            context.scene.collection.objects.link(cam)
            cam.parent = blobj
            cam.rotation_euler = Euler((radians(90),0,0))
            lockObject(cam, True, True, True)
    
    msg = "Imported %d objects in %.3fs"%(c,time.time() - t0)
    if self is None: print(msg)
    else: self.report({'INFO'}, msg)
    bpy.ops.ed.undo_push()
    return True
def exportForgeObjects(context, self=None):
    if not forge.TrySetConnect(True): return False

    t0 = time.time()
    forge.ClearObjectList()
    i = 0
    for blobj in context.scene.objects:
        if not blobj.get('isForgeObject', False): continue
        fobj = forge.GetObjectPtr(i).contents
        blobj.forge.ToForgeObject(fobj, blobj)
        forge.AddObject(i)
        i += 1
    forge.WriteMemory()
    msg = "Exported %d objects in %.3fs"%(i,time.time() - t0)
    if self is None: print(msg)
    else: self.report({'INFO'}, msg)
    return True

class ImportForgeObjects(bpy.types.Operator):
    """Attempt to connect to MCC and import current forge objects"""
    bl_idname = 'forge.import'
    bl_label = "Import Forge Objects From MCC"

    def execute(self, context):
        if importForgeObjects(context, self): return {'FINISHED'}
        else:
            self.report({'ERROR'}, forge.GetLastError())
            return {'CANCELLED'}
class ExportForgeObjects(bpy.types.Operator):
    """Export current forge objects into MCC's forge"""
    bl_idname = 'forge.export'
    bl_label = "Export Forge Objects To MCC"

    confirm: BoolProperty(name="I understand, I saved a backup", default=False)
    neverAsk: BoolProperty(name="Don't ask again", default=False, description="Don't talk to me or my son ever again")

    def invoke(self, context, event):
        '''ret = context.window_manager.invoke_props_dialog(self)
        print(ret)
        return ret'''
        if persist_vars.get('forge_show_warning',True): return context.window_manager.invoke_props_dialog(self)
        else:
            self.confirm=True
            self.neverAsk=True
            return self.execute(context)
    def draw(self, context):
        textTowrap = "This modifies MCC's RAM, changes take effect after round restart. This can accidentally cause crashes or corrupt the map. Before saving ingame, restart the round and ensure objects haven't unexpectedly changed."
        wList = textwrap.TextWrapper(width=60).wrap(text=textTowrap)

        layout = self.layout.column(align=True)
        layout.scale_y = 0.7
        for text in wList: layout.label(text=text)

        col = self.layout.column(align=True)
        col.prop(self, "confirm")
        row = col.row(align=True)
        row.enabled = self.confirm
        row.prop(self, "neverAsk")
    def execute(self, context):
        if not self.confirm:
            self.report({'ERROR'}, "Cancelled Export")
            return {'CANCELLED'}
        persist_vars['forge_show_warning'] = not self.neverAsk
        if exportForgeObjects(context, self): return {'FINISHED'}
        else:
            self.report({'ERROR'}, forge.GetLastError())
            return {'CANCELLED'}

def importForgeMenu(self, context): self.layout.operator(ImportForgeObjects.bl_idname, text="Forge Objects", icon='ANTIALIASED')
def exportForgeMenu(self, context): self.layout.operator(ExportForgeObjects.bl_idname, text="Forge Objects", icon='ANTIALIASED')

'''class TeleportPlayer(bpy.types.Operator):
    """Teleport player monitor to current camera position"""
    bl_idname = "forge.teleport"
    bl_label = "Teleport Monitor to Camera Position"

    def execute(self, context):
        forge.TrySetConnect(True)
        forge.ReadMemory()
        print(forge.GetMapName())
        
        pos = float3()
        res = forge.TryGetMonitorPosition(byref(pos))
        pos.z += 10
        if not forge.TryTeleportMonitor(pos):
            self.report({'ERROR'}, "Failed to teleport")
            return {'CANCELLED'}
        
        return {'FINISHED'}
class TeleportPlayerToCursor(bpy.types.Operator):
    """Teleport player monitor to cursor"""
    bl_idname = "forge.teleport_cursor"
    bl_label = "Teleport Monitor to Cursor"

    def execute(self, context):
        cursorPos = context.scene.cursor.location
        pos = float3(cursorPos.x,cursorPos.y,cursorPos.z+1)

        forge.TrySetConnect(True)
        forge.ReadMemory()
        
        if not forge.TryTeleportMonitor(pos):
            self.report({'ERROR'}, "Failed to teleport")
            return {'CANCELLED'}
        
        return {'FINISHED'}'''

class ForgeObjectProps(bpy.types.PropertyGroup):
    teamEnum = [
        ('RED', "Red", ""),
        ('BLUE', "Blue", ""),
        ('GREEN', "Green", ""),
        ('ORANGE', "Orange", ""),
        ('PURPLE', "Purple", ""),
        ('YELLOW', "Yellow", ""),
        ('BROWN', "Brown", ""),
        ('PINK', "Pink", ""),
        ('NEUTRAL', "Neutral", ""),
    ]
    colorEnum = teamEnum + [ ('TEAM_COLOR', "Team", "") ]

    object: StringProperty()
    shapeObject: StringProperty()
    
    objectType: IntProperty()
    cachedType: IntProperty()
    scriptLabelIndex: IntProperty(name="Game Type Label", description="Index number determining how the gametype interprets this object")
    otherInfoA: IntProperty()
    otherInfoB: IntProperty()

    physics: EnumProperty(name="Physics", description="Physics mode",
        items=[
            ('NORMAL', "Normal", "Affected by gravity and movable"),
            ('FIXED', "Fixed", "Unaffected by gravity"),
            ('PHASED', "Phased", "Unaffected by gravity and collisionless"),
        ]
    )
    team: EnumProperty(name="Team", description="Object team", items=teamEnum, default='NEUTRAL')
    color: EnumProperty(name="Color", description="Object color", items=colorEnum, default='TEAM_COLOR')
    spawnSequence: IntProperty(name="Spawn Sequence", description="Gamemode phase at which the object will spawn", min=-100, max=100)
    spawnTime: IntProperty(name="Spawn Time", description="Time in seconds before the object spawns or respawns", min=0, max=180)# 0 is never
    gameSpecific: BoolProperty(name="Game Specific", description="Should object exclusively spawn for current gamemode")
    placeAtStart: BoolProperty(name="Place At Start", description="Should object spawn at start", default=True)
    symmetry: EnumProperty(name="Symmetry", description="Gamemode symmetry",
        items=[
            ('BOTH', "Both", "Present in symmetric and asymmetric gamemodes (default)"),
            ('SYMMETRIC', "Symmetric", "Present only in symmetric gamemodes"),
            ('ASYMMETRIC', "Asymmetric", "Present only in asymmetric gamemodes"),
        ],
        default='BOTH'
    )
    
    def UpdateShape(self, context):
        shObj = bpy.data.objects.get(self.shapeObject, None)
        if self.shape == 'NONE':
            if shObj is not None: bpy.data.objects.remove(shObj, do_unlink=True)
            return
        
        if shObj is None:
            shObj = bpy.data.objects.new("Shape", None)
            context.scene.collection.objects.link(shObj)# TODO: limit to main scene?
            shObj.rotation_euler = Euler((0,0,radians(90)))
            shObj.show_instancer_for_viewport = shObj.show_instancer_for_render = False
            shObj.instance_type = 'COLLECTION'
            shObj.parent = bpy.data.objects[self.object]
            lockObject(shObj, True, True, True)
            
            self.shapeObject = shObj.name
        
        if self.shape == 'BOX':
            shObj.instance_collection = bpy.data.collections['Shape Box']
            shObj.scale = (self.width,self.length,self.top + self.bottom)
        elif self.shape == 'CYLINDER':
            shObj.instance_collection = bpy.data.collections['Shape Cylinder']
            d = self.width * 2
            shObj.scale = (d,d,self.top + self.bottom)
        
        shObj.location = (0,0,(self.top-self.bottom)/2)
    
    shape: EnumProperty(name="Shape", description="Area shape", default='NONE', update=UpdateShape,
        items=[('NONE', "None", ""), ('CYLINDER', "Cylinder", ""), ('BOX', "Box", "")]
    )
    width: FloatProperty(name="Width", description="", unit='LENGTH', min=0,max=100.9, update=UpdateShape)
    length: FloatProperty(name="Length", description="", unit='LENGTH', min=0,max=100.9, update=UpdateShape)
    top: FloatProperty(name="Top", description="Distance to top from center", unit='LENGTH', min=0,max=100.9, update=UpdateShape)
    bottom: FloatProperty(name="Bottom", description="Distance to bottom from center", unit='LENGTH', min=0,max=100.9, update=UpdateShape)

    def FromForgeObject(self, fobj, blobj):
        flags = ForgeObjectFlags(fobj.flags)

        self.object = blobj.name
        
        self.physics = flagToPhysEnum[flags & ForgeObjectFlags.PhysicsMask]
        self.team = colorNumberToEnum[fobj.team]
        self.color = colorNumberToEnum[fobj.color]
        self.symmetry = flagToSymmetry[flags & ForgeObjectFlags.SymmetryMask]
        self.spawnSequence = fobj.spawnSequence
        self.spawnTime = fobj.spawnTime
        self.placeAtStart = not ForgeObjectFlags.HideAtStart in flags
        self.gameSpecific = ForgeObjectFlags.GameSpecific in flags
        self.shape = shapeNumberToEnum[fobj.shape]
        self.width = fobj.width
        self.length = fobj.length
        self.top = fobj.top
        self.bottom = fobj.bottom

        self.objectType = fobj.itemCategory << 8 | fobj.itemVariant
        self.cachedType = fobj.cachedType
        self.scriptLabelIndex = fobj.scriptLabelIndex
        self.otherInfoA = fobj.otherInfoA
        self.otherInfoB = fobj.otherInfoB

        fwd = fobj.forward
        up = fobj.up
        right = fwd.cross(up)
        pos = fobj.position
        blobj.matrix_world = Matrix(((right.x,fwd.x,up.x,pos.x),(right.y,fwd.y,up.y,pos.y),(right.z,fwd.z,up.z,pos.z),(0,0,0,1)))

        #self.UpdateShape(bpy.context)
    def ToForgeObject(self, fobj, blobj):
        m = blobj.matrix_world
        fobj.forward = float3.fromVector(m.col[1])
        fobj.up = float3.fromVector(m.col[2])
        fobj.position = float3.fromVector(m.col[3])

        coll = blobj.instance_collection
        ty = forge.ItemNameToType(coll.name_full) if coll is not None else self.objectType
        fobj.itemCategory = ty >> 8
        fobj.itemVariant = ty & 0x00FF
        #fobj.idExt = 0xFFFFFFFF

        fobj.cachedType = self.cachedType
        fobj.scriptLabelIndex = self.scriptLabelIndex
        fobj.otherInfoA = self.otherInfoA
        fobj.otherInfoB = self.otherInfoB

        fobj.show = 1
        fobj.team = colorEnumToNumber[self.team]
        fobj.color = colorEnumToNumber[self.color]
        fobj.flags = ForgeObjectFlags.HideAtStart*(not self.placeAtStart) + ForgeObjectFlags.GameSpecific*self.gameSpecific + physEnumToFlag[self.physics] + symmetryToFlag[self.symmetry]
        fobj.spawnSequence = self.spawnSequence
        fobj.spawnTime = self.spawnTime
        fobj.shape = shapeEnumToNumber[self.shape]
        fobj.width = self.width
        fobj.length = self.length
        fobj.top = self.top
        fobj.bottom = self.bottom

def getWidth(targetRegion):
  for region in bpy.context.area.regions:
    if region.type == targetRegion:
      return region.width
  return 500
def drawForgeObjectProperties(self, context, region):
    layout = self.layout
    width = getWidth(region)
    wide = width > 200
    layout.use_property_split = wide
    obj = context.object
    fprops = obj.forge
    
    locRotLay = layout if wide else layout.column(align=True)
    locRotLay.prop(obj, 'location')
    locRotLay.prop(obj, 'rotation_euler', text="Rotation")
    
    if wide:
        layout.prop(obj, 'instance_collection', text="Object Type")
    else:
        col = layout.column(align=True)
        col.label(text="Object Type:")
        col.prop(obj, 'instance_collection', text="")
    
    col = layout.column(align=True)
    col.use_property_split = width > 160
    col.prop(fprops, 'physics')
    col.prop(fprops, 'team')
    col.prop(fprops, 'color')
    col.prop(fprops, 'symmetry')

    if fprops.cachedType == 1:
        col = layout.column(align=True)
        col.label(text="Weapon")
        col.prop(fprops, 'otherInfoA', text="Spare Clips")
    elif fprops.cachedType in teleporterTypes:
        col = layout.column(align=True)
        col.label(text="Teleporter")
        col.prop(fprops, 'otherInfoA', text="Channel")
        col.prop(fprops, 'otherInfoB', text="Flags")
    
    col = layout.column(align=True)
    if wide:
        col.prop(fprops, 'spawnSequence')
        col.prop(fprops, 'spawnTime')
    else:
        col.label(text="Spawn")
        col.prop(fprops, 'spawnSequence', text="Sequence")
        col.prop(fprops, 'spawnTime', text="Time")
    
    col = layout.column(align=True)
    col.prop(fprops, 'placeAtStart')
    col.prop(fprops, 'gameSpecific')
    
    shape = fprops.shape
    col = layout.column(align=True)
    col.prop(fprops, 'shape')
    col2 = col.column(align=True)
    col2.enabled = shape != 'NONE'
    col2.prop(fprops, 'width', text=('Width' if shape == 'BOX' else 'Radius'))
    if shape == 'BOX': col2.prop(fprops, 'length')
    '''lenRow = col2.row(align=True)
    lenRow.enabled = shape == 'BOX'
    lenRow.prop(fprops, 'length')'''
    col2.prop(fprops, 'top')
    col2.prop(fprops, 'bottom')

    col = layout.column(align=True)
    col.prop(fprops, 'scriptLabelIndex')
    
    #layout.operator('render.render')
def pollForgePanel(self, context):
    if context.object is None: return False
    return context.object.get('isForgeObject', False)
    #return len(context.selected_objects) > 0
class ForgeObjectPanel(bpy.types.Panel):
    bl_label = "Forge Properties"
    bl_idname = 'SCENE_PT_forge_object'
    bl_space_type = 'PROPERTIES'
    bl_region_type = 'WINDOW'
    bl_context = 'object'
    
    @classmethod
    def poll(self, context): return pollForgePanel(self, context)
    
    def draw(self, context): drawForgeObjectProperties(self, context, 'WINDOW')
class ForgeObjectPanel_Sidebar(bpy.types.Panel):
    bl_label = "Forge Properties"
    bl_idname = 'SCENE_PT_forge_object_sidebar'
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = 'Forge'
    bl_context = 'objectmode'
    
    @classmethod
    def poll(self, context): return pollForgePanel(self, context)
    
    def draw(self, context): drawForgeObjectProperties(self, context, 'UI')

class PasteOverload(bpy.types.Operator):
    """Duplicates selected objects (for forge compatibility)"""
    bl_idname = "view3d.pastebuffer"
    bl_label = "Paste Forge Objects"
    
    autoselect: BoolProperty(default=True)
    active: BoolProperty(default=True)

    @classmethod
    def poll(cls, context): return context.active_object is not None and context.active_object.get('isForgeObject',False)
    
    def execute(self, context):
        bpy.ops.object.duplicate_move('INVOKE_DEFAULT')
        return {'FINISHED'}

class ConvertForge(bpy.types.Operator):
    """Set if object is exported to MCC"""
    bl_idname = 'object.convert_forge'
    bl_label = "Convert Forge Object"

    isForgeObject: BoolProperty()

    def execute(self, context):
        for obj in context.selected_objects: obj['isForgeObject'] = self.isForgeObject
        return {'FINISHED'}
def convertForgeMenuItem(self, context):
    isForge = context.active_object.get('isForgeObject', False)
    if isForge: op = self.layout.operator(ConvertForge.bl_idname, icon='REMOVE', text="Convert To Blender Object")
    else: op = self.layout.operator(ConvertForge.bl_idname, icon='ADD', text="Convert To Forge Object")
    op.isForgeObject = not isForge

def draw_forgeObjectOverlay():
    for area in bpy.context.screen.areas:
        if area.type != 'VIEW_3D': continue
        for space in area.spaces:
            if space.type == 'VIEW_3D':
                if not space.overlay.show_overlays: return
                break
    
    objCount = 0
    for obj in bpy.context.scene.objects:
        if obj.get('isForgeObject',False): objCount += 1
    
    font_id = 0
    blf.position(font_id, 15, 15, 0)
    blf.size(font_id, 11, 72)
    if objCount > maxObjectCount: blf.color(font_id, 1,0,0,1)
    elif objCount > 500: blf.color(font_id, 1,0.5,0,1)
    else: blf.color(font_id, 1,1,1,1)
    blf.draw(font_id, "%d / %d" % (objCount,maxObjectCount))

def exploreChildren(collection, list):
    for coll in collection.children:
        if (len(coll.objects) > 0): list.append((coll.name, coll.name, ""))
        else: exploreChildren(coll, list)
    return list
def genObjectTypesEnum(self, context): return exploreChildren(bpy.data.scenes['Props'].collection, [])
class AddForgeObject(bpy.types.Operator):
    """Add forge object"""
    bl_idname = 'forge.add_object'
    bl_label = "Forge Object..."
    bl_property = 'objectType'

    objectType: EnumProperty(name="Object Type", items=genObjectTypesEnum)
    
    def execute(self, context):
        blobj = createForgeObject(context, self.objectType)
        bpy.ops.ed.undo_push()
        
        blobj.location = context.scene.cursor.location
        for obj in bpy.context.selected_objects: obj.select_set(False)
        blobj.select_set(True)
        context.view_layer.objects.active = blobj
        
        return {'FINISHED'}
    
    def invoke(self, context, event):
        context.window_manager.invoke_search_popup(self)
        return {'FINISHED'}
def addForgeObjectMenuItem(self, context):
    layout = self.layout
    layout.operator_context = 'INVOKE_DEFAULT'
    layout.operator(AddForgeObject.bl_idname, icon='ADD')
    
    layout.context_pointer_set('forgeColl',bpy.data.scenes['Props'].collection)
    layout.menu(AddForgeObjectMenu.bl_idname)

class ForgeCollectionProps(bpy.types.PropertyGroup):
    def iconsEnum(self, context):
        icons = bpy.types.UILayout.bl_rna.functions['prop'].parameters['icon'].enum_items.keys()
        icoEnum = []
        for i in range(0,len(icons)):
            ico = icons[i]
            icoEnum.append((ico,"",ico,ico,i))
            #if (ico == 'MESH_CUBE'): print(i)
        return icoEnum
    icon: EnumProperty(name="Icon",description="Icon used in menus", items=iconsEnum, default=265)
class ForgeCollectionPanel(bpy.types.Panel):
    bl_label = "Forge Collection"
    bl_idname = 'SCENE_PT_forge_collection'
    bl_space_type = 'PROPERTIES'
    bl_region_type = 'WINDOW'
    bl_context = 'collection'
    
    def draw(self, context):
        layout = self.layout
        layout.use_property_split = True
        collProps = context.collection.forge
        layout.prop(collProps, 'icon')

class AddForgeObjectMenu(bpy.types.Menu):
    bl_label = "Forge Object"
    bl_idname = 'VIEW3D_MT_add_forge_object'

    def draw(self, context):
        try: children = context.forgeColl.children
        except: return

        layout = self.layout
        for coll in children:
            name = coll.name
            if (len(coll.objects) > 0):
                layout.operator_context = 'EXEC_DEFAULT'
                op = layout.operator(AddForgeObject.bl_idname, text=name, icon=coll.forge.icon)
                op.objectType = name
            elif (len(coll.children) > 0):
                self.layout.context_pointer_set('forgeColl',coll)
                layout.menu(self.__class__.bl_idname, text=name, icon=coll.forge.icon)

reg_classes = [
    ForgeObjectProps, ForgeCollectionProps, 
    ImportForgeObjects, ExportForgeObjects, AddForgeObject, PasteOverload, ConvertForge,
    ForgeObjectPanel, ForgeObjectPanel_Sidebar, ForgeCollectionPanel, AddForgeObjectMenu
]
# TeleportPlayer, TeleportPlayerToCursor
reg_objMenus = [convertForgeMenuItem]
reg_addMenus = [addForgeObjectMenuItem]
persist_vars = bpy.app.driver_namespace

def registerDrawEvent(event, item):
    id = event.bl_rna.name
    handles = persist_vars.get(id, [])
    event.append(item)
    handles.append(item)
    persist_vars[id] = handles
def removeDrawEvents(event):
    for item in persist_vars.get(event.bl_rna.name,[]):
        try: event.remove(item)
        except: pass

def register():
    for cls in reg_classes: bpy.utils.register_class(cls)
    for item in reg_objMenus: registerDrawEvent(bpy.types.VIEW3D_MT_object_context_menu, item)
    for item in reg_addMenus: registerDrawEvent(bpy.types.VIEW3D_MT_add, item)
    registerDrawEvent(bpy.types.TOPBAR_MT_file_import, importForgeMenu)
    registerDrawEvent(bpy.types.TOPBAR_MT_file_export, exportForgeMenu)

    bpy.types.Object.forge = bpy.props.PointerProperty(type=ForgeObjectProps)
    bpy.types.Collection.forge = bpy.props.PointerProperty(type=ForgeCollectionProps)

    persist_vars['forgeObjectsOverlay_handle'] = bpy.types.SpaceView3D.draw_handler_add(draw_forgeObjectOverlay, (), 'WINDOW', 'POST_PIXEL')
def unregister():
    forge.TrySetConnect(False)
    for cls in reg_classes:
        try: bpy.utils.unregister_class(cls)
        except: pass
    removeDrawEvents(bpy.types.VIEW3D_MT_object_context_menu)
    removeDrawEvents(bpy.types.VIEW3D_MT_add)
    removeDrawEvents(bpy.types.TOPBAR_MT_file_import)
    removeDrawEvents(bpy.types.TOPBAR_MT_file_export)
    bpy.types.SpaceView3D.draw_handler_remove(persist_vars['forgeObjectsOverlay_handle'], 'WINDOW')
if __name__ == "__main__":
    try: unregister()
    except: pass
    register()