// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 07-13-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-13-2016
// ***********************************************************************
// <copyright file="CheckForHighCostSort.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using el = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanElementNames;
using att = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanAttributeNames;

namespace SqlServer.ExecutionPlan.Analysis.Checks.RelOpChecks
{
	/// <summary>
	/// Class CheckForHighCostSort.
	/// </summary>
	/// <seealso cref="SqlServer.ExecutionPlan.Analysis.Checks.BasePlanIssueCheck" />
	public class CheckForHighCostSort : BasePlanIssueCheck
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckForHighCostSort"/> class.
		/// </summary>
		public CheckForHighCostSort()
			: base(el.RelOp)
		{
		}

		/// <summary>
		/// Gets the issues.
		/// </summary>
		/// <param name="xElement">The x element.</param>
		/// <returns>IEnumerable&lt;IPlanIssueResult&gt;.</returns>
		protected override IEnumerable<IPlanIssueResult> GetIssues(XElement xElement)
		{
			if (!Comparer.Equals(this.RelOpArgs.PhysicalOp, "sort")) { yield break; }

			var costPercent = this.RelOpArgs.GetOperatorCostPercent(this.StatementArgs.StatementCost);
			var rowCount = this.RelOpArgs.RowCount;

			if (costPercent >= this.CheckParameters.HighSortCost || this.RelOpArgs.RowCount >= this.CheckParameters.HighSortCount)
			{
				yield return new PlanIssueResult(this.StatementArgs.StatementId) { Result = PlanResult.HighCostSort, NodeId = this.RelOpArgs.NodeId, RowCount = Convert.ToInt64(rowCount) };
			}
		}
	}
}
