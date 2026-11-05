shader_type canvas_item;

// Frosted card shader for the Telegram-like UI.
// Blurs the screen behind the card, adds a soft vertical tint and a light rim.
// No warping or chromatic aberration, so text and icons stay crisp.

uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;

uniform float blur : hint_range(0.0, 8.0) = 2.5;
uniform float corner_radius : hint_range(0.0, 1.0) = 0.18;
uniform float edge_smoothness : hint_range(0.5, 3.0) = 1.0;

uniform vec4 top_color : source_color = vec4(1.0, 1.0, 1.0, 0.55);
uniform vec4 bottom_color : source_color = vec4(0.86, 0.93, 1.0, 0.30);
uniform vec4 rim_color : source_color = vec4(1.0, 1.0, 1.0, 0.55);
uniform float rim_width : hint_range(0.0, 10.0) = 1.5;
uniform float specular_strength : hint_range(0.0, 0.3) = 0.06;

float rounded_box(vec2 p, vec2 b, float r) {
    vec2 q = abs(p) - b + r;
    return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r;
}

void fragment() {
    float dx = dFdx(UV.x);
    float dy = dFdy(UV.y);

    dx = sign(dx) * max(abs(dx), 0.0001);
    dy = sign(dy) * max(abs(dy), 0.0001);

    vec2 size = clamp(abs(vec2(1.0 / dx, 1.0 / dy)), vec2(1.0), vec2(4096.0));

    vec2 half_size = size * 0.5;
    vec2 pixel = UV * size;
    vec2 centered = pixel - half_size;

    float max_r = min(half_size.x, half_size.y);
    float r = corner_radius * max_r;

    float sdf = rounded_box(centered, half_size, r);
    float alpha = 1.0 - smoothstep(-edge_smoothness, edge_smoothness, sdf);

    if (alpha < 0.001) {
        discard;
    }

    vec3 bg = textureLod(screen_texture, SCREEN_UV, blur).rgb;

    float grad_t = 1.0 - UV.y;
    vec3 grad_rgb = mix(bottom_color.rgb, top_color.rgb, grad_t);
    float grad_a = mix(bottom_color.a, top_color.a, grad_t);

    vec3 color = mix(bg, grad_rgb, grad_a);

    float rim = 1.0 - smoothstep(0.0, rim_width, -sdf);
    rim *= step(sdf, 0.0);
    color = mix(color, rim_color.rgb, rim * rim_color.a);

    float spec = smoothstep(0.35, 0.0, UV.y) * specular_strength;
    color += vec3(spec);

    COLOR = vec4(color, alpha);
}
