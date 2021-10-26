import bpy, enum, time, blf, textwrap, io, zlib
from bpy.types import Operator
from bpy.props import *
from ctypes import *
from os.path import exists
from math import *
from mathutils import *
from struct import *
from bpy_extras.io_utils import ImportHelper

print("Mjolnir v0.9.8")

maxObjectCount = 650
propSceneName = 'Props'
shapesCollName = 'Shapes'
mapPalette = 'Forge World Palette'
dllPath = bpy.path.abspath("//") + "ForgeBridge.dll"
persist_vars = bpy.app.driver_namespace
defaultGtLabel = ('NO_LABEL', "No Label", "Default")

class float3(Structure):
    _fields_ = [ ('x', c_float), ('y', c_float), ('z', c_float) ]
    def cross(self, other): return float3(self.y * other.z - self.z * other.y, self.z * other.x - self.x * other.z, self.x * other.y - self.y * other.x)
    def dot(self, other): return self.x*other.x + self.y*other.y + self.z*other.z
    def sqrMag(self): return self.x*self.x + self.y*self.y + self.z*self.z
    def normalized(self):
        sqrMag = self.sqrMag()
        if sqrMag == 0: return float3()
        invMag = 1 / sqrt(sqrMag)
        return float3(self.x*invMag, self.y*invMag, self.z*invMag)
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
        ('gtLabelIndex', c_ushort),
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

def inverseDict(dict):# TODO: two way dictionary class
    invDict = {}
    for key, val in dict.items():
        if val not in invDict: invDict[val] = key
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
objectNameToShapeColor = { 'Kill Boundary':1, 'Soft Kill Boundary':1, 'Safe Boundary':2, 'Soft Safe Boundary':2 }
objectTypeToShapeColor = { 0x3D00:1, 0x3D01:1, 0x3C00:2, 0x3C01:2 }
physEnumToFlag = inverseDict(flagToPhysEnum)
symmetryToFlag = inverseDict(flagToSymmetry)
colorEnumToNumber = inverseDict(colorNumberToEnum)
shapeEnumToNumber = inverseDict(shapeNumberToEnum)
gtIndexToLabel = {}
gtLabelToIndex = {}

#teleporterTypes = ("Receiver Node","Sender Node","Two-Way Node")
teleporterTypes = (12,13,14)

def initGtLabels():
    global gtIndexToLabel
    gtIndexToLabel = {65535: 'NO_LABEL'}
    persist_vars['gtIndexToLabel'] = gtIndexToLabel
def initInvGtLabels():
    global gtLabelToIndex
    gtLabelToIndex = inverseDict(gtIndexToLabel)

def wrapText(text):
    strList = []
    for line in text.split('\n'):
        strList += textwrap.TextWrapper(width=60).wrap(text=line)
    return strList
def wrapTextPanel(self, text, scale=0.75):
    layout = self.layout.column(align=True)
    layout.scale_y = scale
    for wrapped in wrapText(text): layout.label(text=wrapped)

def lockObject(obj, scale=True, loc=False, rot=False):
    for i_ax in range(0,3):
        obj.lock_scale[i_ax] = scale
        obj.lock_location[i_ax] = loc
        obj.lock_rotation[i_ax] = rot

def createForgeObject(context, itemName, i=None, data=None):
    blobj = bpy.data.objects.new(itemName if i is None else "%d - %s"%(i,itemName), data)
    context.collection.objects.link(blobj)
    #blobj.show_instancer_for_viewport = blobj.show_instancer_for_render = False
    blobj['isForgeObject'] = True
    blobj.forge.object = blobj.name
    lockObject(blobj)
    
    if itemName in bpy.data.collections:
        blobj.instance_collection = bpy.data.collections[itemName]
        blobj.instance_type = 'COLLECTION'
        blobj.empty_display_size = 0.0001
    else:
        blobj.empty_display_type = 'ARROWS'
        blobj.empty_display_size = 0.5
        #blobj.show_name = True
    return blobj

def importForgeObjects(context, self=None, createCollections=False):
    if context.scene.name == propSceneName: return False
    if not forge.TrySetConnect(True): return False

    t0 = time.time()

    forge.ReadMemory()
    if forge.GetMapName() == "None":
        print(forge.GetLastError())
    c = forge.GetObjectCount()
    gt_c = forge.GetGtLabelCount()
    print("Map: %s, %d Objects, %d Gametype Labels"%(forge.GetMapName(), c, gt_c))

    initGtLabels()
    for i in range(0,gt_c):
        label = forge.GetGtLabel(i).upper()
        gtIndexToLabel[i] = label
    initInvGtLabels()
    persist_vars['gtIndexToLabel'] = gtIndexToLabel

    for i in range(0,maxObjectCount):
        fobj = forge.GetObjectPtr(i).contents
        if fobj.show == 0: continue
        itemName = forge.ForgeObject_GetItemName(i)

        if createCollections and itemName not in bpy.data.collections:
            collection = bpy.data.collections.new(itemName)
            bpy.data.scenes[propSceneName].collection.children.link(collection)

            blobj = bpy.data.objects.new(itemName, None)
            collection.objects.link(blobj)
            blobj.empty_display_type = 'ARROWS'
            blobj.empty_display_size = 0.5
        
        blobj = createForgeObject(context, itemName, i)
        blobj.forge.FromForgeObject(fobj, blobj)

        if itemName == "Initial Loadout Camera":# TODO: put in collection instead
            cam = bpy.data.objects.new("Initial Loadout Camera", bpy.data.cameras.new("Initial Loadout Camera"))
            context.collection.objects.link(cam)
            cam.parent = blobj
            cam.rotation_euler = Euler((radians(90),0,0))
            lockObject(cam, True, True, True)
    
    msg = "Imported %d objects in %.3fs"%(c,time.time() - t0)
    if self is None: print(msg)
    else: self.report({'INFO'}, msg)
    return True
def exportForgeObjects(context, self=None):
    if not forge.TrySetConnect(True): return False

    hitLimit = False
    t0 = time.time()
    forge.CacheCurrentMap()
    forge.ClearObjectList()
    i = 0
    for inst in bpy.context.evaluated_depsgraph_get().object_instances:
        blobj = tryGetForgeObjectFromInstance(inst)
        if blobj != None:
            if i >= maxObjectCount: 
                hitLimit = True
                continue
            fobj = forge.GetObjectPtr(i).contents
            blobj.forge.ToForgeObject(fobj, blobj, inst)
            forge.AddObject(i)
            i += 1
    
    forge.WriteMemory()
    msg = "Exported %d objects in %.3fs"%(i,time.time() - t0)
    if self is None: print(msg)
    else: self.report({'WARNING' if hitLimit else 'INFO'}, msg)
    return True

def executeAndReport(self, context, method):
    error = False
    try:
        if method(context, self): return {'FINISHED'}
        else: error = True
    except:
        error = True
    finally:
        if error:
            msg = forge.GetLastError()
            print(msg)
            self.report({'ERROR'}, msg)
            return {'CANCELLED'}

class ImportForgeObjects(Operator):
    """Attempt to connect to MCC and import current forge objects"""
    bl_idname = 'forge.import'
    bl_label = "Import Forge Objects"
    bl_options = {'REGISTER', 'UNDO'}

    additive: BoolProperty(name="Additive", description="Add the forge objects to the existing scene loaded in Blender", default=True)

    def execute(self, context):
        if not self.additive:
            for o in context.scene.objects:
                if o.get('isForgeObject', False): bpy.data.objects.remove(o, do_unlink=True)
        
        return executeAndReport(self, context, importForgeObjects)
class ExportForgeObjects(Operator):
    """Export current forge objects into MCC's forge"""
    bl_idname = 'forge.export'
    bl_label = "Export Forge Objects"

    confirm: BoolProperty(name="I understand, I saved a backup", default=False)
    neverAsk: BoolProperty(name="Don't ask again", default=False, description="Don't talk to me or my son ever again")

    def invoke(self, context, event):
        if persist_vars.get('forge_show_warning',True): return context.window_manager.invoke_props_dialog(self)
        else:
            self.confirm=True
            self.neverAsk=True
            return self.execute(context)
    def draw(self, context):
        wrapTextPanel(self, "This modifies MCC's RAM, changes take effect after round restart. This can accidentally cause crashes or corrupt the map. Before saving ingame, restart the round and ensure objects haven't unexpectedly changed.")

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

        return executeAndReport(self, context, exportForgeObjects)

