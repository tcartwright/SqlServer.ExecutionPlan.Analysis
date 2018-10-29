// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 07-13-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-13-2016
// ***********************************************************************
// <copyright file="CheckForHighDesiredMemory.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;
using System.Xml.Linq;
using SqlServer.ExecutionPlan.Analysis.Internals;
using el = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanElementNames;
using att = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanAttributeNames;

namespace SqlServer.ExecutionPlan.Analysis.Checks.StatementChecks
{
	/// <summary>
	/// Class CheckForHighDesiredMemory.
	/// </summary>
	/// <seealso cref="SqlServer.ExecutionPlan.Analysis.Checks.BasePlanIssueCheck" />
	public class CheckForHighDesiredMemory : BasePlanIssueCheck
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckForHighDesiredMemory"/> class.
		/// </summary>
		public CheckForHighDesiredMemory()
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
			var memInfo = xElement.GetElement(el.QueryPlan, el.MemoryGrantInfo);
			if (memInfo == null) { yield break;}

			var memory = memInfo.AttributeInt32(att.SerialDesiredMemory, -1);

			if (memory > this.CheckParameters.HighDesiredMemoryKb)
			{
				yield return new PlanIssueResult(this.StatementArgs.StatementId) { Result = PlanResult.HighDesiredMemoryKB, Category = PlanCategory.Warning, ThreshHold = this.CheckParameters.HighDesiredMemoryKb };
			}
		}
	}
}
