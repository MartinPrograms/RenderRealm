#version 410 core
#extension GL_NV_gpu_shader_fp64 : enable // Enable double-precision floats
// Mandelbrot set fractal shader.
#define INFINITY 1e+10

// Define the boundaries for the complex plane (real and imaginary axis)
const float MIN_X = -2.0;
const float MAX_X = 2.0;
const float MIN_Y = -2.0;
const float MAX_Y = 2.0;
const float zoomMin = -2.0;
const float zoomMax = 2.0;

uniform int iterations;
uniform double zoom;
uniform dvec2 pos; // f64 is a double-precision float (double c#)

uniform vec3 posLight;

uniform float u_time;

uniform dvec2 cref; // Reference point in the complex plane
uniform dvec2 crefOrbit[500]; // High-precision orbit of c_ref (max 1000 iterations)

in vec2 texCoord;

out vec4 FragColor;

uniform float aspectRatio;

void main() {
    vec2 uv = texCoord;

    // Adjust UV for aspect ratio and map to -1.0 to 1.0 range
    uv.x *= aspectRatio;
    uv = uv * 2.0 - 1.0;

    // Scale and offset UV coordinates
    dvec2 c = dvec2(uv.x, uv.y) * zoom + pos;
    dvec2 c_normalized;
    c_normalized.x = (c.x - zoomMin) / (zoomMax - zoomMin);
    c_normalized.y = (c.y - zoomMin) / (zoomMax - zoomMin);

    dvec2 normalizedDelta = dvec2(c_normalized) - dvec2(cref);

    vec2 dz = vec2(normalizedDelta);
    vec2 z = vec2(dz); // Convert to single precision for iteration
    float smoothN = 0.0;
    bool escaped = false;

    for (int i = 0; i < iterations; i++) {
        float z_ref_x = float(crefOrbit[i].x);
        float z_ref_y = float(crefOrbit[i].y);

        float z_x = z.x;
        float z_y = z.y;

        float z_x_new = z_x * z_x - z_y * z_y + z_ref_x + float(dz.x);
        float z_y_new = 2.0 * z_x * z_y + z_ref_y + float(dz.y);

        z = vec2(z_x_new, z_y_new);

        smoothN = float(i);
        if (length(z) > 2.0) {
            escaped = true;
            break;
        }
    }
    
    // Color based on smooth iteration count
    vec3 col = (vec3(z.xyy) + 1.0) / 2.0;
    // We can assume z as a primitive normal, for basic lighting
    
    vec3 normal = normalize(vec3(z.x, z.y, 1.0));
    if (escaped) {
        normal = vec3(0.0, 0.0, -1.0);
    }
    
    vec3 hitPos = vec3(c.x, c.y, 0.0);

    vec3 lightPos = posLight;
    vec3 lightDir = normalize(lightPos - hitPos);
    float diff = max(dot(normal, lightDir), 0.0); // diffuse factor
    
    // Specular light
    vec3 viewDir = normalize(vec3(0.0, 0.0, 1.0) - hitPos);
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
    vec3 specular = vec3(0.5) * spec;
    
    if (escaped) {
        col = vec3(0.0);
    }
    
    col *= diff + specular;
    
    FragColor = vec4(vec3(col), 1.0);
    // Also show the position of the light
    if (length(c.xy - posLight.xy) < 0.01) {
        FragColor = vec4(1.0, 0.0, 0.0, 1.0);
    }
}