// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 07-13-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-13-2016
// ***********************************************************************
// <copyright file="CheckForCursor.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;
using System.Xml.Linq;
using el = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanElementNames;
using att = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanAttributeNames;

namespace SqlServer.ExecutionPlan.Analysis.Checks.StatementChecks
{
	/// <summary>
	/// Class CheckForCursor.
	/// </summary>
	/// <seealso cref="SqlServer.ExecutionPlan.Analysis.Checks.BasePlanIssueCheck" />
	public class CheckForCursor : BasePlanIssueCheck
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckForCursor"/> class.
		/// </summary>
		public CheckForCursor()
			: base(el.StmtCursor)
		{
		}

		/// <summary>
		/// Gets the issues.
		/// </summary>
		/// <param name="xElement">The x element.</param>
		/// <returns>IEnumerable&lt;IPlanIssueResult&gt;.</returns>
		protected override IEnumerable<IPlanIssueResult> GetIssues(XElement xElement)
		{
			if (xElement.Name == el.StmtCursor && Comparer.Equals(this.StatementArgs.StatementType, "DECLARE CURSOR"))
			{
				yield return new PlanIssueResult(this.StatementArgs.StatementId) { Result = PlanResult.Cursor, Category = PlanCategory.Error };
			}
		}
	}
}
