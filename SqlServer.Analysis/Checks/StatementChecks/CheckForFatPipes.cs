// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 07-13-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-13-2016
// ***********************************************************************
// <copyright file="CheckForFatPipes.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SqlServer.ExecutionPlan.Analysis.Internals;
using el = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanElementNames;
using att = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanAttributeNames;

namespace SqlServer.ExecutionPlan.Analysis.Checks.StatementChecks
{
	/// <summary>
	/// Class CheckForFatPipes.
	/// </summary>
	/// <seealso cref="SqlServer.ExecutionPlan.Analysis.Checks.BasePlanIssueCheck" />
	public class CheckForFatPipes : BasePlanIssueCheck
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckForFatPipes"/> class.
		/// </summary>
		public CheckForFatPipes()
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
			var rootRowCount = this.StatementArgs.StatementRowCount;
			var statmentType = this.StatementArgs.StatementType;

			if (!(Comparer.Equals(statmentType, "SELECT") 
				|| Comparer.Equals(statmentType, "UPDATE") 
				|| Comparer.Equals(statmentType, "INSERT") 
				|| Comparer.Equals(statmentType, "DELETE")))
			{
				yield break;
			}

			var exist = xElement.Descendants(el.RelOp).Any(ro => this.CheckPipeFatness(rootRowCount, ro));
			if (exist)
			{
				yield return new PlanIssueResult(this.StatementArgs.StatementId) { Result = PlanResult.FatPipes, ThreshHold = this.CheckParameters.FatPipeRowCount };
			}
		}

		/// <summary>
		/// Checks the pipe fatness.
		/// </summary>
		/// <param name="rootRowCount">The root row count.</param>
		/// <param name="relOp">The relative op.</param>
		/// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
		private bool CheckPipeFatness(double rootRowCount, XElement relOp)
		{
			var estimateRows = relOp.AttributeDouble(att.EstimateRows, 1);
			var estimateRebinds = relOp.AttributeDouble(att.EstimateRebinds, 1);
			var rowCount = Math.Max(estimateRows, 1) * Math.Max(estimateRebinds, 1);

			//we are looking for pipes where the rowcount greatly exceeds the root row count
			var checkCount = rowCount - rootRowCount;

			return checkCount >= this.CheckParameters.FatPipeRowCount;
		}
	}
}
