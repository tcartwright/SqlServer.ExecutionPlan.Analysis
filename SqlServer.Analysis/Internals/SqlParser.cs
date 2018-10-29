// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 07-13-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-13-2016
// ***********************************************************************
// <copyright file="SqlParser.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************
#if !CLR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.ExecutionPlan.Analysis.Internals
{
	/// <summary>
	/// Class SqlParser.
	/// </summary>
	internal static class SqlParser
	{
		/// <summary>
		/// Determines whether the specified SQL is parameterized.
		/// </summary>
		/// <param name="sql">The SQL.</param>
		/// <returns><c>true</c> if the specified SQL is parameterized; otherwise, <c>false</c>.</returns>
		public static bool IsParameterized(string sql)
		{
			var ret = false;
			using (var reader = new StringReader(sql))
			{
				IList<ParseError> errors;
				var parser = new TSql100Parser(true);
				var script = parser.Parse(reader, out errors) as TSqlScript;

				if (script != null)
				{
					foreach (var batch in script.Batches)
					{
						foreach (var statement in batch.Statements)
						{
							if (statement is SelectStatement)
							{
								var selectStatement = (SelectStatement)statement;
								if (selectStatement.QueryExpression is QuerySpecification)
								{
									ret = HasLiterals(((QuerySpecification)selectStatement.QueryExpression).WhereClause);
								}
								else if (selectStatement.QueryExpression is BinaryQueryExpression) //union
								{
									ret = CheckBinaryQuery((BinaryQueryExpression)selectStatement.QueryExpression);
								}
							}
							else if (statement is DeleteStatement)
							{
								ret = HasLiterals(((DeleteStatement)statement).DeleteSpecification.WhereClause);
							}
							else if (statement is UpdateStatement)
							{
								ret = HasLiterals(((UpdateStatement)statement).UpdateSpecification.WhereClause);
							}
							else if (statement is InsertStatement)
							{
								var insertStatement = statement as InsertStatement;
								if (insertStatement.InsertSpecification.InsertSource is SelectInsertSource)
								{
									var selectInsertSource = (SelectInsertSource)insertStatement.InsertSpecification.InsertSource;
									if (selectInsertSource.Select is QuerySpecification)
									{
										ret = HasLiterals(((QuerySpecification)selectInsertSource.Select).WhereClause);
									}
									else if (selectInsertSource.Select is BinaryQueryExpression)
									{
										ret = CheckBinaryQuery((BinaryQueryExpression)selectInsertSource.Select);
									}
								}
								else
								{
									ret = false;
								}
							}

							if (ret) { break; }
							//Console.WriteLine(statement.ToString());
						}
					}
				}
			}

			return !ret;
		}

		/// <summary>
		/// Checks the binary query.
		/// </summary>
		/// <param name="expression">The expression.</param>
		/// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
		private static bool CheckBinaryQuery(QueryExpression expression)
		{
			var ret = false;
			if (expression is BinaryQueryExpression)
			{
				var binaryExpression = (BinaryQueryExpression)expression;
				if (binaryExpression.FirstQueryExpression is BinaryQueryExpression)
				{
					ret |= CheckBinaryQuery(binaryExpression.FirstQueryExpression);
				}
				else
				{
					ret |= HasLiterals(((QuerySpecification)binaryExpression.FirstQueryExpression).WhereClause);
				}

				if (binaryExpression.SecondQueryExpression is BinaryQueryExpression)
				{
					ret |= !ret && CheckBinaryQuery(binaryExpression.SecondQueryExpression);
				}
				else
				{
					ret |= !ret && HasLiterals(((QuerySpecification)binaryExpression.SecondQueryExpression).WhereClause);
				}
			}
			else
			{
				ret |= HasLiterals(((QuerySpecification)expression).WhereClause);
			}

			return ret;
		}

		/// <summary>
		/// Determines whether the specified where clause has literals.
		/// </summary>
		/// <param name="whereClause">The where clause.</param>
		/// <returns><c>true</c> if the specified where clause has literals; otherwise, <c>false</c>.</returns>
		private static bool HasLiterals(WhereClause whereClause)
		{
			return whereClause != null && HasLiterals(whereClause.SearchCondition);
		}

		/// <summary>
		/// Determines whether the specified expression has literals.
		/// </summary>
		/// <param name="expression">The expression.</param>
		/// <returns><c>true</c> if the specified expression has literals; otherwise, <c>false</c>.</returns>
		private static bool HasLiterals(BooleanExpression expression)
		{
			var ret = false;

			if (expression is BooleanBinaryExpression)
			{
				var boolBinary = (BooleanBinaryExpression)expression;
				if (boolBinary.FirstExpression is BooleanBinaryExpression)
				{
					ret |= HasLiterals(boolBinary.FirstExpression);
				}
				if (!ret && boolBinary.SecondExpression is BooleanBinaryExpression)
				{
					ret |= HasLiterals(boolBinary.SecondExpression);
				}

				ret |= !ret && CheckExpression(boolBinary.FirstExpression);
				ret |= !ret && CheckExpression(boolBinary.SecondExpression);
			}
			else
			{
				ret |= CheckExpression(expression);
			}
			return ret;
		}

		/// <summary>
		/// Checks the expression.
		/// </summary>
		/// <param name="expression">The expression.</param>
		/// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
		private static bool CheckExpression(BooleanExpression expression)
		{
			bool ret;

			if (expression is BooleanBinaryExpression)
			{
				ret = HasLiterals(expression);
			}
			else if (expression is BooleanComparisonExpression || expression is LikePredicate)
			{
				ret = CheckComparisonForLiterals(expression);
			}
			else if (expression is InPredicate)
			{
				var inPredicate = expression as InPredicate;
				ret = inPredicate.Values.Any(v => v is Literal);
			}
			else if (expression is BooleanParenthesisExpression)
			{
				var parenExpression = expression as BooleanParenthesisExpression;
				ret = HasLiterals(parenExpression.Expression);
			}
			else
			{
				ret = false;
			}
			return ret;
		}

		/// <summary>
		/// Checks the comparison for literals.
		/// </summary>
		/// <param name="expression">The expression.</param>
		/// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
		/// <exception cref="System.NotSupportedException"></exception>
		private static bool CheckComparisonForLiterals(BooleanExpression expression)
		{
			if (expression == null) { return false; }

			if (expression is BooleanComparisonExpression)
			{
				var compareExpression = (BooleanComparisonExpression)expression;
				return compareExpression.FirstExpression is Literal || compareExpression.SecondExpression is Literal;
			}
			else if (expression is LikePredicate)
			{
				var likePredicate = (LikePredicate)expression;
				return likePredicate.FirstExpression is Literal || likePredicate.SecondExpression is Literal;
			}
			else
			{
				throw new NotSupportedException();
			}
		}
	}
}
#endif