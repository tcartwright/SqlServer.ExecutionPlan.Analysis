// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 07-13-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-13-2016
// ***********************************************************************
// <copyright file="CheckForTableFunctionJoins.cs" company="Tim Cartwright">
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

namespace SqlServer.ExecutionPlan.Analysis.Checks.RelOpChecks
{
	/// <summary>
	/// Class CheckForTableFunctionJoins.
	/// </summary>
	/// <seealso cref="SqlServer.ExecutionPlan.Analysis.Checks.BasePlanIssueCheck" />
	public class CheckForTableFunctionJoins : BasePlanIssueCheck
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckForTableFunctionJoins"/> class.
		/// </summary>
		public CheckForTableFunctionJoins()
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
			if (this.RelOpArgs.LogicalOp.ToLower().Contains("join"))
			{
				var childRelop = xElement.Elements().FirstOrDefault(e => e.Elements(el.RelOp).FirstOrDefault(r => Comparer.Equals(r.AttributeString(att.LogicalOp, ""), "Table-valued function")) != null);
				if (childRelop != null)
				{
					yield return new PlanIssueResult(this.StatementArgs.StatementId) { Result = PlanResult.JoinToTableValueFunction, NodeId = this.RelOpArgs.NodeId, Category = PlanCategory.Warning };
				}
			}
		}
	}
}
