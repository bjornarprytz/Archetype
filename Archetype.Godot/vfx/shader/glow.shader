shader_type spatial;

void fragment(){
	float g = (sin(TIME) + 1.0 ) * 0.5;
	float r = (cos(TIME) + 1.0 ) * 0.5;
	
	ALBEDO = vec3(r, g, 0.0);
}