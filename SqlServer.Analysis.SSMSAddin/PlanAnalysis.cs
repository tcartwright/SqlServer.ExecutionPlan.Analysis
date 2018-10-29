// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis.SSMSAddin
// Author           : tdcart
// Created          : 07-03-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-04-2016
// ***********************************************************************
// <copyright file="Connect.cs" company="Tim Cartwright">
//     Copyright (c) Tim Cartwright. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using SqlServer.ExecutionPlan.Analysis.SSMSAddin.Properties;
using Process = System.Diagnostics.Process;

// ReSharper disable RedundantAssignment

// ReSharper disable InconsistentNaming

namespace SqlServer.ExecutionPlan.Analysis.SSMSAddin
{
	//http://sqlblogcasts.com/blogs/jonsayce/archive/2008/01/15/building-a-sql-server-management-studio-addin.aspx
	//http://solutioncenter.apexsql.com/the-art-and-science-of-creating-ssms-2012-add-ins-part-1-laying-the-foundation/

	/// <summary>
	/// Class Connect.
	/// </summary>
	/// <seealso cref="Extensibility.IDTExtensibility2" />
	/// <seealso cref="EnvDTE.IDTCommandTarget" />
	[ProgId("SqlServer.ExecutionPlan.Analysis.SSMSAddin")]
	public class PlanAnalysis : IDTExtensibility2, IDTCommandTarget
	{
		private DTE _DTE;
		private AddIn _AddIn;
		private CommandBar _ExecutionPlanBar;

		private static readonly StringComparer _comparer = StringComparer.InvariantCultureIgnoreCase;
		private static string _ProductName = "SQL Server Execution Plan Analysis";
		private static readonly string _CommandNameFriendly = "View with " + _ProductName;
		private static string _CommandName = "ViewExecutionPlanAnalysis";
		private static string _CommandNameFormatted;
		private readonly Type _showPlanControl;
		private readonly Control _activeScriptEditorControl;
		private readonly MethodInfo _getShowPlanXml;
		private readonly MethodInfo _getGraphPanel;
		private readonly FieldInfo _dataBindings;

