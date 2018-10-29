// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 07-13-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-13-2016
// ***********************************************************************
// <copyright file="CheckForCompileIssues.cs" company="Tim Cartwright">
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
	/// Class CheckForCompileIssues.
	/// </summary>
	/// <seealso cref="SqlServer.ExecutionPlan.Analysis.Checks.BasePlanIssueCheck" />
	public class CheckForCompileIssues : BasePlanIssueCheck
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckForCompileIssues"/> class.
		/// </summary>
		public CheckForCompileIssues()
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
			var compileReason = xElement.AttributeString(att.StatementOptmEarlyAbortReason, "");
			if (Comparer.Equals(compileReason, "TimeOut"))
			{
				yield return new PlanIssueResult(this.StatementArgs.StatementId) { Result = PlanResult.CompilationTimeout, Category = PlanCategory.Warning };
			}
			else if (Comparer.Equals(compileReason, "MemoryLimitExceeded"))
			{
				yield return new PlanIssueResult(this.StatementArgs.StatementId) { Result = PlanResult.CompilationMemoryLimitExceeded, Category = PlanCategory.Warning };
			}
		}
	}
}
