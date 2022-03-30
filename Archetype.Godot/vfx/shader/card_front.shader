shader_type canvas_item;

uniform vec2 tiled_factor = vec2(2.0, 2.0);
uniform float time_factor = 1.0;

uniform vec2 amplitude = vec2(10.0, 4.0);

void vertex(){
	//VERTEX.x += cos(TIME * time_factor + VERTEX.y) * amplitude.x;
	//VERTEX.y += sin(TIME * time_factor) * amplitude.y;
}

void fragment(){
	vec2 tiled_uvs = UV * tiled_factor;
	
	float r = tiled_uvs.x + tiled_uvs.y;
	
	vec2 waves_uv_offset;
	waves_uv_offset.x = cos(TIME + r);// * 0.05;
	waves_uv_offset.y = sin(TIME + r);// * 0.05;
	
	//COLOR = vec4(waves_uv_offset, 0.0, 1.0);
	
	COLOR = texture(TEXTURE, tiled_uvs * waves_uv_offset);
}