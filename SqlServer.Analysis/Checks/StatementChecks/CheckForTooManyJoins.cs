// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 07-13-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-13-2016
// ***********************************************************************
// <copyright file="CheckForTooManyJoins.cs" company="Tim Cartwright">
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
	/// Class CheckForTooManyJoins.
	/// </summary>
	/// <seealso cref="SqlServer.ExecutionPlan.Analysis.Checks.BasePlanIssueCheck" />
	public class CheckForTooManyJoins : BasePlanIssueCheck
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckForTooManyJoins"/> class.
		/// </summary>
		public CheckForTooManyJoins()
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
			var joinOps = new[] { "Nested Loops", "Merge Join", "Hash Match" };
			var joins = xElement.Descendants(el.RelOp).Where(op => joinOps.Contains(op.AttributeString(att.PhysicalOp, ""))).ToList();
			
			if (joins.Count() <= this.CheckParameters.HighJoinCount)
			{
				yield break;
			}

			var highJoinIssue = new PlanIssueResult(this.StatementArgs.StatementId) { Result = PlanResult.HighNumberOfJoins, Category = PlanCategory.Warning };
			if (joins.Count() > this.CheckParameters.VeryHighJoinCount) { highJoinIssue.Category = PlanCategory.Error; }
			yield return highJoinIssue;
		}
	}
}
