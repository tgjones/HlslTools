using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ShaderTools.Hlsl.Parser;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ShaderTools")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("ShaderTools")]
[assembly: AssemblyCopyright("Copyright © 2015-2016 Tim Jones")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("823d71b9-0c8e-4f1f-9837-7284a8883c17")]

[assembly: AssemblyVersion(HlslParser.Version)]
[assembly: AssemblyFileVersion(HlslParser.Version)]

[assembly: InternalsVisibleTo("ShaderTools.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100db0e6b6325cc8d19c80ca770a04910ef5fa65f56adf2b3b5edf9941d151fa4441fb64c68f93b61491fe730fc97a0fff117482b91476b55310e7ef8b90dc0f88974fabbff0f410fbe5a709df50ba1892e01152656f590e1e1670d7ba006708dba2a4410217ba5a478d499e2d08748b9e2ee09a03e97100c9a5d2218c90ebfc9b9")]
[assembly: InternalsVisibleTo("ShaderTools.Editor.VisualStudio.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100db0e6b6325cc8d19c80ca770a04910ef5fa65f56adf2b3b5edf9941d151fa4441fb64c68f93b61491fe730fc97a0fff117482b91476b55310e7ef8b90dc0f88974fabbff0f410fbe5a709df50ba1892e01152656f590e1e1670d7ba006708dba2a4410217ba5a478d499e2d08748b9e2ee09a03e97100c9a5d2218c90ebfc9b9")]