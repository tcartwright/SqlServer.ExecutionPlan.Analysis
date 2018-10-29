// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 06-17-2016
//
// Last Modified By : tdcart
// Last Modified On : 06-17-2016
// ***********************************************************************
// <copyright file="IPlanIssueResult.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
namespace SqlServer.ExecutionPlan.Analysis
{
	/// <summary>
	/// Interface IPlanIssueResult
	/// </summary>
	public interface IPlanIssueResult
	{
		/// <summary>
		/// Gets the category.
		/// </summary>
		/// <value>The category.</value>
		PlanCategory Category { get; }
		/// <summary>
		/// Gets the result.
		/// </summary>
		/// <value>The result.</value>
		PlanResult Result { get; }
		/// <summary>
		/// Gets the cost.
		/// </summary>
		/// <value>The cost.</value>
		double Cost { get; }
		/// <summary>
		/// Gets the SQL.
		/// </summary>
		/// <value>The SQL.</value>
		string Sql { get; }
		/// <summary>
		/// Gets the query hash.
		/// </summary>
		/// <value>The query hash.</value>
		string QueryHash { get; }
		/// <summary>
		/// Gets the query plan hash.
		/// </summary>
		/// <value>The query plan hash.</value>
		string QueryPlanHash { get; }
		/// <summary>
		/// Gets the thresh hold.
		/// </summary>
		/// <value>The thresh hold.</value>
		Int64? ThreshHold { get; }
		/// <summary>
		/// Gets the statement identifier.
		/// </summary>
		/// <value>The statement identifier.</value>
		int StatementId { get; }
		/// <summary>
		/// Gets the node identifier.
		/// </summary>
		/// <value>The node identifier.</value>
		int? NodeId { get; }
		/// <summary>
		/// Gets the name of the object.
		/// </summary>
		/// <value>The name of the object.</value>
		string ObjectName { get; }
		/// <summary>
		/// Gets the row count.
		/// </summary>
		/// <value>The row count.</value>
		Int64? RowCount { get; set; }
	}
}
