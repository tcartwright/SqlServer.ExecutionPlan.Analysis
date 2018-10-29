// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 07-13-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-13-2016
// ***********************************************************************
// <copyright file="CheckForScans.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using SqlServer.ExecutionPlan.Analysis.Internals;
using el = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanElementNames;
using att = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanAttributeNames;

namespace SqlServer.ExecutionPlan.Analysis.Checks.RelOpChecks
{
	/// <summary>
	/// Class CheckForScans.
	/// </summary>
	/// <seealso cref="SqlServer.ExecutionPlan.Analysis.Checks.BasePlanIssueCheck" />
	public class CheckForScans : BasePlanIssueCheck
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckForScans"/> class.
		/// </summary>
		public CheckForScans()
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
			if (!(Comparer.Equals(this.RelOpArgs.PhysicalOp, "clustered index scan") || Comparer.Equals(this.RelOpArgs.PhysicalOp, "index scan"))) { yield break; }
			if (!(this.RelOpArgs.EstimateRows >= this.CheckParameters.HighScanCount)) { yield break; }

			
			var dbobj = xElement.GetElement(el.IndexScan, el.Object);
			var dbobjName = dbobj.AttributeString(att.Index, null);
			var pi = new PlanIssueResult(this.StatementArgs.StatementId)
			{
				Result = PlanResult.IndexScan, 
				Category = PlanCategory.Error, 
				NodeId = this.RelOpArgs.NodeId, 
				RowCount = Convert.ToInt64(this.RelOpArgs.EstimateRows)
			};

			if (Comparer.Equals(this.RelOpArgs.PhysicalOp, "clustered index scan"))
			{
				pi.Result = PlanResult.ClusteredIndexScan;
			}

			if (!string.IsNullOrWhiteSpace(dbobjName)) { pi.ObjectName = dbobjName.Replace("[", "").Replace("]", ""); }
			yield return pi;
		}
	}
}
