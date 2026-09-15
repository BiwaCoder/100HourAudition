import bpy, math
from pathlib import Path
from mathutils import Vector
P=Path(__file__).resolve().parent
bpy.ops.wm.open_mainfile(filepath=str(P/'SeasideMansion-DetailV2.blend'))
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=True
scene.render.resolution_x=1000;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
for c in bpy.data.collections:c.hide_render=False
for o in bpy.data.objects:o.hide_render=True
world=bpy.data.worlds.new('Studio');world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.5,.55,.65,1);world.node_tree.nodes['Background'].inputs[1].default_value=.5;scene.world=world
bpy.ops.mesh.primitive_plane_add(size=200);plane=bpy.context.object;plane.location.z=-.02
m=bpy.data.materials.new('Studio floor');m.diffuse_color=(.25,.23,.2,1);plane.data.materials.append(m)
for loc,power,size in [((4,-5,10),2200,6),((-5,-1,6),1200,5),((2,5,8),2000,4)]:
 bpy.ops.object.light_add(type='AREA',location=loc);o=bpy.context.object;o.data.energy=power;o.data.shape='DISK';o.data.size=size;o.rotation_euler=(Vector((0,0,2))-o.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add();cam=bpy.context.object;scene.camera=cam;cam.data.type='ORTHO'
for name,loc,target,scale in [('Fountain',(6,-8,5),(0,0,1.35),7.8),('Column',(5,-8,4.5),(0,0,2.3),5.6),('Palm',(8,-12,8),(0,0,3.5),8.5),('PlantBed',(6,-8,5),(0,0,.5),5.8)]:
 ob=bpy.data.objects[name];ob.hide_render=False;cam.location=loc;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=scale
 scene.render.filepath=str(P/(name+'-model.png'));bpy.ops.render.render(write_still=True);ob.hide_render=True
