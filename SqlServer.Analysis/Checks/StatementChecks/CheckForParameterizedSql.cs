// ReSharper disable RedundantUsingDirective
// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 07-13-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-13-2016
// ***********************************************************************
// <copyright file="CheckForParameterizedSql.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SqlServer.ExecutionPlan.Analysis.Internals;
using att = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanAttributeNames;
using el = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanElementNames;

namespace SqlServer.ExecutionPlan.Analysis.Checks.StatementChecks
{
	/// <summary>
	/// Class CheckForParameterizedSql.
	/// </summary>
	/// <seealso cref="SqlServer.ExecutionPlan.Analysis.Checks.BasePlanIssueCheck" />
	public class CheckForParameterizedSql : BasePlanIssueCheck
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckForParameterizedSql"/> class.
		/// </summary>
		public CheckForParameterizedSql()
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
			var statmentType = this.StatementArgs.StatementType;

			if (!(Comparer.Equals(statmentType, "SELECT") || Comparer.Equals(statmentType, "UPDATE")  
				|| Comparer.Equals(statmentType, "INSERT") || Comparer.Equals(statmentType, "DELETE")))
			{
				yield break;
			}

			var statementSql = this.StatementArgs.StatementSql;

			/*the clr CAN NOT use the Microsoft.SqlServer.TransactSql.ScriptDom reference as it has a public static var that is not readonly. 
				 * So when we compile / push the CLR we need to not compile the code that uses it. If you try to push that dll as a reference this is the error you get:
				 * 
				 * Creating [Microsoft.SqlServer.TransactSql.ScriptDom]...
				 *	Warning: The SQL Server client assembly 'microsoft.sqlserver.transactsql.scriptdom, version=12.0.0.0, culture=neutral, publickeytoken=89845dcd8080cc91, processorarchitecture=msil.' you are registering is not fully tested in SQL Server hosted environment.
				 *	(47,1): SQL72014: .Net SqlClient Data Provider: Msg 6211, Level 16, State 1, Line 1 CREATE ASSEMBLY failed because type 'antlr.Token' in safe assembly 'Microsoft.SqlServer.TransactSql.ScriptDom' has a static field 'badToken'. Attributes of static fields in safe assemblies must be marked  readonly in Visual C#, ReadOnly in Visual Basic, or initonly in Visual C++ and intermediate language.
				 *	(47,0): SQL72045: Script execution error.  The executed script:
				 */
#if CLR
			var whereIndex = statementSql.IndexOf("WHERE", StringComparison.InvariantCultureIgnoreCase);
			if (whereIndex > 0)
			{
				var atIndex = statementSql.IndexOf("@", whereIndex, StringComparison.Ordinal);
				if (atIndex <= 0)
				{
					yield return new PlanIssueResult(this.StatementArgs.StatementId) { Result = PlanResult.PossibleNonparameterizedQuery, Category = PlanCategory.Warning };
				}
			}
#else
			var isParameterized = SqlParser.IsParameterized(statementSql);
			if (!isParameterized)
			{
				yield return new PlanIssueResult(this.StatementArgs.StatementId) { Result = PlanResult.PossibleNonparameterizedQuery, Category = PlanCategory.Warning };
			}
#endif
		}
	}
}
