//------------------------------------------------------------------------------
// <copyright file="LaunchPlanAnalysisPackage.cs" company="Tim Cartwright">
//     Copyright (c) Tim Cartwright.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;

namespace SqlServer.ExecutionPlan.Analysis.SSMSExtension
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideBindingPath(SubPath = "SQL Server Execution Plan Analysis Extension")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class LaunchPlanAnalysisPackage : Package
    {
        /// <summary>
        /// LaunchPlanAnalysisPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "4f435be6-6197-4074-9f8e-9040e9ec0ac1";

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchPlanAnalysis"/> class.
        /// </summary>
        public LaunchPlanAnalysisPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            LaunchPlanAnalysis.Initialize(this);
            base.Initialize();
        }

        protected override int QueryClose(out bool canClose)
        {
            WriteSkipLoading();
            return base.QueryClose(out canClose);
        }

        public static void WriteSkipLoading()
        {
            try
            {
                //HACK to work around SSMS add in whitelist:
                //https://social.msdn.microsoft.com/Forums/sqlserver/en-US/efb4bdf3-6bd7-44ff-8431-5f67f3399f29/how-to-createdeploy-vspackage-for-sql-server-management-studio?forum=vsx
                //HAS TO BE WRITTEN EACH TIME IT IS CLOSED
                for(var i = 12; i <= 20; i++)
                {
                    var baseKeyPath = $@"Software\Microsoft\SQL Server Management Studio\{i}.0";
                    var keyPath = $@"{baseKeyPath}\Packages\{{{PackageGuidString}}}";

                    var sqlKey = Registry.CurrentUser.OpenSubKey(baseKeyPath, false);

                    if (sqlKey != null)
                    {
                        using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath))
                        {
                            key.SetValue("SkipLoading", 1, RegistryValueKind.DWord);
                            key.Close();
                        }
                    }
                }

            }
            catch
            {
                //eat any exceptions... this will suck tho as the addin will no longer load.. :|
            }

        }
        #endregion
    }
}
