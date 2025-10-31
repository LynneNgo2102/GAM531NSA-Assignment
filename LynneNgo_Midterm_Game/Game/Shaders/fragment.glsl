#version 330 core
in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;


out vec4 FragColor;


struct Light {
    vec3 position;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};


uniform sampler2D texture_diffuse1;
uniform vec3 viewPos;
uniform Light light;
uniform float shininess;
uniform int lightOn; // Used for the on/off logic

void main() {
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(light.position - FragPos);
    
    // 1. Ambient
    vec3 ambient = light.ambient * vec3(texture(texture_diffuse1, TexCoord));
    
    // 2. Diffuse
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = light.diffuse * diff * vec3(texture(texture_diffuse1, TexCoord));
    
    // 3. Specular
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), shininess);
    vec3 specular = light.specular * spec;

    vec3 result;
    if (lightOn == 1) {
        result = ambient + diffuse + specular;
    } else {
        // If the main light is off, only use a small amount of ambient light 
        // to prevent total darkness (fulfills "dim when off" idea)
        result = vec3(0.1f) * vec3(texture(texture_diffuse1, TexCoord));
    }

    FragColor = vec4(result, 1.0);
}