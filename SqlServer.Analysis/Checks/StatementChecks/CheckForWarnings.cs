// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 07-13-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-13-2016
// ***********************************************************************
// <copyright file="CheckForWarnings.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SqlServer.ExecutionPlan.Analysis.Internals;
using el = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanElementNames;
using att = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanAttributeNames;

namespace SqlServer.ExecutionPlan.Analysis.Checks.StatementChecks
{
	/// <summary>
	/// Class CheckForWarnings.
	/// </summary>
	/// <seealso cref="SqlServer.ExecutionPlan.Analysis.Checks.BasePlanIssueCheck" />
	public class CheckForWarnings : BasePlanIssueCheck
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckForWarnings"/> class.
		/// </summary>
		public CheckForWarnings()
			: base(el.StmtSimple)
		{
		}

		/// <summary>
		/// Gets the issues.
		/// </summary>
		/// <param name="xElement">The x element.</param>
		/// <returns>IEnumerable&lt;IPlanIssueResult&gt;.</returns>
		protected override IEnumerable<IPlanIssueResult> GetIssues(XElement xElement)
		{
			var statmentWarnings = xElement.GetElement(el.QueryPlan, el.Warnings);
			if (statmentWarnings == null) { yield break; }

			var convertElements = statmentWarnings.Elements(el.PlanAffectingConvert).ToList();
			if (!convertElements.Any()) { yield break; }
			
			var implicitCount = convertElements.Count(e => e.AttributeString(att.Expression, "").ToUpper().Contains("CONVERT_IMPLICIT"));
			if (implicitCount > 0)
			{
				yield return new PlanIssueResult(this.StatementArgs.StatementId) {Result = PlanResult.ImplicitConversion, Category = PlanCategory.Error};
			}
			if (implicitCount != convertElements.Count())
			{
				yield return new PlanIssueResult(this.StatementArgs.StatementId) {Result = PlanResult.CardinalityConvertIssue, Category = PlanCategory.Error};
			}
		}
	}
}
