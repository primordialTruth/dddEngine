float costheta= clamp(dot(n,1),0,1);

//ins from VS
in vec3 position;
in vec3 camera_normal;
in vec3 eye_direction;
in vec3 light_direction;
//CONSTANT UNIFORM
uniform vec3 light_position;

//data output
out vec3 newcolor;

void main(){
	vec3 LightColor = vec3(1,1,1);
	//float LightPower = 50.0f;
	newcolor = LightColor * costheta;

	//light distance
	//float distance = length(light_position - position);

	//frag normals
	vec3 n = normalize(camera_normal);
	//light direction
	vec3 l = normalize(light_direction);

	float cosTheta = clamp( dot(n,l),0,1);
	newcolor = LightColor * cosTheta;
}