// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 07-13-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-13-2016
// ***********************************************************************
// <copyright file="StatementArgs.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Xml.Linq;
using SqlServer.ExecutionPlan.Analysis.Internals;
using att = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanAttributeNames;

namespace SqlServer.ExecutionPlan.Analysis.Checks
{
	//NOTE: WE CANNOT USE LAZY<T> WHEN RUNNING CLR IN SAFE MODE.

	/// <summary>
	/// Class StatementArgs.
	/// </summary>
	public class StatementArgs
	{
		/// <summary>
		/// The _statement
		/// </summary>
		private readonly XElement _statement;

		/// <summary>
		/// The _statement identifier
		/// </summary>
		private readonly int _statementId;
		/// <summary>
		/// The _statement cost
		/// </summary>
		private readonly int _statementCost;
		/// <summary>
		/// The _statement text
		/// </summary>
		private readonly string _statementText;
		/// <summary>
		/// The _statement type
		/// </summary>
		private readonly string _statementType;
		/// <summary>
		/// The _statement row count
		/// </summary>
		private readonly double _statementRowCount;

		/// <summary>
		/// Gets the statement identifier.
		/// </summary>
		/// <value>The statement identifier.</value>
		public int StatementId { get { return _statementId; } }

		/// <summary>
		/// Gets the statement SQL.
		/// </summary>
		/// <value>The statement SQL.</value>
		public string StatementSql { get { return _statementText; } }

		/// <summary>
		/// Gets the statement cost.
		/// </summary>
		/// <value>The statement cost.</value>
		public int StatementCost { get { return _statementCost; } }

		/// <summary>
		/// Gets the type of the statement.
		/// </summary>
		/// <value>The type of the statement.</value>
		public string StatementType { get { return _statementType; } }

		/// <summary>
		/// Gets the statement row count.
		/// </summary>
		/// <value>The statement row count.</value>
		public double StatementRowCount { get { return _statementRowCount; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="StatementArgs"/> class.
		/// </summary>
		/// <param name="statement">The statement.</param>
		public StatementArgs(XElement statement)
		{
			_statement = statement;

			_statementId = _statement.AttributeInt32(att.StatementId, -1);
			_statementCost = _statement.AttributeInt32(att.StatementSubTreeCost, -1);
			_statementText = _statement.AttributeString(att.StatementText, "");
			_statementType = _statement.AttributeString(att.StatementType, "");
			_statementRowCount = _statement.AttributeDouble(att.StatementEstRows, 0);
		}
	}
}
