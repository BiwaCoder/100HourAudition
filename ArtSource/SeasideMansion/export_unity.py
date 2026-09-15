import bpy,json
from pathlib import Path
root=Path(__file__).resolve().parents[2]
bpy.ops.wm.open_mainfile(filepath=str(root/'ArtSource/SeasideMansion/SeasideMansion.blend'))
# Remove coplanar roof overlaps and flush approach surfaces before exporting.
for o in bpy.data.objects:
 if o.name.startswith('Flat roof slab') and abs(o.location.x)>14:o.dimensions.x=10
 if o.name=='Entrance approach paving':o.location.z=-.045
bpy.ops.object.select_all(action='DESELECT')
for n in ['Architecture','Roof - hide to edit interiors','Garden and fountain','Interior furniture']:
 for o in bpy.data.collections[n].objects:o.select_set(True)
# Mesh conversion ensures fountain curves are exported too.
bpy.context.view_layer.objects.active=next(o for o in bpy.context.selected_objects if o.type=='MESH')
bpy.ops.object.convert(target='MESH')
out=root/'Assets/Environments/SeasideMansion';out.mkdir(parents=True,exist_ok=True)
bpy.ops.export_scene.fbx(filepath=str(out/'SeasideMansion.fbx'),use_selection=True,object_types={'MESH','EMPTY'},apply_unit_scale=True,axis_forward='-Z',axis_up='Y',bake_anim=False,add_leaf_bones=False,use_mesh_modifiers=True)
mats=[]
for m in bpy.data.materials:
 if not m.use_nodes:continue
 p=m.node_tree.nodes.get('Principled BSDF')
 if p:mats.append({'name':m.name,'color':list(p.inputs['Base Color'].default_value),'metallic':p.inputs['Metallic'].default_value,'roughness':p.inputs['Roughness'].default_value,'alpha':p.inputs['Alpha'].default_value,'emission':list(p.inputs['Emission Color'].default_value),'emissionStrength':p.inputs['Emission Strength'].default_value})
(out/'MaterialPalette.json').write_text(json.dumps({'materials':mats},indent=2))
print('UNITY_FBX_EXPORTED')
