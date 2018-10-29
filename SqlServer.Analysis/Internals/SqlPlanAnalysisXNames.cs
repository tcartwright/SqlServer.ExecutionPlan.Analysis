// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 07-13-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-13-2016
// ***********************************************************************
// <copyright file="SqlPlanAnalysisXNames.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Xml.Linq;

namespace SqlServer.ExecutionPlan.Analysis.Internals
{
	/// <summary>
	/// Class SqlPlanAnalysisXNames.
	/// </summary>
	internal static class SqlPlanAnalysisXNames
	{
		/// <summary>
		/// The XMLNS
		/// </summary>
		public static readonly XNamespace Xmlns = XNamespace.Get("http://Tim Cartwright.com/plananalysis");
		/// <summary>
		/// The result
		/// </summary>
		public static readonly XName Result = Xmlns.GetName("Result");
		/// <summary>
		/// The warnings
		/// </summary>
		public static readonly XName Warnings = Xmlns.GetName("Warnings");
		/// <summary>
		/// The errors
		/// </summary>
		public static readonly XName Errors = Xmlns.GetName("Errors");
		/// <summary>
		/// The warning
		/// </summary>
		public static readonly XName Warning = Xmlns.GetName("Warning");
		/// <summary>
		/// The error
		/// </summary>
		public static readonly XName Error = Xmlns.GetName("Error");

	}
}
