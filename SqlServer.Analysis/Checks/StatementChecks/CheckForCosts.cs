// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 07-13-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-13-2016
// ***********************************************************************
// <copyright file="CheckForHighCost.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;
using System.Xml.Linq;
using el = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanElementNames;
using att = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanAttributeNames;

namespace SqlServer.ExecutionPlan.Analysis.Checks.StatementChecks
{
	/// <summary>
	/// Class CheckForHighCost.
	/// </summary>
	/// <seealso cref="SqlServer.ExecutionPlan.Analysis.Checks.BasePlanIssueCheck" />
	public class CheckForCosts : BasePlanIssueCheck
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckForCosts"/> class.
		/// </summary>
		public CheckForCosts()
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
			if (this.StatementArgs.StatementCost >= this.CheckParameters.HighCostOperator)
			{
				yield return new PlanIssueResult(this.StatementArgs.StatementId) { Result = PlanResult.HighCostOperator, ThreshHold = this.CheckParameters.HighCostOperator };
			}

			if (this.StatementArgs.StatementCost >= this.CheckParameters.MediumCostOperator && this.StatementArgs.StatementCost < this.CheckParameters.HighCostOperator)
			{
				yield return new PlanIssueResult(this.StatementArgs.StatementId) { Result = PlanResult.MediumCostOperator, ThreshHold = this.CheckParameters.MediumCostOperator };
			}
		}
	}
}
