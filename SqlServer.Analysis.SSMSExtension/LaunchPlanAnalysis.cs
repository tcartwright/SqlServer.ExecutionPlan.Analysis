//------------------------------------------------------------------------------
// <copyright file="LaunchPlanAnalysis.cs" company="Tim Cartwright">
//     Copyright (c) Tim Cartwright.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using SqlServer.ExecutionPlan.Analysis.SSMSAddin;

namespace SqlServer.ExecutionPlan.Analysis.SSMSExtension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class LaunchPlanAnalysis
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("EDCB9381-59B4-49C8-A36B-C887DF7AC119");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchPlanAnalysis"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private LaunchPlanAnalysis(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static LaunchPlanAnalysis Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new LaunchPlanAnalysis(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "LaunchPlanAnalysis";

            // Show a message box to prove we were here


            try
            {
                DTE dTE = (DTE)this.ServiceProvider.GetService(typeof(DTE));
                if (dTE == null)
                {
                    VsShellUtilities.ShowMessageBox(
                        this.ServiceProvider,
                        "Could not obtain a reference to the DTE object.",
                        title,
                        OLEMSGICON.OLEMSGICON_INFO,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                }
                else
                {
                    PlanAnalysis addin = new PlanAnalysis(dTE);
                    bool flag = false;
                    addin.ShowAnalysis(ref flag);
                }
            }
            catch (Exception ex)
            {
                VsShellUtilities.ShowMessageBox(
                    this.ServiceProvider,
                    "Exception: " + ex.Message,
                    title,
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }
    }
}
