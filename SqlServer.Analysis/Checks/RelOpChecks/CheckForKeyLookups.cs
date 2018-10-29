// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 07-13-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-13-2016
// ***********************************************************************
// <copyright file="CheckForKeyLookups.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Collections.Generic;
using System.Xml.Linq;
using SqlServer.ExecutionPlan.Analysis.Internals;
using att = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanAttributeNames;
using el = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanElementNames;

namespace SqlServer.ExecutionPlan.Analysis.Checks.RelOpChecks
{
	/// <summary>
	/// Class CheckForKeyLookups.
	/// </summary>
	/// <seealso cref="SqlServer.ExecutionPlan.Analysis.Checks.BasePlanIssueCheck" />
	public class CheckForKeyLookups : BasePlanIssueCheck
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckForKeyLookups"/> class.
		/// </summary>
		public CheckForKeyLookups()
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
			var element = xElement.GetElement(el.IndexScan);
			if (element == null) { yield break; }
			var lookup = element.AttributeString(att.Lookup, "");
			if (!(Comparer.Equals(lookup, "true") || lookup == "1")) { yield break; }

			var keyLookup = new PlanIssueResult(this.StatementArgs.StatementId) { Result = PlanResult.KeyLookup, NodeId = this.RelOpArgs.NodeId, Category = PlanCategory.Warning };
			if (!this.RelOpArgs.PhysicalOp.ToLower().Contains("index seek"))
			{
				keyLookup.Result = PlanResult.RIDLookup;
			}
			if (this.RelOpArgs.GetOperatorCostPercent(this.StatementArgs.StatementCost) > 5)
			{
				keyLookup.Category = PlanCategory.Error;
			}
			yield return keyLookup;
		}
	}
}
