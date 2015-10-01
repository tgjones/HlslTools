#version 140
in vec4 frag_TEXCOORD0;
out vec4 rast_SV_Target;
struct PS_INPUT {
    vec4 TexC;
};
uniform sampler2D TextureBase;
uniform sampler2D TextureDetail;
vec4 xx_main(PS_INPUT xx_input) {
    vec4 base = texture(TextureBase, xx_input.TexC);
    vec4 detail = texture(TextureDetail, xx_input.TexC);
    return base * detail;
}
void main() {
    PS_INPUT xx_input;
    xx_input.TexC = frag_TEXCOORD0;
    vec4 result = xx_main(xx_input);
    rast_SV_Target = result;
}
