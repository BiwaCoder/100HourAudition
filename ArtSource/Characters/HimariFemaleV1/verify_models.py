import bpy,json,math,sys
from pathlib import Path
from mathutils import Vector
P=Path(__file__).resolve().parent
report=[]
DO_RENDER='--no-render' not in sys.argv
for name in ['Himari-Wire','Himari-StreetCel','Himari-Anime']:
 bpy.ops.wm.open_mainfile(filepath=str(P/(name+'.blend')))
 arm=bpy.data.objects.get('Himari_Rig');objects=[o for o in bpy.data.objects if o.type=='MESH' and o.name.startswith('Himari_')]
 entry={'model':name,'blend_opens':True,'bones':len(arm.data.bones),'actions':[a.name for a in bpy.data.actions]}
 bad=[]
 for o in objects:
  for v in o.data.vertices:
   total=sum(g.weight for g in v.groups)
   if abs(total-1)>.001:bad.append((o.name,v.index,total))
  for g in o.vertex_groups:
   if g.name not in arm.data.bones:bad.append((o.name,g.name,'missing bone'))
 entry['body_materials']=len(next(o for o in objects if o.name.endswith('_Body')).data.materials);entry['vertex_colors']=bool(next(o for o in objects if o.name.endswith('_Body')).data.color_attributes.get('Color'))
 entry['weight_errors']=len(bad);entry['weight_error_sample']=bad[:5]
 entry['pose_bounds']=[]
 arm.animation_data.action=bpy.data.actions['Himari_WalkPreview']
 for frame in [1,9,17,25,33]:
  bpy.context.scene.frame_set(frame);dg=bpy.context.evaluated_depsgraph_get();points=[]
  for o in objects:
   eo=o.evaluated_get(dg);me=eo.to_mesh();points += [eo.matrix_world@v.co for v in me.vertices];eo.to_mesh_clear()
  mins=[min(p[k] for p in points) for k in range(3)];maxs=[max(p[k] for p in points) for k in range(3)]
  entry['pose_bounds'].append({'frame':frame,'min':mins,'max':maxs,'finite':all(math.isfinite(t) for t in mins+maxs)})
 bpy.context.scene.frame_set(9);scene=bpy.context.scene
 if DO_RENDER:
  scene.cycles.samples=20;scene.render.filepath=str(P/(name+'-walk.png'));bpy.ops.render.render(write_still=True)
 if DO_RENDER and name=='Himari-Wire':
  # Fixed rest pose, changed procedural light only.
  arm.animation_data.action=None
  for pb in arm.pose.bones:pb.rotation_euler=(0,0,0)
  for frame in [1,49]:
   scene.frame_set(frame);scene.render.filepath=str(P/(name+f'-light-{frame}.png'));bpy.ops.render.render(write_still=True)
 bpy.ops.wm.read_factory_settings(use_empty=True)
 bpy.ops.import_scene.fbx(filepath=str(P/(name+'.fbx')),use_anim=True)
 entry['fbx_reimport']={'meshes':len([o for o in bpy.data.objects if o.type=='MESH']),'armatures':len([o for o in bpy.data.objects if o.type=='ARMATURE']),'actions':len(bpy.data.actions),'vertices':sum(len(o.data.vertices) for o in bpy.data.objects if o.type=='MESH')}
 entry['fbx_reimport']['meshes_with_vertex_colors']=len([o for o in bpy.data.objects if o.type=='MESH' and len(o.data.color_attributes)>0])
 entry['pass']=entry['vertex_colors'] and entry['body_materials']==1 and entry['fbx_reimport']['meshes_with_vertex_colors']>=1 and not bad and all(p['finite'] for p in entry['pose_bounds']) and entry['fbx_reimport']['armatures']==1 and entry['fbx_reimport']['actions']>=2
 report.append(entry)
(P/'Verification.json').write_text(json.dumps(report,indent=2));print('VERIFICATION',json.dumps(report))