def importForgeMenu(self, context): self.layout.operator(ImportForgeObjects.bl_idname, text="Forge Objects", icon='ANTIALIASED')
def exportForgeMenu(self, context): self.layout.operator(ExportForgeObjects.bl_idname, text="Forge Objects", icon='ANTIALIASED')

'''class TeleportPlayer(Operator):
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
class TeleportPlayerToCursor(Operator):
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
    def GetLabelEnum(self, context):
        labelEnum = []
        for i, label in persist_vars['gtIndexToLabel'].items():
            if i == 65535: labelEnum.append(('NO_LABEL',"No Label","Default"))
            else: labelEnum.append((label,label,""))
        return labelEnum

    def UpdateColor(self, context):
        col = self.color
        if col == 'TEAM_COLOR': col = self.team
        self.colorIndex = colorEnumToNumber[col]

    def UpdateShape(self, context):
        shObj = bpy.data.objects.get(self.shapeObject, None)
        if self.shape == 'NONE':
            if shObj != None: bpy.data.objects.remove(shObj, do_unlink=True)
            return
        
        if shObj is None:
            shObj = bpy.data.objects.new("%s Shape" % self.object, None)
            self.shapeObject = shObj.name
            '''collChildren = context.scene.collection.children
            shapesCollection = collChildren.get(shapesCollName, None)
            if shapesCollection is None:
                shapesCollection = bpy.data.collections.new(shapesCollName)
                collChildren.link(shapesCollection)
            coll = shapesCollection if shapesCollection != None else context.collection'''
            coll = context.collection
            coll.objects.link(shObj)
            shObj.rotation_euler = Euler((0,0,radians(90)))
            shObj.show_instancer_for_viewport = shObj.show_instancer_for_render = False
            shObj.instance_type = 'COLLECTION'
            blObj = bpy.data.objects[self.object]
            shObj.parent = blObj
            lockObject(shObj, True, True, True)
            #shObj['colorIndex'] = objectTypeToShapeColor.get(blObj.forge.objectType, 0)
        
        coll = None
        if self.shape == 'BOX':
            coll = bpy.data.collections['Shape Box']
            shObj.scale = (self.width,self.length,self.top + self.bottom)
        elif self.shape == 'CYLINDER':
            coll = bpy.data.collections['Shape Cylinder']
            d = self.width * 2
            shObj.scale = (d,d,self.top + self.bottom)
        if shObj.instance_collection != coll: shObj.instance_collection = coll
        
        shObj.location = (0,0,(self.top-self.bottom)/2)

    teamEnum = [
        ('RED', "Red", ""),
        ('BLUE', "Blue", ""),
        ('GREEN', "Green", ""),
        ('ORANGE', "Orange", ""),
        ('PURPLE', "Purple", ""),
        ('YELLOW', "Yellow", ""),
        ('BROWN', "Brown", ""),
        ('PINK', "Pink", ""),
        ('NEUTRAL', "Neutral", "")
    ]
    colorEnum = teamEnum + [ ('TEAM_COLOR', "Team", "") ]

    object: StringProperty()
    shapeObject: StringProperty()
    
    objectType: IntProperty()
    cachedType: IntProperty()
    gtLabel: EnumProperty(name="Game Type Label", description="How the gametype interprets this object", items=GetLabelEnum, default=0)
    otherInfoA: IntProperty()
    otherInfoB: IntProperty()

    
    physics: EnumProperty(name="Physics", description="Physics mode", default='PHASED',
        items=[
            ('NORMAL', "Normal", "Affected by gravity and movable"),
            ('FIXED', "Fixed", "Unaffected by gravity"),
            ('PHASED', "Phased", "Unaffected by gravity and collisionless"),
        ]
    )
    team: EnumProperty(name="Team", description="Object team", items=teamEnum, default='NEUTRAL', update=UpdateColor)
    color: EnumProperty(name="Color", description="Object color", items=colorEnum, default='TEAM_COLOR', update=UpdateColor)
    colorIndex: IntProperty(default=8)
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
    
    shape: EnumProperty(name="Shape", description="Area shape", default='NONE', update=UpdateShape,
        items=[('NONE', "None", ""), ('CYLINDER', "Cylinder", ""), ('BOX', "Box", "")]
    )
    width: FloatProperty(name="Width", description="", unit='LENGTH', min=0,max=100.9, update=UpdateShape)
    length: FloatProperty(name="Length", description="", unit='LENGTH', min=0,max=100.9, update=UpdateShape)
    top: FloatProperty(name="Top", description="Distance to top from center", unit='LENGTH', min=0,max=100.9, update=UpdateShape)
    bottom: FloatProperty(name="Bottom", description="Distance to bottom from center", unit='LENGTH', min=0,max=100.9, update=UpdateShape)

    def FromForgeObject(self, fobj, blobj):
        flags = ForgeObjectFlags(fobj.flags)
        
        self.objectType = fobj.itemCategory << 8 | fobj.itemVariant
        self.cachedType = fobj.cachedType
        self.gtLabel = gtIndexToLabel.get(fobj.gtLabelIndex, defaultGtLabel[0])
        self.otherInfoA = fobj.otherInfoA
        self.otherInfoB = fobj.otherInfoB

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

        fwd = fobj.forward
        up = fobj.up
        right = fwd.cross(up)

        print('%s - %s %s %s' % (blobj.name, fwd,up,right))

        pos = fobj.position
        blobj.matrix_world = Matrix(((right.x,fwd.x,up.x,pos.x),(right.y,fwd.y,up.y,pos.y),(right.z,fwd.z,up.z,pos.z),(0,0,0,1)))
    
    def ToForgeObject(self, fobj, blobj, inst=None):
        m = blobj.matrix_world if inst is None else inst.matrix_world
        fobj.forward = float3.fromVector(m.col[1])
        fobj.up = float3.fromVector(m.col[2])
        fobj.position = float3.fromVector(m.col[3])

        coll = blobj.instance_collection
        ty = forge.ItemNameToType(coll.name_full) if coll != None else self.objectType
        fobj.itemCategory = ty >> 8
        fobj.itemVariant = ty & 0x00FF

        fobj.cachedType = self.cachedType
        fobj.gtLabelIndex = gtLabelToIndex[self.gtLabel]
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
    col.prop(fprops, 'gtLabel')
    
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

class PasteOverload(Operator):
    """Duplicates selected objects (for forge compatibility)"""
    bl_idname = 'view3d.pastebuffer'
    bl_label = "Paste Forge Objects"
    
    autoselect: BoolProperty(default=True)
    active: BoolProperty(default=True)

    @classmethod
    def poll(cls, context): return context.active_object != None and context.active_object.get('isForgeObject',False)
    def execute(self, context): return bpy.ops.object.duplicate_move('INVOKE_DEFAULT')

class ConvertForge(Operator):
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

def arrayModifier(context, type, count=3, offset=(1,0,0), rotation=(0,0,0), curveLen=5):
    srcObj = context.active_object
    loc = srcObj.location
    size = 1

    coll = srcObj.instance_collection
    if coll != None:
        name = coll.name

        for inst_o in coll.objects:
            s = inst_o.dimensions.y
            if s > size: size = s
    else: name = srcObj.name

    bpy.ops.mesh.primitive_plane_add(enter_editmode=False, align='WORLD', location=loc, size=1)
    arrObj = masterParent = context.object
    arrObj.name = "%s Array" % name
    arrObj.instance_type = 'FACES'
    arrObj.show_instancer_for_viewport = arrObj.show_instancer_for_render = False
    arrObj.select_set(True)
    
    bpy.ops.object.modifier_add(type='ARRAY')
    mod = arrObj.modifiers["Array"]
    mod.show_on_cage = True
    mod.use_relative_offset = False

    if type != 'CONST':
        parentObj = masterParent = bpy.data.objects.new("%s Array Holder" % name, None)
        context.collection.objects.link(parentObj)
        parentObj.location = loc
        parentObj.empty_display_size = 2
        arrObj.parent = parentObj
        arrObj.location = (0,0,0)
    if type == 'OBJECT':
        offsetObj = bpy.data.objects.new("%s Array Offset" % name, None)
        context.collection.objects.link(offsetObj)
        #offsetObj.empty_display_type = 'CUBE'
        offsetObj.empty_display_size = size# 0.5 * size + 0.05
        offsetObj.location = offset
        offsetObj.rotation_euler = rotation
        lockObject(offsetObj)
        offsetObj.parent = parentObj
        offsetObj.select_set(True)

        mod.use_constant_offset = False
        mod.use_object_offset = True
        mod.offset_object = offsetObj
    else:
        mod.use_constant_offset = True
        mod.constant_offset_displace = offset
    if type == 'CURVE':
        bpy.ops.object.modifier_add(type='CURVE')
        cMod = arrObj.modifiers["Curve"]
        cMod.show_on_cage = cMod.show_in_editmode = True
        
        bpy.ops.curve.primitive_bezier_curve_add(enter_editmode=False, align='WORLD', location=(0,0,0), radius=curveLen)
        curve = bpy.context.object
        curve.name = "%s Curve" % name
        curve.parent = parentObj
        curve.select_set(True)
        lockObject(curve)
        curveData = curve.data
        curveData.twist_mode = 'Z_UP'
        curveData.use_deform_bounds = curve.show_in_front = True

        mod.fit_type = 'FIT_CURVE'
        mod.curve = curve
        cMod.object = curve

        arrObj.select_set(True)
    else:
        mod.count = count

    masterParent.rotation_euler = srcObj.rotation_euler

    srcObj.parent = arrObj
    srcObj.location = (0,0,0)
    srcObj.rotation_euler = (0,0,0)
    srcObj.display_type = 'BOUNDS'
    srcObj.select_set(True)
    if type != 'CONST': parentObj.select_set(True)

class SetupArray(Operator):
    """Setup array (repeated object duplication) for forge object"""
    bl_idname = 'object.setup_array'
    bl_label = "Setup Array"
    bl_options = {'REGISTER', 'UNDO'}

    type: EnumProperty(name="Type", description="Array modifier mode", default='CONST',
        items=[
            ('CONST', "Linear", "Simple XYZ offset from the modifier panel", 'MOD_ARRAY', 0),
            ('OBJECT', "Object", "Advanced XYZ offset and rotation from an object", 'MOD_TINT', 1),
            ('CURVE', "Curve", "Place objects along curve", 'MOD_CURVE', 2),
        ]
    )
    count: IntProperty(name="Count", description="Number of duplicated objects", default=3, min=1, max=650)
    offset: FloatVectorProperty(name="Offset", default=(1,0,0))
    rotation: FloatVectorProperty(name="Rotation", default=(0,0,0))
    size: FloatProperty(name="Size", description="Size of curve", default=5, min=0)

    @classmethod
    def poll(cls, context): 
        o = context.active_object
        return o != None# and o.instance_type == 'COLLECTION' and o.parent is None
    def execute(self, context):
        rot = self.rotation
        arrayModifier(context, self.type, self.count, self.offset, (radians(rot[0]),radians(rot[1]),radians(rot[2])), self.size)
        return {'FINISHED'}
    def draw(self, context):
        layout = self.layout
        layout.use_property_split = True
        layout.prop(self, 'type')
        if self.type != 'CURVE': layout.prop(self, 'count')
        layout.prop(self, 'offset')
        if self.type == 'OBJECT': layout.prop(self, 'rotation')
        if self.type == 'CURVE': layout.prop(self, 'size')
def setupArrayMenuItem(self, context): self.layout.operator(SetupArray.bl_idname, icon='MOD_ARRAY')

def tryGetForgeObjectFromInstance(inst):
    o = inst.object
    if inst.is_instance:
        if o.is_instancer: return o
    elif o.get('isForgeObject',False):
        p = o.parent
        if p is None or p.instance_type == 'NONE' or p.instance_type == 'COLLECTION': return o
    else: return None

def draw_forgeObjectOverlay():
    for area in bpy.context.screen.areas:
        if area.type != 'VIEW_3D': continue
        for space in area.spaces:
            if space.type == 'VIEW_3D':
                if not space.overlay.show_overlays: return
                break
    
    objCount = 0
    for inst in bpy.context.evaluated_depsgraph_get().object_instances:
        if tryGetForgeObjectFromInstance(inst) != None: objCount += 1
    
    font_id = 0
    blf.position(font_id, 15, 15, 0)
    blf.size(font_id, 11, 72)
    if objCount > maxObjectCount: blf.color(font_id, 1,0,0,1)
    elif objCount > 500: blf.color(font_id, 1,0.5,0,1)
    else: blf.color(font_id, 1,1,1,1)
    blf.draw(font_id, "%d / %d" % (objCount,maxObjectCount))

iconDict = {}
def fillIconDict(collection):
    global iconDict
    for coll in collection.children:
        if len(coll.objects) > 0:
            if iconDict.get(coll, None) is None: iconDict[coll] = coll.forge.icon
        else: fillIconDict(coll)

def getCollectionEnums(collection, list):
    global iconDict
    for coll in collection.children:
        if len(coll.objects) > 0:
            list.append((coll.name, coll.name, "", iconDict.get(coll, 'NONE'), len(list)))
        else: getCollectionEnums(coll, list)
    return list
def genObjectTypesEnum(self, context): return getCollectionEnums(bpy.data.collections[mapPalette], [])
class AddForgeObject(Operator):
    """Add forge object"""
    bl_idname = 'forge.add_object'
    bl_label = "Forge Object..."
    bl_property = 'objectType'

    objectType: EnumProperty(name="Object Type", items=genObjectTypesEnum)
    
    def invoke(self, context, event):
        fillIconDict(bpy.data.collections[mapPalette])
        context.window_manager.invoke_search_popup(self)
        return {'FINISHED'}
    def execute(self, context):
        blobj = createForgeObject(context, self.objectType)
        blobj.location = context.scene.cursor.location
        for obj in bpy.context.selected_objects: obj.select_set(False)
        blobj.select_set(True)
        context.view_layer.objects.active = blobj
        
        bpy.ops.ed.undo_push()
        return {'FINISHED'}
def addForgeObjectMenuItem(self, context):
    layout = self.layout
    layout.operator_context = 'INVOKE_DEFAULT'
    layout.operator(AddForgeObject.bl_idname, icon='ADD')
    
    layout.context_pointer_set('forgeColl', bpy.data.collections[mapPalette])
    layout.menu(AddForgeObjectMenu.bl_idname)

class ForgeCollectionProps(bpy.types.PropertyGroup):
    def iconsEnum(self, context):
        icons = bpy.types.UILayout.bl_rna.functions['prop'].parameters['icon'].enum_items.keys()
        icoEnum = []
        for i in range(0,len(icons)):
            ico = icons[i]
            icoEnum.append((ico,"",ico,ico,i))
            #if ico == 'NONE': print(i)
        return icoEnum
    icon: EnumProperty(name="Icon",description="Icon used in menus", items=iconsEnum, default=0)
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
            if len(coll.objects) > 0:
                layout.operator_context = 'EXEC_DEFAULT'
                op = layout.operator(AddForgeObject.bl_idname, text=name, icon=coll.forge.icon)
                op.objectType = name
            elif len(coll.children) > 0:
                self.layout.context_pointer_set('forgeColl',coll)
                layout.menu(self.__class__.bl_idname, text=name, icon=coll.forge.icon)

class ErrorMessage(Operator):
    """Error message"""
    bl_idname = 'forge.error'
    bl_label = "Error"

    message: bpy.props.StringProperty()
    url: bpy.props.StringProperty()

    def invoke(self, context, event):
        print("Error: " + self.message + "\n")
        return context.window_manager.invoke_props_dialog(self)
    def draw(self, context):
        wrapTextPanel(self, self.message)

        if self.url != "":
            layout = self.layout.column(align=True)
            layout.operator_context = 'INVOKE_DEFAULT'
            op = layout.operator('wm.url_open', text="Open Webpage")
            op.url = self.url
    def execute(self, context):
        return {'CANCELLED'}


class ImportMVAR(Operator, ImportHelper):
    """Import Halo Reach Map Variant"""
    bl_idname = 'forge.import_mvar'
    bl_label = "Import MVAR"
    filename_ext = ".mvar"
    filter_glob: StringProperty( default="*.mvar", options={'HIDDEN'}, maxlen=255 )

    def execute(self, context):
        if TryReadMvarFile(self.filepath): return {'FINISHED'}
        else: return{'CANCELLED'}

def importMvarMenu(self, context): self.layout.operator(ImportMVAR.bl_idname, text="Map Variant (.mvar)", icon='ANTIALIASED')

forgeItemNames = {
	#Universal
	#Weapons Human
	0x0000:"Assault Rifle", 0x0100:"DMR", 0x0200:"Grenade Launcher", 0x0300:"Magnum", 0x0400:"Rocket Launcher", 0x0500:"Shotgun", 0x0600:"Sniper Rifle", 0x0700:"Spartan Laser", 0x0800:"Frag Grenade", 0x0900:"Mounted Machinegun",
	#Weapons Covenant
	0x0A00:"Concussion Rifle", 0x0B00:"Energy Sword", 0x0C00:"Fuel Rod Gun", 0x0D00:"Gravity Hammer", 0x0E00:"Focus Rifle", 0x0F00:"Needle Rifle", 0x1000:"Needler", 0x1100:"Plasma Launcher", 0x1200:"Plasma Pistol", 0x1300:"Plasma Repeater", 0x1400:"Plasma Rifle", 0x1500:"Spiker", 0x1600:"Plasma Grenade", 0x1700:"Plasma Turret",
	#Armor Abilities
	0x1800:"Active Camouflage", 0x1900:"Armor Lock", 0x1A00:"Drop Shield", 0x1B00:"Evade", 0x1C00:"Hologram", 0x1D00:"Jet Pack", 0x1E00:"Sprint",

	#Forge World
	#Vehicles
	0x1F00:"Banshee", 0x2000:"Falcon", 0x2100:"Ghost", 0x2200:"Mongoose", 0x2300:"Revenant", 0x2400:"Scorpion", 0x2500:"Shade Turret", 0x2600:"Warthog, Default", 0x2601:"Warthog, Gauss", 0x2602:"Warthog, Rocket", 0x2700:"Wraith",
	#Gadgets
	0x2800:"Fusion Coil", 0x2801:"Landmine", 0x2802:"Plasma Battery", 0x2803:"Propane Tank", 0x2900:"Health Station", 0x2A00:"Camo Powerup", 0x2A01:"Overshield", 0x2A02:"Custom Powerup", 0x2B00:"Cannon, Man", 0x2B01:"Cannon, Man, Heavy", 0x2B02:"Cannon, Man, Light", 0x2B03:"Cannon, Vehicle", 0x2B04:"Gravity Lift", 0x2C00:"One Way Shield 2", 0x2D00:"One Way Shield 3", 0x2E00:"One Way Shield 4", 0x2F00:"FX:Colorblind", 0x2F01:"FX:Next Gen", 0x2F02:"FX:Juicy", 0x2F03:"FX:Nova", 0x2F04:"FX:Olde Timey", 0x2F05:"FX:Pen And Ink", 0x2F06:"FX:Purple", 0x2F07:"FX:Green", 0x2F08:"FX:Orange", 0x3000:"Shield Door, Small", 0x3100:"Shield Door, Medium", 0x3200:"Shield Door, Large", 0x3300:"Receiver Node", 0x3301:"Sender Node", 0x3302:"Two-Way Node", 0x3400:"Die", 0x3401:"Golf Ball", 0x3402:"Golf Club", 0x3403:"Kill Ball", 0x3404:"Soccer Ball", 0x3405:"Tin Cup", 0x3500:"Light, Red", 0x3501:"Light, Blue", 0x3502:"Light, Green", 0x3503:"Light, Orange", 0x3504:"Light, Purple", 0x3505:"Light, Yellow", 0x3506:"Light, White", 0x3507:"Light, Red, Flashing", 0x3508:"Light, Yellow, Flashing",
	#Spawning
	0x3600:"Initial Spawn", 0x3700:"Respawn Point", 0x3800:"Initial Loadout Camera", 0x3900:"Respawn Zone", 0x3A00:"Respawn Zone, Weak", 0x3B00:"Respawn Zone, Anti", 0x3C00:"Safe Boundary", 0x3C01:"Soft Safe Boundary", 0x3D00:"Kill Boundary", 0x3D01:"Soft Kill Boundary",
	#Objectives
	0x3E00:"Flag Stand", 0x3F00:"Capture Plate", 0x4000:"Hill Marker",
	#Scenery
	0x4100:"Barricade, Small", 0x4101:"Barricade, Large", 0x4102:"Covenant Barrier", 0x4103:"Portable Shield", 0x4200:"Camping Stool", 0x4300:"Crate, Heavy Duty", 0x4301:"Crate, Heavy, Small", 0x4302:"Covenant Crate", 0x4303:"Crate, Half Open", 0x4400:"Sandbag Wall", 0x4401:"Sandbag, Turret Wall", 0x4402:"Sandbag Corner, 45", 0x4403:"Sandbag Corner, 90", 0x4404:"Sandbag Endcap", 0x4500:"Street Cone",
	#Structure
	#Building Blocks
	0x4600:"Block, 1x1", 0x4601:"Block, 1x1, Flat", 0x4602:"Block, 1x1, Short", 0x4603:"Block, 1x1, Tall", 0x4604:"Block, 1x1, Tall And Thin", 0x4605:"Block, 1x2", 0x4606:"Block, 1x4", 0x4607:"Block, 2x1, Flat", 0x4608:"Block, 2x2", 0x4609:"Block, 2x2, Flat", 0x460A:"Block, 2x2, Short", 0x460B:"Block, 2x2, Tall", 0x460C:"Block, 2x3", 0x460D:"Block, 2x4", 0x460E:"Block, 3x1, Flat", 0x460F:"Block, 3x3", 0x4610:"Block, 3x3, Flat", 0x4611:"Block, 3x3, Short", 0x4612:"Block, 3x3, Tall", 0x4613:"Block, 3x4", 0x4614:"Block, 4x4", 0x4615:"Block, 4x4, Flat", 0x4616:"Block, 4x4, Short", 0x4617:"Block, 4x4, Tall", 0x4618:"Block, 5x1, Short", 0x4619:"Block, 5x5, Flat",
	#Bridges And Platforms
	0x4700:"Bridge, Small", 0x4701:"Bridge, Medium", 0x4702:"Bridge, Large", 0x4703:"Bridge, XLarge", 0x4704:"Bridge, Diagonal", 0x4705:"Bridge, Diag, Small", 0x4706:"Dish", 0x4707:"Dish, Open", 0x4708:"Corner, 45 Degrees", 0x4709:"Corner, 2x2", 0x470A:"Corner, 4x4", 0x470B:"Landing Pad", 0x470C:"Platform, Ramped", 0x470D:"Platform, Large", 0x470E:"Platform, XL", 0x470F:"Platform, XXL", 0x4710:"Platform, Y", 0x4711:"Platform, Y, Large", 0x4712:"Sniper Nest", 0x4713:"Staircase",
	#Buildings
	0x4714:"Walkway, Large", 0x4800:"Bunker, Small", 0x4801:"Bunker, Small, Covered", 0x4802:"Bunker, Box", 0x4803:"Bunker, Round", 0x4804:"Bunker, Ramp", 0x4805:"Pyramid", 0x4806:"Tower, 2 Story", 0x4807:"Tower, 3 Story", 0x4808:"Tower, Tall", 0x4809:"Room, Double", 0x480A:"Room, Triple",
	#Decorative
	0x4900:"Antenna, Small", 0x4901:"Antenna, Satellite", 0x4902:"Brace", 0x4903:"Brace, Large", 0x4904:"Brace, Tunnel", 0x4905:"Column", 0x4906:"Cover", 0x4907:"Cover, Crenellation", 0x4908:"Cover, Glass", 0x4909:"Glass Sail", 0x490A:"Railing, Small", 0x490B:"Railing, Medium", 0x490C:"Railing, Long", 0x490D:"Teleporter Frame", 0x490E:"Strut", 0x490F:"Large Walkway Cover",
	#Doors, Windows, And Walls
	0x4A00:"Door", 0x4A01:"Door, Double", 0x4A02:"Window", 0x4A03:"Window, Double", 0x4A04:"Wall", 0x4A05:"Wall, Double", 0x4A06:"Wall, Corner", 0x4A07:"Wall, Curved", 0x4A08:"Wall, Coliseum", 0x4A09:"Window, Colesium", 0x4A0A:"Tunnel, Short", 0x4A0B:"Tunnel, Long",
	#Inclines
	0x4B00:"Bank, 1x1", 0x4B01:"Bank, 1x2", 0x4B02:"Bank, 2x1", 0x4B03:"Bank, 2x2", 0x4B04:"Ramp, 1x2", 0x4B05:"Ramp, 1x2, Shallow", 0x4B06:"Ramp, 2x2", 0x4B07:"Ramp, 2x2, Steep", 0x4B08:"Ramp, Circular, Small", 0x4B09:"Ramp, Circular, Large", 0x4B0A:"Ramp, Bridge, Small", 0x4B0B:"Ramp, Bridge, Medium", 0x4B0C:"Ramp, Bridge, Large", 0x4B0D:"Ramp, XL", 0x4B0E:"Ramp, Stunt",
	#Natural
	0x4C00:"Rock, Small", 0x4C01:"Rock, Flat", 0x4C02:"Rock, Medium 1", 0x4C03:"Rock, Medium 2", 0x4C04:"Rock, Spire 1", 0x4C05:"Rock, Spire 2", 0x4C06:"Rock, Seastack", 0x4C07:"Rock, Arch",
	0x4D00:"Grid",
	#Vehicles (MCC)
	0x5900:"Falcon, Nose Gun", 0x5901:"Falcon, Grenadier", 0x5902:"Falcon, Transport", 0x5A00:"Warthog, Transport", 0x5B00:"Sabre", 0x5C00:"Seraph", 0x5D00:"Cart, Electric", 0x5E00:"Forklift", 0x5F00:"Pickup", 0x6000:"Truck Cab", 0x6100:"Van, Oni", 0x6200:"Shade, Fuel Rod",
	#Gadgets (MCC)
	0x6300:"Cannon, Man, Forerunner", 0x6301:"Cannon, Man, Heavy, Forerunner", 0x6302:"Cannon, Man, Light, Forerunner", 0x6303:"Gravity Lift, Forerunner", 0x6304:"Gravity Lift, Tall, Forerunner", 0x6305:"Cannon, Man, Human", 0x6400:"One Way Shield 1", 0x6500:"One Way Shield 5", 0x6600:"Shield Wall, Small", 0x6700:"Shield Wall, Medium", 0x6800:"Shield Wall, Large", 0x6900:"Shield Wall, X-Large", 0x6A00:"One Way Shield 2", 0x6B00:"One Way Shield 3", 0x6C00:"One Way Shield 4", 0x6D00:"Shield Door, Small", 0x6E00:"Shield Door, Small 1", 0x6F00:"Shield Door, Large", 0x7000:"Shield Door, Large 1", 0x7100:"Ammo Cabinet", 0x7200:"Spnkr Ammo", 0x7300:"Sniper Ammo",
	#Scenery (MCC)
	0x7400:"Jersey Barrier", 0x7401:"Jersey Barrier, Short", 0x7402:"Heavy Barrier", 0x7500:"Small, Closed", 0x7501:"Crate, Metal, Multi", 0x7502:"Crate, Metal, Single", 0x7503:"Crate, Fully Open", 0x7504:"Crate, Forerunner, Small", 0x7505:"Crate, Forerunner, Large", 0x7600:"Pallet", 0x7601:"Pallet, Large", 0x7602:"Pallet, Metal", 0x7700:"Driftwood 1", 0x7701:"Driftwood 2", 0x7702:"Driftwood 3", 0x7800:"Phantom", 0x7801:"Spirit", 0x7802:"Pelican", 0x7803:"Drop Pod, Elite", 0x7804:"Anti Air Gun", 0x7900:"Cargo Truck, Destroyed", 0x7901:"Falcon, Destroyed", 0x7902:"Warthog, Destroyed", 0x7A00:"Folding Chair", 0x7B00:"Dumpster", 0x7C00:"Dumpster, Tall", 0x7D00:"Equipment Case", 0x7E00:"Monitor", 0x7F00:"Plasma Storage", 0x8000:"Camping Stool, Covenant", 0x8100:"Covenant Antenna", 0x8200:"Fuel Storage", 0x8300:"Engine Cart", 0x8400:"Missile Cart",
	#Structure (MCC)
	0x8500:"Bridge", 0x8501:"Platform, Covenant", 0x8502:"Catwalk, Straight", 0x8503:"Catwalk, Short", 0x8504:"Catwalk, Bend, Left", 0x8505:"Catwalk, Bend, Right", 0x8506:"Catwalk, Angled", 0x8507:"Catwalk, Large", 0x8600:"Bunker, Overlook", 0x8601:"Gunners Nest", 0x8700:"Cover, Small", 0x8701:"Block, Large", 0x8702:"Blocker, Hallway", 0x8703:"Column, Stone", 0x8704:"Tombstone", 0x8705:"Cover, Large, Stone", 0x8706:"Cover, Large", 0x8707:"Walkway Cover", 0x8708:"Walkway Cover, Short", 0x8709:"Cover, Large, Human", 0x870A:"I-Beam", 0x8800:"Wall (MCC)", 0x8801:"Door (MCC)", 0x8802:"Door, Human", 0x8803:"Door A, Forerunner", 0x8804:"Door B, Forerunner", 0x8805:"Door C, Forerunner", 0x8806:"Door D, Forerunner", 0x8807:"Door E, Forerunner", 0x8808:"Door F, Forerunner", 0x8809:"Door G, Forerunner", 0x880A:"Door H, Forerunner", 0x880B:"Wall, Small, Forerunner", 0x880C:"Wall, Large, Forerunner", 0x8900:"Rock, Spire 3", 0x8901:"Tree, Dead",
	0x8D00:"Target Designator",#Other (MCC)
	#Hidden Structure Blocks
	0x4E00:"Block, 2x2, Invisible", 0x4F00:"Block, 1x1, Invisible", 0x5000:"Block, 2x2x2, Invisible", 0x5100:"Block, 4x4x2, Invisible", 0x5200:"Block, 4x4x4, Invisible", 0x5300:"Block, 2x1, Flat, Invisible", 0x5400:"Block, 1x1, Flat, Invisible", 0x5500:"Block, 1x1, Small, Invisible", 0x5600:"Block, 2x2, Flat, Invisible", 0x5700:"Block, 4x2, Flat, Invisible", 0x5800:"Block, 4x4, Flat, Invisible",
	#Hidden Misc
	0x8A00:"Generator", 0x8B00:"Vending Machine"
}

class BitStream:
    def __init__(self, byteStream):
        self.byteStream = byteStream
        self.bitPos = 0
    
    def ReadBytes(self, count): return self.ReadBits(8*count)
    def ReadBits(self, count):
        finalPos = self.bitPos + count
        lShift = finalPos % 8
        rShift = -finalPos % 8
        skip = self.bitPos % 8 + rShift >= 8
        carryMask = 2**rShift - 1# redundant because of shift?
        fullBitMask = 2**count - 1 << rShift

        byteCount = ceil(finalPos/8) - floor(self.bitPos/8)
        bytes = self.byteStream.read(byteCount)
        self.SeekBits(count)

        outBytes = bytearray()
        carry = 0
        for i in range(byteCount):
            by = bytes[i] & (fullBitMask >> 8*(byteCount-1 - i) & 0xFF)
            carryNext = (by & carryMask) << lShift# TODO: overflow?!
            if not skip: outBytes.append((by >> rShift) | carry)
            else: skip = False
            carry = carryNext
        return outBytes
        
    def ReadString(self, count, stopOnNull=False): return self.ReadBytes(count).decode('utf-8',errors='ignore').rstrip('\0')
    def ReadString16(self, count, stopOnNull=False):
        if stopOnNull:
            s = ""
            for i in range(2*count):
                char = self.ReadBytes(2)
                if char == b'\x00\x00': break
                s += char.decode('utf-16-be',errors='ignore')
            return s
        else: return self.ReadBytes(2*count).decode('utf-16-be',errors='ignore').rstrip('\0')
    def ReadUInt32(self): return unpack('>I',self.ReadBytes(4))[0]
    def ReadUInt16(self): return unpack('>H',self.ReadBytes(2))[0]
    def ReadFloat(self): return unpack('>f',self.ReadBytes(4))[0]
    def ReadBoolBit(self): return unpack('?',self.ReadBits(1))[0]
    def ReadUInt8Bits(self, count): return unpack('B',self.ReadBits(count))[0]
    def ReadUInt16Bits(self, count): return unpack('>H',self.ReadBits(count))[0]
    def ReadUInt32Bits(self, count): return unpack('>I',self.ReadBits(count))[0]
    def ReadUInt64Bits(self, count): return unpack('>L',self.ReadBits(count))[0]
    def ReadUInt128Bits(self, count): return unpack('>Q',self.ReadBits(count))[0]
    def ReadUIntBits(self, count): return int.from_bytes(self.ReadBits(count),'big')
    def ReadStruct(self, format, count): return unpack('>'+format,self.ReadBytes(count))
    def ReadStructBits(self, format, count): return unpack('>'+format,self.ReadBits(count))
    def Seek(self, offset, mode=io.SEEK_CUR): self.SeekBits(8*offset,mode)
    def SeekBits(self, offset, mode=io.SEEK_CUR):
        if mode == io.SEEK_SET: self.bitPos = offset
        elif mode == io.SEEK_CUR: self.bitPos += offset
        else: raise
        self.byteStream.seek(floor(self.bitPos/8))
    def PosToString(self): return '%d bytes, %d bits' % (floor(self.bitPos/8), self.bitPos%8)

def ReadShapeDimension(stream):
    value = stream.ReadUInt16Bits(11)
    if not value: return 0
    elif value == 0x7FF: return 200
    else: return (value - 1) * 0.0977517142892 + 0.0488758571446
def ReadUpAxis(stream):
    lookup_table = [
        [0x0000A, 0x002], [0x00015, 0x003],
        [0x0002A, 0x005], [0x00055, 0x008],
        [0x000AA, 0x00C], [0x00155, 0x011],
        [0x002AA, 0x019], [0x00555, 0x023],
        [0x00AAA, 0x033], [0x01555, 0x048],
        [0x02AAA, 0x067], [0x05555, 0x092],
        [0x0AAAA, 0x0D0], [0x15555, 0x126],
        [0x2AAAA, 0x1A1], [0x55555, 0x24E],
        [0xAAAAA, 0x343], [0x155555, 0x9D04],
        [0xAAAAAA, 0x687], [0x555555, 0x93B],
        [0x2AAAAAA, 0x1A1F], [0x5555555, 0x24F4],
        [0xAAAAAAA, 0x3440]
    ]
    bitcount = 20
    bits = stream.ReadUIntBits(bitcount)# TODO
    r9   = bitcount - 6
    r11  = floor(bits / lookup_table[r9][0])
    r8   = lookup_table[r9][0] * r11
    ebx  = bits - r8
    
    eax = floor(ebx / lookup_table[r9][1])
    ecx = lookup_table[r9][1] * eax
    xmm2 = eax
    eax *= 2
    ebx -= ecx
    ecx = lookup_table[r9][1] - 1
    xmm0 = ecx
    ecx -= 1
    
    xmm1 = 2 / xmm0
    xmm2 = xmm2 * xmm1 - 1
    xmm1 *= 0.5
    xmm2 += xmm1
    xmm0 = xmm0
    xmm1 = xmm1
    xmm2 = xmm2
    if eax == ecx: xmm2 = 0
    
    ecx  = lookup_table[r9][1] - 1
    xmm1 = ebx
    xmm0 = ecx
    eax = ebx * 2
    ecx -= 1
    xmm3 = 2 / xmm0
    xmm1 *= xmm3
    xmm3 *= 0.5
    xmm1 -= 1
    xmm1 += xmm3
    xmm0 = xmm0
    xmm1 = xmm1
    xmm2 = xmm2
    xmm3 = xmm3
        
    if (eax == ecx): xmm1 = 0
    
    if r11 <= 5:
        if r11 == 0: upAxis = [1, xmm2, xmm1]
        elif r11 == 3: upAxis = [-1, xmm2, xmm1]
        elif r11 == 1: upAxis = [xmm2, 1, xmm1]
        elif r11 == 4: upAxis = [xmm2, -1, xmm1]
        elif r11 == 2: upAxis = [xmm2, xmm1, 1]
        elif r11 == 5: upAxis = [xmm2, xmm1, -1]
        else: upAxis = [0, 0, 1]
    else:
        print("need default up vector!")
        raise
    return float3(upAxis[0],upAxis[1],upAxis[2]).normalized()

def fnc1(a, b, c):
    xmm4 = a.z # correct, notwithstanding precision differences
    xmm7 = a.z * 0 # correct
    xmm6 = a.x * 0 # correct
    xmm0 = a.x + a.y + xmm7 # correct, notwithstanding precision differences
    xmm1 = xmm6 + a.y + xmm7 # correct
    if (abs(xmm1) > abs(xmm0)):
        xmm0 = xmm6 - a.y
        b.y = a.z - xmm6
        b.x = a.y - xmm7
    else:
        xmm6 -= xmm7 # correct
        xmm0 = a.y - a.x # slightly wrong, possibly explainable by precision differences
        b.y = xmm6
        b.x = a.z - (a.y * 0) # correct
    b.z = xmm0
    b = b.normalized() # correct, notwithstanding precision differences
    xmm5 = a.x
    xmm1 = b.x
    xmm3 = a.z
    xmm0 = b.x * a.y # correct
    xmm8 = a.x * b.y - (b.x * a.y) # correct
    xmm3 *= b.y # correct
    xmm2 = xmm3 * b.x # wrong, possibly explainable by precision differences
    xmm0 = b.z * a.x # correct, notwithstanding precision differences
    xmm4 = b.z * a.y - xmm3 # correct
    c.z = xmm8
    xmm2 -= xmm0 # correct, but this would be due to xmm2's incredibly small value initially
    c.x = xmm4
    c.y = xmm2
    c = c.normalized()# unused!
    return b
def fnc2(a, b, xmm2, xmm3):
    #a.enforce_single_precision()
    #b.enforce_single_precision()
    cx = xmm3
    cy = xmm2
    # dot(a,b)
    xmm3 = (a.x*b.x + a.y*b.y + a.z*b.z) * (1 - cx) # wrong digits and sign, right mag
    a.x = (xmm3*b.x + a.x*cx) - (a.y*b.z - a.z*b.y)*cy# wrong
    a.y = (xmm3*b.y + a.y*cx) - (a.z*b.x - a.x*b.z)*cy
    a.z = (xmm3*b.z + a.z*cx) - (a.x*b.y - a.y*b.x)*cy
    return a

def loadAxisAngleAngle(rawAng, upAxis):
    angRads  = rawAng * 0.0003834952 - pi + 0.0001917476
    '''xmm6 = angRads
    rsp20 = float3()
    rsp30 = float3()
    rsp20 = fnc1(upAxis, rsp20, rsp30)
    # good up to here?
    rotation = float3()
    rotation.x = rsp20.x
    rotation.z = rsp20.z'''

    '''result = (xmm6 == pi)
    if (isnan(xmm6) or isnan(pi)): # when would this ever be true?
        result = (xmm6 == -pi)
        if (isnan(xmm6) or isnan(-pi)): # when would this ever be true?
            result = False
            #
            # The comparisons use UCOMISS, and the weird-ass NaN checks are JP branches. 
            # UCOMISS should only set the parity flag (PF) if either or both operands 
            # are NaN, but the code here is written as if the *constant* is the one 
            # that might be NaN.
    if result:
        xmm7 = 0
        xmm0 = -1.0
    else:
        xmm7 = sin(xmm6)
        xmm0 = cos(xmm6)'''
    
    '''if (xmm6 == pi):
        xmm7 = 0
        xmm0 = -1.0
    else:
        xmm7 = sin(xmm6)
        xmm0 = cos(xmm6)
    
    rotation = fnc2(rotation, upAxis, xmm7, xmm0)
    return rotation.normalized()'''

    '''s = sin(angRads)
    c = cos(angRads)
    _c = 1 - c

    x = upAxis.x
    y = upAxis.y
    z = upAxis.z

    m = Matrix((
        (c + x*x*_c, x*y*_c - z*s, x*z*_c + y*s), 
        (y*x*_c + z*s, c + y*y*_c, y*z*_c - x*s), 
        (z*x*_c - y*s, z*y*_c + x*s, c + z*z*_c),
    ))'''

    #return float3.fromVector(m @ Vector((0,1,0))).normalized()
    #return float3.fromVector(m @ Vector((1,0,0))).normalized()
    #return float3.fromVector(m @ Vector((0,0,1))).normalized()
    #return float3.fromVector((m @ Matrix.Rotation(radians(-90), 3, 'Z')) @ Vector((0,1,0))).normalized()
    #return float3.fromVector((Matrix.Rotation(radians(-90), 3, 'Z') @ m) @ Vector((0,1,0))).normalized()
    frm = float3(0,0,1)
    right = frm.cross(upAxis)
    #return upAxis.cross(right).normalized()

    angRads2 = acos(frm.dot(upAxis))

    s = sin(angRads2)
    c = cos(angRads2)
    _c = 1 - c

    x = right.x
    y = right.y
    z = right.z

    m = Matrix((
        (c + x*x*_c, x*y*_c - z*s, x*z*_c + y*s), 
        (y*x*_c + z*s, c + y*y*_c, y*z*_c - x*s), 
        (z*x*_c - y*s, z*y*_c + x*s, c + z*z*_c),
    ))
    #return float3.fromVector((m @ Matrix.Rotation(angRads - (pi/2), 3, 'Z')) @ Vector((0,1,0))).normalized()
    return float3.fromVector((m @ Matrix.Rotation(angRads, 3, 'Z')) @ Vector((1,0,0))).normalized()
    #return float3.fromVector((Matrix.Rotation(angRads, 3, 'Z')) @ Vector((1,0,0))).normalized()

def ReadBlamEngineFileHeader(stream, size):
    stream.Seek(size - 8)
def ReadContentHeader(stream, size):
    startPos = stream.bitPos
    stream.Seek(135)
    title = stream.ReadString16(128)
    description = stream.ReadString16(128)
    print('%s - %s' % (title,description))
    stream.SeekBits(startPos + 8*(size-8), io.SEEK_SET)
def ReadMapVariant(stream, size):
    startPos = stream.bitPos
    stream.SeekBits(64*8 + 4)
    print(stream.PosToString())
    activity = stream.ReadUInt8Bits(3) - 1
    gameMode = stream.ReadUInt8Bits(3)
    engine = stream.ReadUInt8Bits(3)
    mapId = stream.ReadUInt32()
    stream.SeekBits(298)
    title = stream.ReadString16(128,True)
    description = stream.ReadString16(128,True)
    print('%s - %s' % (title,description))

    if (activity == 2): stream.SeekBytes(2)
    if (gameMode == 1): stream.SeekBits(52)
    elif (gameMode == 2): stream.SeekBits(34)

    stream.SeekBits(115)

    mapBounds = (stream.ReadStruct('ff',8), stream.ReadStruct('ff',8), stream.ReadStruct('ff',8))
    print('Bounds: %s' % str(mapBounds))

    rangesByAxis = [
        mapBounds[0][1] - mapBounds[0][0],
        mapBounds[1][1] - mapBounds[1][0],
        mapBounds[2][1] - mapBounds[2][0]
    ]

    if mapBounds:# Determines the proper bitcounts to use for object position coordinates, given the map bounds and the baseline bitcount specified.
        bitcount = 21
        MINIMUM_UNIT_16BIT = 0.00833333333 # hex 0x3C088889 == 0.00833333376795F
        if bitcount > 16: min_step = MINIMUM_UNIT_16BIT / (1 << (bitcount - 16))
        else: min_step = (1 << (16 - bitcount)) * MINIMUM_UNIT_16BIT
        
        axisBits = [0,0,0]
        if min_step >= 0.0001: # hex 0x38D1B717 == 9.99999974738e-05
            min_step *= 2
            for i in range(3):
                edx = min(0x800000, ceil(rangesByAxis[i] / min_step))
                ecx = (int(log(edx,2)) if edx >= 0 else 31) if edx else -1 # 23

                axisBits[i] = min(26, 
                    (ecx + (1 if ((edx & ((1 << ecx) - 1)) != 0) else 0)) if ecx != -1 
                    else 0
                )
        else: axisBits = [26,26,26]
    
    stream.Seek(8)
    strCount = stream.ReadUInt16Bits(9)
    print('%d Gametype Labels' % strCount)

    if strCount > 0:
        offsets = []
        for i in range(strCount):
            presence = stream.ReadBoolBit()
            if not presence: continue
            offset = stream.ReadUInt16Bits(12)
            offsets.append(offset)

        dataLength   = stream.ReadUInt16Bits(13)
        isCompressed = stream.ReadBoolBit()
        if isCompressed:
            compSize   = stream.ReadUInt16Bits(13)
            stream.Seek(4) # skip zlib header's uncompressed size
            decodedStrs = zlib.decompress(stream.ReadBytes(compSize-4))
            if len(decodedStrs) != dataLength:
                print('Expected ~%d bytes got %d (compressed into %d)' % (dataLength,len(decodedStrs),compSize))
            decodedStrs = decodedStrs.decode('utf-8').rstrip('\0')
        else:
            decodedStrs = stream.ReadString(dataLength)
        
        labelStrs = decodedStrs.upper().split('\0')
        print(labelStrs)
    
    st = time.time_ns()
    for i in range(651):
        presence = stream.ReadBoolBit()
        if not presence: break

        fobj = ForgeObject()
        fobj.show = 1
        unk00 = stream.ReadUInt8Bits(2)
        noSubcat = not stream.ReadBoolBit()
        if noSubcat: fobj.itemCategory = stream.ReadUInt8Bits(8)
        absence = stream.ReadBoolBit()
        if absence: fobj.itemVariant = 0xFF
        else: fobj.itemVariant = stream.ReadUInt8Bits(5)

        # Position
        a = stream.ReadBoolBit()
        if not mapBounds:
            print("POS TODO")
            if not a and not stream.ReadBoolBit() and stream.ReadUInt8Bits(2) != -1: print("POS TODO2")# != 3?!
            raise
            
        fobj.position = float3(
            (0.5 + unpack('>I',b'\x00' + stream.ReadBits(axisBits[0]))[0]) * (rangesByAxis[0] / (1 << axisBits[0])) + mapBounds[0][0],
            (0.5 + unpack('>I',b'\x00' + stream.ReadBits(axisBits[1]))[0]) * (rangesByAxis[1] / (1 << axisBits[1])) + mapBounds[1][0],
            (0.5 + unpack('>I',b'\x00' + stream.ReadBits(axisBits[2]))[0]) * (rangesByAxis[2] / (1 << axisBits[2])) + mapBounds[2][0]
        )

        vertical = stream.ReadBoolBit()
        if vertical: upAxis = float3(0,0,1)
        else: upAxis = ReadUpAxis(stream)
        
        rawAng = stream.ReadUInt16Bits(14)
        ang = (rawAng * 0.0003834952 - pi + 0.0001917476) * 180/pi

        if (i == 4):
            print("debug")
        fwd = loadAxisAngleAngle(rawAng, upAxis)

        #fobj.forward = float3(0,1,0)
        #fobj.forward = float3(0,1,-upAxis.y / upAxis.z).normalized()
        fobj.forward = fwd
        fobj.up = upAxis

        if i == 4:
            print(fwd)
            print(upAxis)

        fobj.spawnRelativeToMapIndex = stream.ReadUInt16Bits(10)

        # Load Object Data
        fobj.shape = stream.ReadUInt8Bits(2)
        if fobj.shape > 0 and fobj.shape < 4:
            fobj.width = ReadShapeDimension(stream)
            if fobj.shape == 3: fobj.length = ReadShapeDimension(stream)
            if fobj.shape != 1:
                fobj.top = ReadShapeDimension(stream)
                fobj.bottom = ReadShapeDimension(stream)
        
        eax = stream.ReadUInt8Bits(8)
        if eax & 0x80000000: eax |= 0xFFFFFF00 # test if signed
        fobj.spawnSequence = eax & 0xFF# TODO: make sure signed
        
        fobj.respawnTime = stream.ReadUInt8Bits(8)
        fobj.cachedType = stream.ReadUInt8Bits(5)
        fobj.gtLabelIndex = -1 if stream.ReadBoolBit() else stream.ReadUInt8Bits(8)
        fobj.flags = stream.ReadUInt8Bits(8)
        fobj.team = stream.ReadUInt8Bits(4) - 1
        fobj.color = -1 if stream.ReadBoolBit() else stream.ReadUInt8Bits(3)
        
        if fobj.cachedType == 1: fobj.otherInfoA = stream.ReadUInt8Bits(8)# weapon spare clips
        elif fobj.cachedType > 11 and fobj.cachedType <= 14:# teleporter
            fobj.otherInfoA = stream.ReadUInt8Bits(5)# channel
            fobj.otherInfoB = stream.ReadUInt8Bits(5)# passability
        elif fobj.cachedType == 19: fobj.otherInfoA = stream.ReadUInt8Bits(8) - 1# locationNameIndex

        blobj = createForgeObject(bpy.context, forgeItemNames.get(fobj.itemCategory << 8 | fobj.itemVariant, "Unknown"), i)
        blobj.forge.FromForgeObject(fobj, blobj)
        blobj['ang'] = ang
        blobj['up'] = Vector((upAxis.x, upAxis.y, upAxis.z))
        blobj['fwd'] = Vector((fwd.x, fwd.y, fwd.z))

        '''angRads  = rawAng * 0.0003834952 - pi + 0.0001917476
        s = sin(angRads)
        c = cos(angRads)
        _c = 1 - c

        x = upAxis.x
        y = upAxis.y
        z = upAxis.z
        m = Matrix((
            (c + x*x*_c, x*y*_c - z*s, x*z*_c + y*s, 0), 
            (y*x*_c + z*s, c + y*y*_c, y*z*_c - x*s, 0), 
            (z*x*_c - y*s, z*y*_c + x*s, c + z*z*_c, 0),
            (0,0,0,1)
        ))'''
        '''m = Matrix((
            (c + x*x*_c, x*z*_c - y*s, x*y*_c + z*s, 0), 
            (z*x*_c + y*s, c + z*z*_c, z*y*_c - x*s, 0), 
            (y*x*_c - z*s, y*z*_c + x*s, c + y*y*_c, 0),
            (0,0,0,1)
        ))'''
        '''m = Matrix((
            (c + x*x*_c, x*y*_c - z*s, x*z*_c + y*s, fobj.position.x), 
            (y*x*_c + z*s, c + y*y*_c, y*z*_c - x*s, fobj.position.y), 
            (z*x*_c - y*s, z*y*_c + x*s, c + z*z*_c, fobj.position.z),
            (0,0,0,1)
        ))'''

        #blobj.matrix_world = Matrix.Translation((fobj.position.x, fobj.position.y, fobj.position.z)) @ m #@ Matrix.Rotation(radians(180), 4, 'Z') @ Matrix.Rotation(radians(-90), 4, 'X')
        #blobj.matrix_world = Matrix(((right.x,fwd.x,up.x,pos.x),(right.y,fwd.y,up.y,pos.y),(right.z,fwd.z,up.z,pos.z),(0,0,0,1)))
        #blobj.matrix_world = 
    tEl = (time.time_ns() - st) * 1e-6
    print('Loaded %d Objects (%dms)' % (i,tEl))
def TryReadMvarFile(filename):
    f = open(filename, 'rb')
    stream = BitStream(f)

    #try:
    while True:
        signature = stream.ReadString(4)
        
        fnc = dictToFnc.get(signature, None)
        if fnc != None:
            size = stream.ReadUInt32()
            print('%s (%d)' % (signature,size))
            fnc(stream, size)
        else: break
    return True
    '''except Exception as ex:
        print("Error: %s" % ex)
        pass'''
    return False

dictToFnc = { '_blf':ReadBlamEngineFileHeader, 'chdr':ReadContentHeader, 'mvar':ReadMapVariant }



reg_classes = [
    ForgeObjectProps, ForgeCollectionProps, 
    ImportForgeObjects, ExportForgeObjects, AddForgeObject, PasteOverload, ConvertForge, SetupArray, ErrorMessage, ImportMVAR,
    ForgeObjectPanel, ForgeObjectPanel_Sidebar, ForgeCollectionPanel, AddForgeObjectMenu
]
# TeleportPlayer, TeleportPlayerToCursor
reg_objMenus = [convertForgeMenuItem, setupArrayMenuItem]
reg_addMenus = [addForgeObjectMenuItem]

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

    registerDrawEvent(bpy.types.TOPBAR_MT_file_import, importMvarMenu)
    #registerDrawEvent(bpy.types.TOPBAR_MT_file_export, exportForgeMenu)

    bpy.types.Object.forge = bpy.props.PointerProperty(type=ForgeObjectProps)
    bpy.types.Collection.forge = bpy.props.PointerProperty(type=ForgeCollectionProps)

    if not exists(dllPath):
        bpy.ops.forge.error('INVOKE_DEFAULT', 
            message="ForgeBridge.dll not found!\nPlease download the latest release from https://github.com/Waffle1434/Mjolnir-Forge-Editor/releases", 
            url="https://github.com/Waffle1434/Mjolnir-Forge-Editor/releases")
        return
    global forge
    forge = cdll.LoadLibrary(dllPath)
    try:
        print("ForgeBridge.dll Version %d" % forge.GetDllVersion())
        forge.TrySetConnect.restype = c_bool
        forge.GetLastError.restype = c_wchar_p
        forge.GetMapName.restype = c_wchar_p
        forge.GetGtLabel.restype = c_wchar_p
        forge.GetObjectPtr.restype = POINTER(ForgeObject)
        forge.ForgeObject_GetItemName.restype = c_wchar_p
        forge.ItemNameToType.argtypes = [c_wchar_p]
        forge.TryGetMonitorPosition.restype = c_bool
        forge.TryTeleportMonitor.restype = c_bool
    except Exception as ex:
        bpy.ops.forge.error('INVOKE_DEFAULT', 
            message="Outdated ForgeBridge.dll in use!\nPlease download the latest release from https://github.com/Waffle1434/Mjolnir-Forge-Editor/releases", 
            rl="https://github.com/Waffle1434/Mjolnir-Forge-Editor/releases")
        print(ex)

    persist_vars['forgeObjectsOverlay_handle'] = bpy.types.SpaceView3D.draw_handler_add(draw_forgeObjectOverlay, (), 'WINDOW', 'POST_PIXEL')
    if persist_vars.get('gtIndexToLabel', None) == None: initGtLabels()
    initInvGtLabels()

    #TryReadMvarFile("D:\Games\Steam\steamapps\common\Halo The Master Chief Collection\haloreach\map_variants\hr_forgeWorld_theCage.mvar")
    TryReadMvarFile("D:\Games\Steam\steamapps\common\Halo The Master Chief Collection\haloreach\map_variants\hr_forgeWorld_asylum.mvar")
    #TryReadMvarFile(r"C:\Users\super\AppData\LocalLow\MCC\LocalFiles\000901fe00493098\HaloReach\Map\rot_samples_1.mvar")
def unregister():
    try: forge.TrySetConnect(False)
    except: pass
    for cls in reg_classes:
        try: bpy.utils.unregister_class(cls)
        except: pass
    removeDrawEvents(bpy.types.VIEW3D_MT_object_context_menu)
    removeDrawEvents(bpy.types.VIEW3D_MT_add)
    removeDrawEvents(bpy.types.TOPBAR_MT_file_import)
    removeDrawEvents(bpy.types.TOPBAR_MT_file_export)
    bpy.types.SpaceView3D.draw_handler_remove(persist_vars['forgeObjectsOverlay_handle'], 'WINDOW')


try: unregister()
except: pass
register()