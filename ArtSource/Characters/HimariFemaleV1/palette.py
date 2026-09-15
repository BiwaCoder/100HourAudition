import bpy,json

def merge_palette(body):
 """One exported material + vertex colors; original design regions remain editable."""
 if body.data.color_attributes.get('Color'):return
 mesh=body.data;legend=[{'name':m.name,'rgba':list(m.diffuse_color)} for m in mesh.materials]
 attr=mesh.color_attributes.new(name='Color',type='FLOAT_COLOR',domain='CORNER')
 region=mesh.attributes.new(name='DesignRegion',type='INT',domain='FACE')
 for poly in mesh.polygons:
  idx=poly.material_index;region.data[poly.index].value=idx
  for loop in poly.loop_indices:attr.data[loop].color=legend[idx]['rgba']
  poly.material_index=0
 body['DesignRegion_palette']=json.dumps(legend)
 # Reuse the same quantized normal lighting as the individual palette materials.
 material=bpy.data.materials.new('Himari - vertex palette');material.diffuse_color=(1,1,1,1);material.use_nodes=True;n=material.node_tree.nodes;n.clear();l=material.node_tree.links
 out=n.new('ShaderNodeOutputMaterial');out.location=(700,0);em=n.new('ShaderNodeEmission');em.location=(500,0)
 vc=n.new('ShaderNodeVertexColor');vc.layer_name='Color';vc.location=(-200,200)
 geo=n.new('ShaderNodeNewGeometry');geo.location=(-700,0);dot=n.new('ShaderNodeVectorMath');dot.operation='DOT_PRODUCT';dot.inputs[1].default_value=(-.45,-.65,.62);dot.location=(-510,0)
 rem=n.new('ShaderNodeMath');rem.operation='MULTIPLY_ADD';rem.inputs[1].default_value=.5;rem.inputs[2].default_value=.5;rem.location=(-340,0)
 ramp=n.new('ShaderNodeValToRGB');ramp.location=(-120,0);r=ramp.color_ramp;r.interpolation='CONSTANT';r.elements[0].position=0;r.elements[0].color=(.48,.48,.48,1);r.elements[1].position=.68;r.elements[1].color=(1,1,1,1);r.elements.new(.35).color=(.75,.75,.75,1)
 mul=n.new('ShaderNodeMixRGB');mul.blend_type='MULTIPLY';mul.inputs[0].default_value=1;mul.location=(260,0)
 l.new(geo.outputs['Normal'],dot.inputs[0]);l.new(dot.outputs['Value'],rem.inputs[0]);l.new(rem.outputs[0],ramp.inputs[0]);l.new(vc.outputs['Color'],mul.inputs[1]);l.new(ramp.outputs[0],mul.inputs[2]);l.new(mul.outputs[0],em.inputs[0]);l.new(em.outputs[0],out.inputs[0])
 mesh.materials.clear();mesh.materials.append(material)
