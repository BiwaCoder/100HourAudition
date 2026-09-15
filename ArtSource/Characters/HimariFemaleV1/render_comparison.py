import bpy
from pathlib import Path
from mathutils import Vector
P=Path(__file__).resolve().parent
bpy.ops.wm.open_mainfile(filepath=str(P/'Himari-Wire.blend'))
bpy.data.objects['Himari_Rig'].location.x=-1.25
for name,x in [('Himari-StreetCel',0),('Himari-Anime',1.25)]:
 with bpy.data.libraries.load(str(P/(name+'.blend')),link=False) as (source,target):target.collections=['CHARACTER - export this collection']
 col=target.collections[0];bpy.context.scene.collection.children.link(col)
 for o in col.objects:
  if o.type=='ARMATURE':o.location.x=x
scene=bpy.context.scene;cam=scene.camera;cam.location=(0,-8,2.60);cam.rotation_euler=(Vector((0,0,.96))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=4.45
scene.render.resolution_x=1800;scene.render.resolution_y=1050;scene.cycles.samples=32;scene.frame_set(1)
scene.render.filepath=str(P/'Himari-ThreeStyles.png');bpy.ops.render.render(write_still=True)
