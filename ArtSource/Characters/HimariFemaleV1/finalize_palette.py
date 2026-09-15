import bpy,sys,json
from pathlib import Path
P=Path(__file__).resolve().parent;sys.path.insert(0,str(P));from palette import merge_palette
for name in ['Himari-Wire','Himari-StreetCel','Himari-Anime']:
 bpy.ops.wm.open_mainfile(filepath=str(P/(name+'.blend')))
 body=next(o for o in bpy.data.objects if o.name.startswith('Himari_') and o.name.endswith('_Body'))
 merge_palette(body);arm=bpy.data.objects['Himari_Rig'];bpy.ops.wm.save_as_mainfile(filepath=str(P/(name+'.blend')))
 bpy.ops.object.select_all(action='DESELECT');arm.select_set(True);body.select_set(True)
 for o in bpy.data.objects:
  if o.name.endswith('_LuminousContours'):o.select_set(True)
 bpy.context.view_layer.objects.active=arm
 bpy.ops.export_scene.fbx(filepath=str(P/(name+'.fbx')),use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,axis_forward='-Z',axis_up='Y',mesh_smooth_type='FACE',use_mesh_modifiers=True)
 stats=json.loads((P/(name+'-stats.json')).read_text());stats['materials']=[m.name for m in body.data.materials];stats['height_m']+=.008;stats['vertex_palette']='Color';(P/(name+'-stats.json')).write_text(json.dumps(stats,indent=2))
print('PALETTE_FINALIZED')
