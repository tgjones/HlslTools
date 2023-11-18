//// HLSL 2017 adds enum and enum class declarations.
//
//enum class MyEnum {
//    FIRST,
//    SECOND,
//    THIRD,
//    FOURTH,
//};
//
//enum class E : min16int {
//  E1 = 3,
//  E2
//};
//
//enum Vertex : int {
//    FIRST = 10,
//    SECOND = -2,
//    THIRD = 48,
//    FOURTH = -25,
//};
//
//enum Day {
//    MONDAY,
//    TUESDAY,
//    WEDNESDAY
//};
//
//int f(MyEnum v) {
//    switch (v) {
//        case MyEnum::FIRST:
//            return 1;
//        case MyEnum::SECOND:
//            return 2;
//        default:
//            return 0;
//    }
//}
//
//int g(Vertex v) {
//    return v + SECOND;
//}
//
//int4 main() : SV_Target {
//    return int3(f(MyEnum::FIRST), g(FOURTH), E::E1);
//}