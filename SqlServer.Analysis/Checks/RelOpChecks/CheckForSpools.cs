// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 07-13-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-13-2016
// ***********************************************************************
// <copyright file="CheckForSpools.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;
using System.Xml.Linq;
using el = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanElementNames;
using att = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanAttributeNames;

namespace SqlServer.ExecutionPlan.Analysis.Checks.RelOpChecks
{
	/// <summary>
	/// Class CheckForSpools.
	/// </summary>
	/// <seealso cref="SqlServer.ExecutionPlan.Analysis.Checks.BasePlanIssueCheck" />
	public class CheckForSpools : BasePlanIssueCheck
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckForSpools"/> class.
		/// </summary>
		public CheckForSpools()
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
			if (!(Comparer.Equals(this.RelOpArgs.PhysicalOp, "index spool") || Comparer.Equals(this.RelOpArgs.PhysicalOp, "table spool"))) { yield break; }

			if (Comparer.Equals(this.RelOpArgs.PhysicalOp, "index spool"))
			{
				yield return new PlanIssueResult(this.StatementArgs.StatementId) { Result = PlanResult.IndexSpool, NodeId = this.RelOpArgs.NodeId };
			}
			else if (Comparer.Equals(this.RelOpArgs.PhysicalOp, "table spool"))
			{
				yield return new PlanIssueResult(this.StatementArgs.StatementId) { Result = PlanResult.TableSpool, NodeId = this.RelOpArgs.NodeId };
			}
		}
	}
}
