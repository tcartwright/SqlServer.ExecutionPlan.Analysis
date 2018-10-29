// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 07-13-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-13-2016
// ***********************************************************************
// <copyright file="CheckForNolock.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using el = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanElementNames;

namespace SqlServer.ExecutionPlan.Analysis.Checks.StatementChecks
{
	/// <summary>
	/// Class CheckForNoLock.
	/// </summary>
	/// <seealso cref="SqlServer.ExecutionPlan.Analysis.Checks.BasePlanIssueCheck" />
	public class CheckForNoLock : BasePlanIssueCheck
	{
		/// <summary>
		/// Class RelOpArgs.
		/// </summary>

		public CheckForNoLock()
			: base(el.StmtSimple, el.StmtCursor)
		{
		}
		/// <summary>
		/// The _physical op
		/// </summary>
		/// <param name="xElement">The x element.</param>
		/// <returns>IEnumerable&lt;IPlanIssueResult&gt;.</returns>
		protected override IEnumerable<IPlanIssueResult> GetIssues(XElement xElement)
		{
			var statementSql = this.StatementArgs.StatementSql;

			if (!string.IsNullOrWhiteSpace(statementSql) && Regex.IsMatch(statementSql, "\\bnolock\\b", RegexOptions.IgnoreCase | RegexOptions.Multiline))
			{
				yield return new PlanIssueResult(this.StatementArgs.StatementId) { Result = PlanResult.NoLock, Category = PlanCategory.Warning };
			}
		}
	}
}
