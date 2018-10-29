// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 06-17-2016
//
// Last Modified By : tdcart
// Last Modified On : 06-17-2016
// ***********************************************************************
// <copyright file="PlanIssueResult.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;

namespace  SqlServer.ExecutionPlan.Analysis
{
	/// <summary>
	/// Class PlanIssueResult.
	/// </summary>
	/// <seealso cref="SqlServer.ExecutionPlan.Analysis.IPlanIssueResult" />
	public class PlanIssueResult : IPlanIssueResult
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PlanIssueResult"/> class.
		/// </summary>
		/// <param name="statementId">The statement identifier.</param>
		public PlanIssueResult(int statementId)
		{
			this.StatementId = statementId;
			this.Category = PlanCategory.Error;
		}

		/// <summary>
		/// Gets the category.
		/// </summary>
		/// <value>The category.</value>
		public PlanCategory Category { get; internal set; }
		/// <summary>
		/// Gets the result.
		/// </summary>
		/// <value>The result.</value>
		public PlanResult Result { get; internal set; }
		/// <summary>
		/// Gets the cost.
		/// </summary>
		/// <value>The cost.</value>
		public double Cost { get; internal set; }
		/// <summary>
		/// Gets the SQL.
		/// </summary>
		/// <value>The SQL.</value>
		public string Sql { get; internal set; }
		/// <summary>
		/// Gets the query hash.
		/// </summary>
		/// <value>The query hash.</value>
		public string QueryHash { get; internal set; }
		/// <summary>
		/// Gets the query plan hash.
		/// </summary>
		/// <value>The query plan hash.</value>
		public string QueryPlanHash { get; internal set; }
		/// <summary>
		/// Gets the thresh hold.
		/// </summary>
		/// <value>The thresh hold.</value>
		public Int64? ThreshHold { get; internal set; }
		/// <summary>
		/// Gets the statement identifier.
		/// </summary>
		/// <value>The statement identifier.</value>
		public int StatementId { get; internal set; }
		/// <summary>
		/// Gets the node identifier.
		/// </summary>
		/// <value>The node identifier.</value>
		public int? NodeId { get; internal set; }

		/// <summary>
		/// Gets the name of the object.
		/// </summary>
		/// <value>The name of the object.</value>
		public string ObjectName { get; internal set; }
		/// <summary>
		/// Gets the row count.
		/// </summary>
		/// <value>The row count.</value>
		public Int64? RowCount { get; set; }

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>A <see cref="System.String" /> that represents this instance.</returns>
		public override string ToString()
		{
			if (this.NodeId.HasValue)
			{
				return string.Format("StatementId:{0}, NodeId:{1}, Result:{2}", this.StatementId, this.NodeId.GetValueOrDefault(-1), this.Result);
			}
			else
			{
				return string.Format("StatementId:{0}, Result:{1}", this.StatementId, this.Result);
			}
		}
	}
}
