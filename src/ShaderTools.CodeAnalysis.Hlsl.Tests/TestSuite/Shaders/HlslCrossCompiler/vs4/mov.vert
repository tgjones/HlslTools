#version 140
in vec4 POSITION;
struct VS_OUTPUT {
    vec4 Position;
};
VS_OUTPUT xx_main(in vec4 vPosition) {
    VS_OUTPUT Output;
    Output.Position = vPosition;
    return Output;
}
void main() {
    vec4 vPosition;
    vPosition = POSITION;
    VS_OUTPUT result = xx_main(vPosition);
    vec4 temp = result.Position;
    gl_Position = temp * vec4(1, -1, 2, 1) - vec4(0, 0, temp.w, 0);
}
