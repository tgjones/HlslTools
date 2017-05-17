//------------------------------------------------------------------------------
// <copyright file="SyntaxVisualizerPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ShaderTools.VisualStudio.SyntaxVisualizer
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(SyntaxVisualizerToolWindow))]
    public sealed class SyntaxVisualizerPackage : Package
    {
        public const string PackageGuidString = "fbbea939-3d7a-4430-a90a-84bf062584a4";

        protected override void Initialize()
        {
            SyntaxVisualizerToolWindowCommand.Initialize(this);

            base.Initialize();
        }
    }
}
