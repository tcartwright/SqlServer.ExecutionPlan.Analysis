// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 07-13-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-13-2016
// ***********************************************************************
// <copyright file="CheckForConvertImplicits.cs" company="Tim Cartwright">
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
	/// Class CheckForConvertImplicits.
	/// </summary>
	/// <seealso cref="SqlServer.ExecutionPlan.Analysis.Checks.BasePlanIssueCheck" />
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Implicits")]
	public class CheckForConvertImplicits : BasePlanIssueCheck
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckForConvertImplicits"/> class.
		/// </summary>
		public CheckForConvertImplicits()
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
			var scalarOperators = xElement.Descendants(el.ScalarOperator).Where(so => so.AttributeString(att.ScalarString, "").ToUpper().Contains("CONVERT_IMPLICIT")).ToList();
			if (!scalarOperators.Any()) { yield break; }

			Dictionary<int, IPlanIssueResult> results = new Dictionary<int, IPlanIssueResult>();

			foreach (var scalarOperator in scalarOperators)
			{
				var parentRelOp = scalarOperator.Ancestors(el.RelOp).FirstOrDefault();
				if (parentRelOp != null)
				{
					var parentNodeId = parentRelOp.AttributeInt32(att.NodeId, 0);
					//try to ignore 
					var nextNode = parentRelOp.Descendants(el.RelOp).FirstOrDefault(r => r.AttributeInt32(att.NodeId, 0) == parentNodeId + 1);
					var nextNodeLogicalOp = nextNode.AttributeString(att.LogicalOp, "");
					//sorts and aggregates have converts and nothing can be done about them
					if (!(Comparer.Equals(parentRelOp.AttributeString(att.LogicalOp, ""), "Aggregate") 
						|| Comparer.Equals(nextNodeLogicalOp, "Aggregate") 
						|| nextNodeLogicalOp.ToUpper().Contains("SORT")))
					{
						if (!results.ContainsKey(parentNodeId))
						{
							results.Add(parentNodeId, new PlanIssueResult(this.StatementArgs.StatementId) { Result = PlanResult.ImplicitConversion, NodeId = parentNodeId });
						}
					}
				}
			}

			foreach (var result in results)
			{
				yield return result.Value;
			}
		}
	}
}
