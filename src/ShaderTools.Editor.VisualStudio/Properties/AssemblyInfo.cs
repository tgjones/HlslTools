using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using ShaderTools.Editor.VisualStudio.Hlsl;
using ShaderTools.VisualStudio.LanguageServices;

[assembly: AssemblyTitle("HLSL Tools for Visual Studio")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Tim Jones")]
[assembly: AssemblyProduct("HLSL Tools for Visual Studio")]
[assembly: AssemblyCopyright("Copyright © 2015-2017 Tim Jones")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]   
[assembly: ComVisible(false)]     
[assembly: CLSCompliant(false)]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: AssemblyVersion(ShaderToolsPackage.Version)]
[assembly: AssemblyFileVersion(ShaderToolsPackage.Version)]

[assembly: InternalsVisibleTo("ShaderTools.Editor.VisualStudio.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

[assembly: ProvideBindingRedirection(AssemblyName = "System.IO.FileSystem", PublicKeyToken = "b03f5f7f11d50a3a", OldVersionLowerBound = "0.0.0.0", OldVersionUpperBound = "4.0.2.0", NewVersion = "4.0.2.0")]
[assembly: ProvideBindingRedirection(AssemblyName = "System.IO.FileSystem.Primitives", PublicKeyToken = "b03f5f7f11d50a3a", OldVersionLowerBound = "0.0.0.0", OldVersionUpperBound = "4.0.2.0", NewVersion = "4.0.2.0")]
[assembly: ProvideBindingRedirection(AssemblyName = "System.Runtime.Serialization.Primitives", PublicKeyToken = "b03f5f7f11d50a3a", OldVersionLowerBound = "0.0.0.0", OldVersionUpperBound = "4.1.2.0", NewVersion = "4.1.2.0")]
[assembly: ProvideBindingRedirection(AssemblyName = "System.ValueTuple", PublicKeyToken = "b03f5f7f11d50a3a", OldVersionLowerBound = "0.0.0.0", OldVersionUpperBound = "4.0.1.0", NewVersion = "4.0.1.0")]