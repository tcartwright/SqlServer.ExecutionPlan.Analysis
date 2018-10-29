// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 07-13-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-13-2016
// ***********************************************************************
// <copyright file="CheckForRelOpWarnings.cs" company="Tim Cartwright">
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
	/// Class CheckForRelOpWarnings.
	/// </summary>
	/// <seealso cref="SqlServer.ExecutionPlan.Analysis.Checks.BasePlanIssueCheck" />
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rel")]
	public class CheckForRelOpWarnings : BasePlanIssueCheck
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckForRelOpWarnings"/> class.
		/// </summary>
		public CheckForRelOpWarnings()
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
			var element = xElement.GetElement(el.Warnings);
			if (element == null) { yield break; }

			var noJoin = element.AttributeString(att.NoJoinPredicate, "false");
			if (Comparer.Equals(noJoin, "true") || noJoin == "1")
			{
				yield return new PlanIssueResult(this.StatementArgs.StatementId) { Result = PlanResult.NoJoinPredicate, NodeId = this.RelOpArgs.NodeId, Category = PlanCategory.Warning };
			}
			if (element.Elements(el.ColumnsWithNoStatistics).Any())
			{
				yield return new PlanIssueResult(this.StatementArgs.StatementId) { Result = PlanResult.ColumnsWithNoStatistics, NodeId = this.RelOpArgs.NodeId, Category = PlanCategory.Warning };
			}
			if (element.Elements(el.SpillToTempDb).Any())
			{
				yield return new PlanIssueResult(this.StatementArgs.StatementId) { Result = PlanResult.SpillToTempDb, NodeId = this.RelOpArgs.NodeId, Category = PlanCategory.Warning };
			}
			if (element.Elements(el.UnmatchedIndexes).Any())
			{
				yield return new PlanIssueResult(this.StatementArgs.StatementId) { Result = PlanResult.UnmatchedIndexes, NodeId = this.RelOpArgs.NodeId, Category = PlanCategory.Warning };
			}
		}
	}
}
