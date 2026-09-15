"""Final architectural and lighting refinements; also invoked by the generator."""
import bpy,math
arch=bpy.data.collections['Architecture']; envc=bpy.data.collections['Environment - not exported']
def box(n,loc,size,material,col=arch):
 bpy.ops.mesh.primitive_cube_add(size=1,location=loc); o=bpy.context.object;o.name=n;o.dimensions=size
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 for c in list(o.users_collection):c.objects.unlink(o)
 col.objects.link(o);o.data.materials.append(bpy.data.materials[material]);b=o.modifiers.new('Edge highlight','BEVEL');b.width=.025;b.segments=2
 return o
for n in ['Entry facade left','Entry facade right']:
 ob=bpy.data.objects.get(n)
 if ob:bpy.data.objects.remove(ob,do_unlink=True)
for sign in [-1,1]:
 for a,b in [(2,4.5),(9.5,12),(18,20)]:box('Front facade pier',(sign*(a+b)/2,-19,2.25),(b-a,.25,4.5),'Warm ivory stucco')
 for a,b in [(4.5,9.5),(12,18)]:
  x=sign*(a+b)/2;w=b-a
  box('Front window sill',(x,-19,.3),(w,.3,.6),'White marble trim')
  box('Front window lintel',(x,-19,4),(w,.3,1),'Warm ivory stucco')
  box('Front glazing',(x,-19,2.05),(w,.03,2.9),'Ocean glazing')
  for dx in [-w/2,0,w/2]:box('Front bronze mullion',(x+dx,-19,2.05),(.09,.13,2.9),'Bronze window frames')
box('Entrance approach paving',(0,-24,-.06),(4,10,.12),'Pearl travertine')
# Render environment: physically sized ripples and a warm sun above the sea.
ocean=bpy.data.materials['Ocean blue'];nodes=ocean.node_tree.nodes
geom=nodes.new('ShaderNodeNewGeometry');noise=next(n for n in nodes if n.type=='TEX_NOISE');noise.inputs['Scale'].default_value=.8
ocean.node_tree.links.new(geom.outputs['Position'],noise.inputs['Vector'])
for n in bpy.context.scene.world.node_tree.nodes:
 if n.type=='TEX_SKY':n.sun_elevation=math.radians(4);n.sun_rotation=math.radians(90);n.air_density=1;n.dust_density=.5
bpy.context.scene.world.node_tree.nodes.get('Background').inputs['Strength'].default_value=.35
# A small distant emissive disc makes sunset legible from the room viewpoint.
m=bpy.data.materials.new('Sunset disc');m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(1,.32,.055,1);p.inputs['Emission Color'].default_value=(1,.32,.055,1);p.inputs['Emission Strength'].default_value=4
bpy.ops.mesh.primitive_uv_sphere_add(segments=32,ring_count=16,radius=7,location=(25,380,22));o=bpy.context.object;o.name='Distant sunset disc'
for c in list(o.users_collection):c.objects.unlink(o)
envc.objects.link(o);o.data.materials.append(m)
# Preserve furniture position, but soften cushions for close interior viewing.
for o in bpy.data.objects:
 if o.name.startswith('Teal cushion'):
  sub=o.modifiers.new('Cushion softness','SUBSURF');sub.levels=2;sub.render_levels=2
for c in bpy.data.collections:
 for o in c.objects:o.select_set(False)
