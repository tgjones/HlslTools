//============================================
//
// Logical Operator Short Circuiting
//
//============================================

[numthreads(1,1,1)]
void main_short_circuiting() {
  int3 X = {1, 1, 1};
  int3 Y = {0, 0, 0};
    
  // Fails in HLSL 2021 (the operands must be scalars)
  bool3 Cond = X && Y;
  bool3 Cond2 = X || Y;
  int3 Z = X ? 1 : 0;

  // Replacement functions were added in HLSL 2021
  bool3 Cond3 = and(X, Y);
  bool3 Cond4 = or(X, Y);
  int3 Z2 = select(X, 1, 0);
}


////============================================
////
//// Template Functions and Data Types
////
////============================================
//
//template<typename T>
//void increment(inout T X) {
//  X += 1;
//}
//
//template<>
//void increment(inout float3 X) {
//  X.x += 1.0;
//}
//
//[numthreads(1,1,1)]
//void main_inferred_template_functions() {
//  int X = 0;
//  int3 Y = {0,0,0};
//  float3 Z = {0.0,0.0,0.0};
//  increment(X);
//  increment(Y);
//  increment(Z);
//}
//
//template<typename V, typename T>
//V cast(T X) {
//  return (V)X;
//}
//
//[numthreads(1,1,1)]
//void main_explicit_template_functions() {
//  int X = 1;
//  uint Y = cast<uint>(X);
//}
//
//
////============================================
////
//// Member Operator Overloading
////
////============================================
//
//struct Pupper {
//  int Fur;
//
//  Pupper operator +(int MoarFur) {
//    Pupper Val = {Fur + MoarFur};
//    return Val;
//  }
//
//  bool operator <=(int y){
//    return Fur <= y;
//  }
//
//  operator bool() {
//    return Fur > 50;
//  }
//};
//
//[numthreads(1, 1, 1)]
//void main_operator_overloading(uint tidx : SV_DispatchThreadId) {
//  Pupper y = {0};
//  for (Pupper x = y; x <= 100; x = x + 1) {
//    if ((bool)x)
//      y = y + 1;
//  }
//}
//
//
////============================================
////
//// Bitfield Members in Data Types
////
////============================================
//
//struct ColorRGBA {
//  uint R : 8;
//  uint G : 8;
//  uint B : 8;
//  uint A : 8;
//};
//
//
////============================================
////
//// Strict Casting of User-defined Data Types
////
////============================================
//
//struct LinearRGB {
//  float3 RGB;
//};
//
//struct LinearYCoCg {
//  float3 YCoCg;
//};
//
//void Modify(inout LinearRGB V) {
//  V.RGB.x += 1;
//}
//
//[numthreads(64, 1, 1)]
//void main_strict_casting() {
//  // Implicit cast to LinearRGB fails in HLSL 2021
//  LinearYCoCg V = {{0.0, 0.0, 0.0}};
//  Modify(V);
//
//  // Explicit cast to LinearRGB succeeds (because it's member layout is the same as LinearYCoCg)
//  LinearYCoCg V2 = {{0.0, 0.0, 0.0}};
//  Modify((LinearRGB)V2);
//}