		private string ActiveWindowCaption
		{
			get
			{
				var activeWindow = _DTE.ActiveWindow;
				string text = null;
				if (activeWindow.Document != null && activeWindow.Document.Saved)
				{
					text = activeWindow.Document.FullName;
				}
				if (string.IsNullOrEmpty(text))
				{
					text = activeWindow.Caption;
				}
				return text;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PlanAnalysis"/> class.
		/// </summary>
		public PlanAnalysis()
		{
			var assembly = Assembly.Load("SQLEditors");
			if (assembly == null)
			{
				MessageBox.Show(Resources.PlanAnalysis_PlanAnalysis_Could_not_load_SQLEditors_assembly, _ProductName);
				return;
			}
			var scriptEditorControl = assembly.GetType("Microsoft.SqlServer.Management.UI.VSIntegration.Editors.ScriptEditorControl");
			if (scriptEditorControl == null)
			{
				MessageBox.Show(Resources.PlanAnalysis_PlanAnalysis_Could_not_load_the_ScriptEditorControl_type_from_the_SQLEditors_assembly, _ProductName);
				return;
			}
			_showPlanControl = assembly.GetType("Microsoft.SqlServer.Management.UI.VSIntegration.Editors.ShowPlan.ShowPlanControl");
			if (_showPlanControl == null)
			{
				MessageBox.Show(Resources.PlanAnalysis_PlanAnalysis_Could_not_load_the_ShowPlanControl_type_from_the_SQLEditors_assembly, _ProductName);
				return;
			}
			_getShowPlanXml = _showPlanControl.GetMethod("GetShowPlanXml", BindingFlags.Instance | BindingFlags.NonPublic);
			_dataBindings = _showPlanControl.GetField("dataBindings", BindingFlags.Instance | BindingFlags.NonPublic);
			_getGraphPanel = _showPlanControl.GetMethod("GetGraphPanel", BindingFlags.Instance | BindingFlags.Public);

			var getActiveScriptEditorControl = scriptEditorControl.GetMethod("GetActiveScriptEditorControl", BindingFlags.Static | BindingFlags.Public) ??
											   scriptEditorControl.GetMethod("GetActiveScriptEditorControl", BindingFlags.Static | BindingFlags.NonPublic);
			
			_activeScriptEditorControl = getActiveScriptEditorControl != null
				? getActiveScriptEditorControl.Invoke(null, null) as Control
				: GeScriptEditorControl(scriptEditorControl) as Control;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dte"></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dte")]
		public PlanAnalysis(DTE dte) : this()
		{
			this._DTE = dte;
		}

		#region IDTExtensibility2

		/// <summary>
		/// Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.
		/// </summary>
		/// <param name="Application">The application.</param>
		/// <param name="ConnectMode">The connect mode.</param>
		/// <param name="AddInInst">The add in inst.</param>
		/// <param name="custom">The custom.</param>
		/// <seealso class="IDTExtensibility2" />
		public void OnConnection(object Application, ext_ConnectMode ConnectMode, object AddInInst, ref Array custom)
		{
			_DTE = (DTE)Application;
			_AddIn = (AddIn)AddInInst;

			_CommandNameFormatted = String.Format("{0}.{1}", _AddIn.ProgID, _CommandName);

			var commandBars = _DTE.CommandBars as CommandBars;
			if (commandBars == null) { return; }
			foreach (CommandBar commandBar in commandBars)
			{
				if (!_comparer.Equals(commandBar.Name, "Execution Plan Context")) { continue; }
				_ExecutionPlanBar = commandBar;
				break;
			}
		}

		/// <summary>
		/// Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.
		/// </summary>
		/// <param name="RemoveMode">The remove mode.</param>
		/// <param name="custom">The custom.</param>
		/// <seealso class="IDTExtensibility2" />
		public void OnDisconnection(ext_DisconnectMode RemoveMode, ref Array custom)
		{
		}

		/// <summary>
		/// Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.
		/// </summary>
		/// <param name="custom">The custom.</param>
		/// <seealso class="IDTExtensibility2" />
		public void OnAddInsUpdate(ref Array custom)
		{
		}

		/// <summary>
		/// Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.
		/// </summary>
		/// <param name="custom">The custom.</param>
		/// <seealso class="IDTExtensibility2" />
		public void OnStartupComplete(ref Array custom)
		{
			this.CreateMenuButton(_CommandName, 0, _CommandNameFriendly, _ExecutionPlanBar, _CommandNameFriendly, null, true);
		}

		/// <summary>
		/// Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.
		/// </summary>
		/// <param name="custom">The custom.</param>
		/// <seealso class="IDTExtensibility2" />
		public void OnBeginShutdown(ref Array custom)
		{
		}

		#endregion IDTExtensibility2

		#region IDTCommandTarget Members

		/// <summary>
		/// Executes the specified command name.
		/// </summary>
		/// <param name="CmdName">Name of the command.</param>
		/// <param name="ExecuteOption">The execute option.</param>
		/// <param name="VariantIn">The variant in.</param>
		/// <param name="VariantOut">The variant out.</param>
		/// <param name="Handled">if set to <c>true</c> [handled].</param>
		public void Exec(string CmdName, vsCommandExecOption ExecuteOption, ref object VariantIn, ref object VariantOut, ref bool Handled)
		{
			Handled = false;
			if (ExecuteOption == vsCommandExecOption.vsCommandExecOptionDoDefault && _comparer.Equals(CmdName, _CommandNameFormatted))
			{
				try
				{
					Cursor.Current = Cursors.WaitCursor;
					this.ShowAnalysis(ref Handled);
				}
				finally
				{
					Cursor.Current = Cursors.Default;
				}
			}

		}
		/// <summary>
		/// Queries the status.
		/// </summary>
		/// <param name="CmdName">Name of the command.</param>
		/// <param name="NeededText">The needed text.</param>
		/// <param name="StatusOption">The status option.</param>
		/// <param name="CommandText">The command text.</param>
		public void QueryStatus(string CmdName, vsCommandStatusTextWanted NeededText, ref vsCommandStatus StatusOption, ref object CommandText)
		{
			if (NeededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
			{
				if (_comparer.Equals(CmdName, _CommandNameFormatted))
				{
					// ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
					StatusOption = vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
					return;
				}
				StatusOption = vsCommandStatus.vsCommandStatusUnsupported;
			}
		}

		#endregion

		#region methods
		/// <summary>
		/// Creates the menu button.
		/// </summary>
		/// <param name="commandName">Name of the command.</param>
		/// <param name="commandImg">The command img.</param>
		/// <param name="description">The description.</param>
		/// <param name="bar">The bar.</param>
		/// <param name="buttonCaption">The button caption.</param>
		/// <param name="hotkey">The hotkey.</param>
		/// <param name="beginGroup">if set to <c>true</c> [begin group].</param>
		/// <returns>CommandBarButton.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		internal CommandBarButton CreateMenuButton(string commandName, int commandImg, string description, CommandBar bar, string buttonCaption, string hotkey, bool beginGroup)
		{
			try
			{
				Command command;
				try
				{
					command = _DTE.Commands.Item(_AddIn.ProgID + "." + commandName, 1);
					if (command != null) { command.Delete(); }
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Exception deleting previous menu item: " + ex.Message, _ProductName);
				}
				object[] array = null;
				command = _DTE.Commands.AddNamedCommand(_AddIn, commandName, description, description, true, commandImg, ref array, 3);
				if (!string.IsNullOrEmpty(hotkey))
				{
					command.Bindings = new object[] { hotkey };
				}
				var commandBarButton = command.AddControl(bar) as CommandBarButton;
				if (commandBarButton != null)
				{
					commandBarButton.Caption = buttonCaption;
					commandBarButton.Visible = true;
					commandBarButton.BeginGroup = beginGroup;
					return commandBarButton;
				}
			}
			catch (Exception ex2)
			{
				MessageBox.Show(Resources.PlanAnalysis_CreateMenuButton_Exception_creating_new_menu_item + ex2.Message, _ProductName);
			}
			return null;
		}

		/// <summary>
		/// Shows the analysis.
		/// </summary>
		/// <param name="Handled">if set to <c>true</c> [handled].</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Handled")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void ShowAnalysis(ref bool Handled)
		{
			//do NOT dispose of these controls. As we are attaching to existing controls. If you do, you will make the execution plan unusable in the ssms ui
			var focus = NativeMethods.GetFocus();
			var control = Control.FromChildHandle(focus);
			var activeWindowCaption = this.ActiveWindowCaption;

			var parentControl = _activeScriptEditorControl == null
				? FindParentControl(control, _showPlanControl)
				: FindControlRecursive(_activeScriptEditorControl, _showPlanControl);

			if (parentControl == null)
			{
				MessageBox.Show(Resources.PlanAnalysis_ShowAnalysis_Could_not_find_the_parent_control, _ProductName);
				return;
			}

			var planXml = GetPlanXml(_showPlanControl, _getShowPlanXml, _dataBindings, _getGraphPanel, parentControl);

			try
			{
				var directory = Path.Combine(Path.GetTempPath(), "PlanAnalysis");
				Directory.CreateDirectory(directory);

				var planValidator = new PlanValidator();
				PlanTransformer.ExtractResources(directory);

				var results = planValidator.ValidateSqlPlan(planXml);
				var planAnalysisXml = planValidator.UpdatePlanWithAnalysis(planXml, results);

				var fileName = Regex.Replace(activeWindowCaption, "[^A-Za-z0-9_-]", "");
				fileName = Path.Combine(directory, fileName + ".analysis.html");

				File.WriteAllText(fileName, PlanTransformer.TransformHtmlPage(activeWindowCaption, planAnalysisXml));

				Process.Start(fileName);
				Handled = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(Resources.PlanAnalysis_ShowAnalysis_Exception_generating_plan_analysis + ex, _ProductName);
			}
		}

		/// <summary>
		/// Gets the plan XML.
		/// </summary>
		/// <param name="showPlanControlType">Type of the show plan control.</param>
		/// <param name="showPlanXmlMethod">The show plan XML method.</param>
		/// <param name="dataBindingsField">The data bindings field.</param>
		/// <param name="getGraphPanelMethod">The get graph panel method.</param>
		/// <param name="showPlanControl">The show plan control.</param>
		/// <returns>System.String.</returns>
		private static string GetPlanXml(Type showPlanControlType, MethodInfo showPlanXmlMethod, FieldInfo dataBindingsField, MethodInfo getGraphPanelMethod, Control showPlanControl)
		{
			string result = null;
			if (showPlanXmlMethod != null)
			{
				result = showPlanXmlMethod.Invoke(showPlanControl, null) as string;
			}
			else if (dataBindingsField != null && getGraphPanelMethod != null)
			{
				var dictionary = dataBindingsField.GetValue(showPlanControl) as IDictionary;
				var obj = getGraphPanelMethod.Invoke(showPlanControl, new object[] { 0 });
				if (dictionary != null && (obj != null && dictionary.Contains(obj)))
				{
					var obj2 = dictionary[obj];
					if (obj2 != null)
					{
						var nestedType = showPlanControlType.GetNestedType("DataBinding", BindingFlags.Instance | BindingFlags.NonPublic);
						if (nestedType != null)
						{
							var field = nestedType.GetField("DataSource", BindingFlags.Instance | BindingFlags.NonPublic);
							if (field != null)
							{
								result = (field.GetValue(obj2) as string);
							}
						}
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Gets the active script editor control.
		/// </summary>
		/// <param name="scriptEditorControlType">Type of the script editor control.</param>
		/// <returns>System.Object.</returns>
		private static object GeScriptEditorControl(Type scriptEditorControlType)
		{
			var assembly = Assembly.Load("Microsoft.SqlServer.SqlTools.VSIntegration");
			var serviceCacheType = assembly.GetType("Microsoft.SqlServer.Management.UI.VSIntegration.ServiceCache");
			var vsMonitorSelectionProperty = serviceCacheType.GetProperty("VSMonitorSelection", BindingFlags.Static | BindingFlags.Public);
			var vsMonitorSelection = vsMonitorSelectionProperty.GetValue(null, null);
			var ivsMonitorSelectionType = assembly.GetType("Microsoft.SqlServer.Management.UI.VSIntegration.IVsMonitorSelection");
			var getCurrentElementValueMethod = ivsMonitorSelectionType.GetMethod("GetCurrentElementValue", BindingFlags.Instance | BindingFlags.Public);
			var array = new object[2];
			array[0] = 2;
			getCurrentElementValueMethod.Invoke(vsMonitorSelection, array);
			var obj = array[1];
			if (obj != null && scriptEditorControlType.IsInstanceOfType(obj))
			{
				return obj;
			}
			return null;
		}

		/// <summary>
		/// Finds the parent control.
		/// </summary>
		/// <param name="control">The control.</param>
		/// <param name="controlType">Type of the control.</param>
		/// <returns>Control.</returns>
		private static Control FindParentControl(Control control, Type controlType)
		{
			var parent = control.Parent;
			while (parent != null && parent.GetType() != controlType)
			{
				parent = parent.Parent;
			}
			return parent;
		}

		/// <summary>
		/// Finds the control recursively.
		/// </summary>
		/// <param name="control">The control.</param>
		/// <param name="controlType">Type of the control.</param>
		/// <returns>Control.</returns>
		private static Control FindControlRecursive(Control control, Type controlType)
		{
			if (control.GetType() == controlType) { return control; }
			foreach (Control subCtl in control.Controls)
			{
				var ctl = FindControlRecursive(subCtl, controlType);
				if (ctl != null) { return ctl; }
			}
			return null;
		}
		#endregion methods
	}
}