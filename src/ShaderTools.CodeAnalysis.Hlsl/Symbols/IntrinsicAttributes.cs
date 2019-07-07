using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    internal static class IntrinsicAttributes
    {
        public static readonly ImmutableArray<AttributeSymbol> AllAttributes;

        static IntrinsicAttributes()
        {
            var allAttributes = new List<AttributeSymbol>();

            allAttributes.Add(Create("allow_uav_condition", "Allows a compute shader loop termination condition to be based off of a UAV read. The loop must not contain synchronization intrinsics."));
            allAttributes.Add(Create("branch", "When applied to if statements: Evaluate only one side of the if statement depending on the given condition.\nWhen applied to switch statements: Compile the statement as a series of if statements each with the branch attribute."));
            allAttributes.Add(Create("call", "The bodies of the individual cases in the switch will be moved into hardware subroutines and the switch will be a series of subroutine calls."));
            allAttributes.Add(Create("fastopt", "Reduces the compile time but produces less aggressive optimizations. If you use this attribute, the compiler will not unroll loops."));
            allAttributes.Add(Create("flatten", "When applied to if statements: Evaluate both sides of the if statement and choose between the two resulting values.\nWhen applied to switch statements: Compile the statement as a series of if statements, each with the flatten attribute."));
            allAttributes.Add(Create("forcecase", "Force a switch statement in the hardware."));
            allAttributes.Add(Create("loop", "Generate code that uses flow control to execute each iteration of the loop. Not compatible with the [unroll] attribute."));

            allAttributes.Add(Create("unroll", "Unroll the loop until it stops executing. Can optionally specify the maximum number of times the loop is to execute. Not compatible with the [loop] attribute.", a => new[]
            {
                new ParameterSymbol("count", "Maximum number of times the loop is to execute.", a, IntrinsicTypes.Int)
            }));

            allAttributes.Add(Create("clipplanes", "", a => new[]
            {
                new ParameterSymbol("clipPlane1", "", a, IntrinsicTypes.Float4),
                new ParameterSymbol("clipPlane2", "", a, IntrinsicTypes.Float4),
                new ParameterSymbol("clipPlane3", "", a, IntrinsicTypes.Float4),
                new ParameterSymbol("clipPlane4", "", a, IntrinsicTypes.Float4),
                new ParameterSymbol("clipPlane5", "", a, IntrinsicTypes.Float4),
                new ParameterSymbol("clipPlane6", "", a, IntrinsicTypes.Float4)
            }));

            allAttributes.Add(Create("domain", "Defines the patch type used in the HS.", a => new[]
            {
                new ParameterSymbol("domainType", "", a, IntrinsicTypes.String)
            }));

            allAttributes.Add(Create("earlydepthstencil", "Forces depth-stencil testing before a shader executes."));

            allAttributes.Add(Create("instance", "Use this attribute to instance a geometry shader.", a => new[]
            {
                new ParameterSymbol("count", "An integer index that indicates the number of instances to be executed for each drawn item (for example, for each triangle).", a, IntrinsicTypes.Int)
            }));

            allAttributes.Add(Create("maxtessfactor", "Indicates the maximum value that the hull shader would return for any tessellation factor.", a => new[]
            {
                new ParameterSymbol("factor", "Upper bound on the amount of tessellation requested to help a driver determine the maximum amount of resources required for tessellation.", a, IntrinsicTypes.Float)
            }));

            allAttributes.Add(Create("numthreads", "Defines the number of threads to be executed in a single thread group when a compute shader is dispatched.", a => new[]
            {
                new ParameterSymbol("x", "Size of the thread group in the X direction.", a, IntrinsicTypes.Int),
                new ParameterSymbol("y", "Size of the thread group in the Y direction.", a, IntrinsicTypes.Int),
                new ParameterSymbol("z", "Size of the thread group in the Z direction.", a, IntrinsicTypes.Int)
            }));

            allAttributes.Add(Create("outputcontrolpoints", "Defines the number of output control points (per thread) that will be created in the hull shader.", a => new[]
            {
                new ParameterSymbol("count", "", a, IntrinsicTypes.Int)
            }));

            allAttributes.Add(Create("outputtopology", "Defines the output primitive type for the tessellator.", a => new[]
            {
                new ParameterSymbol("topology", "Must be one of \"point\", \"line\", \"triangle_cw\", or \"triangle_ccw\".", a, IntrinsicTypes.String)
            }));

            allAttributes.Add(Create("partioning", "Defines the tesselation scheme to be used in the hull shader.", a => new[]
            {
                new ParameterSymbol("scheme", "Must be one of \"integer\", \"fractional_even\", \"fractional_odd\", or \"pow2\".", a, IntrinsicTypes.String)
            }));

            allAttributes.Add(Create("patchconstantfunc", "Defines the function for computing patch constant data.", a => new[]
            {
                new ParameterSymbol("functionName", "Name of a separate function that outputs the patch-constant data.", a, IntrinsicTypes.String)
            }));

            allAttributes.Add(Create("maxvertexcount", "Declares the maximum number of vertices to create.", a => new[]
            {
                new ParameterSymbol("count", "Maximum number of vertices.", a, IntrinsicTypes.Int)
            }));

            allAttributes.Add(Create("intrinsic", "", a => new[]
            {
                new ParameterSymbol("group", "", a, IntrinsicTypes.String),
                new ParameterSymbol("opcode", "", a, IntrinsicTypes.Int)
            }));

            allAttributes.Add(Create("RootSignature", "Defines the root signature.", a => new[]
            {
                new ParameterSymbol("signature", "", a, IntrinsicTypes.String)
            }));

            AllAttributes = allAttributes.ToImmutableArray();
        }

        private static AttributeSymbol Create(string name, string documentation, Func<AttributeSymbol, ParameterSymbol[]> createParameters = null)
        {
            var result = new AttributeSymbol(name, documentation);
            if (createParameters != null)
                foreach (var parameter in createParameters(result))
                    result.AddParameter(parameter);
            return result;
        }
    }
}