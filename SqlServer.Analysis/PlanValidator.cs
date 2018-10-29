// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 06-17-2016
//
// Last Modified By : tdcart
// Last Modified On : 06-17-2016
// ***********************************************************************
// <copyright file="PlanValidator.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SqlServer.ExecutionPlan.Analysis.Checks;
using SqlServer.ExecutionPlan.Analysis.Internals;
using att = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanAttributeNames;
using el = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanElementNames;
using sspaEl = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanAnalysisXNames;

namespace SqlServer.ExecutionPlan.Analysis
{
	/// <summary>
	/// Class PlanValidator.
	/// </summary>
	public class PlanValidator //: IPlanValidator
	{
		/// <summary>
		/// Gets the check parameters.
		/// </summary>
		/// <value>The check parameters.</value>
		public CheckParameters CheckParameters { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PlanValidator" /> class.
		/// </summary>
		public PlanValidator()
		{
			this.SetCheckDefaults();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PlanValidator" /> class.
		/// </summary>
		/// <param name="xmlConfig">The XML configuration.</param>
		/// <example>
		/// var validator = new PlanValidator("&lt;Config highScanCount='1000' fatPipeRowCount='50000' mediumCostOperator='300' highCostOperator='1000' highDesiredMemoryKB='100000' highRewindCount='50' /&gt;");
		/// </example>
		public PlanValidator(string xmlConfig)
		{
			this.SetCheckDefaults();
			if (string.IsNullOrWhiteSpace(xmlConfig)) { return; }
			this.CheckParameters.SetValues(xmlConfig);
		}

		/// <summary>
		/// Sets the check defaults.
		/// </summary>
		/// <param name="fatPipeRowCount">The fat pipe row count.</param>
		/// <param name="mediumCostOperator">The medium cost operator.</param>
		/// <param name="highCostOperator">The high cost operator.</param>
		/// <param name="highDesiredMemoryKB">The high desired memory kb.</param>
		/// <param name="highRewindCount">The high rewind count.</param>
		/// <param name="highScanCount">The high scan count.</param>
		/// <param name="highSortCost">The high sort cost.</param>
		/// <param name="highSortCount">The high sort count.</param>
		/// <param name="highJoinCount">The high join count.</param>
		/// <param name="veryHighJoinCount">The very high join count.</param>
		// ReSharper disable once InconsistentNaming
		public void SetCheckDefaults(int fatPipeRowCount = 100000, int mediumCostOperator = 200,
			// ReSharper disable once InconsistentNaming
			int highCostOperator = 1000, int highDesiredMemoryKB = 1000000,
			int highRewindCount = 50, int highScanCount = 1000,
			int highSortCost = 20, int highSortCount = 50000,
			int highJoinCount = 7, int veryHighJoinCount = 10)
		{
			this.CheckParameters = new CheckParameters
			{
				FatPipeRowCount = fatPipeRowCount,
				MediumCostOperator = mediumCostOperator,
				HighCostOperator = highCostOperator,
				HighDesiredMemoryKb = highDesiredMemoryKB,
				HighRewindCount = highRewindCount,
				HighScanCount = highScanCount,

				HighSortCost = Math.Min(100, Math.Max(10, highSortCost)),
				HighSortCount = highSortCount,
				HighJoinCount = highJoinCount,
				VeryHighJoinCount = veryHighJoinCount,
			};

		}

		/// <summary>
		/// Validates the SQL plan.
		/// </summary>
		/// <param name="planXml">The XML.</param>
		/// <returns>IEnumerable&lt;IPlanIssueResult&gt;.</returns>
		/// <exception cref="System.ArgumentNullException">planXml</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">planXml</exception>
		/// <exception cref="ArgumentNullException">planXml</exception>
		/// <exception cref="ArgumentOutOfRangeException">MediumCostOperator</exception>
		public IEnumerable<IPlanIssueResult> ValidateSqlPlan(string planXml)
		{
			if (string.IsNullOrWhiteSpace(planXml)) { throw new ArgumentNullException("planXml"); }

			using (var sr = new StringReader(planXml))
			{
				var doc = XElement.Load(sr);

				var batchRoot = doc.Element(el.BatchSequence);
				if (batchRoot == null)  { yield break; }

				foreach (var batch in batchRoot.Elements(el.Batch))
				{
					var statementsRoot = batch.Elements(el.Statements);

					var statements = statementsRoot.Descendants().Where(st => st.Name == el.StmtSimple || st.Name == el.StmtCursor);
					if(!statements.Any()) { continue; }

					statements = statements.ToList();
					var highestCostStatement = statements.OrderByDescending(st => st.AttributeDouble(att.StatementSubTreeCost, 0)).First();

					yield return new PlanIssueResult(-1)
					{
						Result = PlanResult.StatementCost,
						Cost = highestCostStatement.AttributeInt64(att.StatementSubTreeCost, 0),
						QueryHash = highestCostStatement.AttributeString(att.QueryHash, ""),
						QueryPlanHash = highestCostStatement.AttributeString(att.QueryPlanHash, ""),
						Category = PlanCategory.Trace
					};

					foreach (var statement in statements)
					{
						var planChecks = BasePlanIssueCheck.RunChecks(statement, this.CheckParameters);
						foreach (var planCheck in planChecks)
						{
							foreach (var check in planCheck)
							{
								yield return check;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Generates the results element.
		/// </summary>
		/// <param name="checkResults">The check results.</param>
		/// <returns>XElement.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public XElement GenerateResultsElement(IEnumerable<IPlanIssueResult> checkResults)
		{
			checkResults = checkResults.ToList();

			var plan = new XElement("Plan");
			var statements = new XElement("Statements");
			plan.Add(this.GetThreshHolds(), statements);

			var highestCostCheck = checkResults.FirstOrDefault(x => x.StatementId == -1 && x.Result == PlanResult.StatementCost);
			if (highestCostCheck != null)
			{
				plan.Add(
					new XAttribute("highestCost", highestCostCheck.Cost),
					new XAttribute("highestCostQueryHash", highestCostCheck.QueryHash)
				);
				if (!string.IsNullOrWhiteSpace(highestCostCheck.QueryPlanHash)) { plan.Add(new XAttribute("queryPlanHash", highestCostCheck.QueryPlanHash)); }
			}


			var stmts = checkResults.GroupBy(cr => cr.StatementId);

			foreach (var stmt in stmts.Where(x => x.Key > 0))
			{
				var statementElement = new XElement("Statement", new XAttribute("statementId", stmt.Key));
				statements.Add(statementElement);

				//find the result with the highest cost info
				var highest = checkResults.FirstOrDefault(x => x.StatementId == stmt.Key && x.Result == PlanResult.StatementCost);
				if (highest != null)
				{
					if (highest.Cost > 0) { statementElement.Add(new XAttribute("cost", highest.Cost)); }
					if (!string.IsNullOrWhiteSpace(highest.QueryHash)) { statementElement.Add(new XAttribute("queryHash", highest.QueryHash)); }
					statementElement.Add(new XAttribute("sql", highest.Sql));
				}
				var crs = stmt.Where(cr => cr.Result != PlanResult.StatementCost && cr.Category != PlanCategory.Trace).ToList();
				if (!crs.Any()) { continue; }

				//create and add all the failed checks to the xml
				var checks = new XElement("Checks");
				statementElement.Add(checks);

				foreach (var cr in crs.OrderBy(x => x.NodeId).ThenBy(x => x.Category))
				{
					var check = new XElement("Check");
					if (cr.NodeId.HasValue) { check.Add(new XAttribute("nodeId", cr.NodeId.Value)); }
					check.Add(new XAttribute("type", cr.Category));
					if (!string.IsNullOrWhiteSpace(cr.ObjectName)) { check.Add(new XAttribute("objectName", cr.ObjectName)); }
					if (cr.RowCount.HasValue) { check.Add(new XAttribute("rowCount", cr.RowCount.Value)); }
					check.Value = cr.Result.ToString();
					checks.Add(check);
				}
			}

			return plan;
		}

		/// <summary>
		/// Updates the plan with analysis.
		/// </summary>
		/// <param name="planXml">The plan XML.</param>
		/// <param name="checkResults">The check results.</param>
		/// <returns>XElement.</returns>
		/// <exception cref="System.ArgumentNullException">planXml</exception>
		public XElement UpdatePlanWithAnalysis(string planXml, IEnumerable<IPlanIssueResult> checkResults)
		{
			if (planXml == null) { throw new ArgumentNullException("planXml"); }

			var doc = XDocument.Parse(planXml);

			var plan = doc.Root;
			if (plan == null) return null;

			checkResults = checkResults.ToList();

			plan.Add(new XAttribute(XNamespace.Xmlns + "sspa", sspaEl.Xmlns.ToString()));
			plan.FirstNode.AddBeforeSelf(this.GetThreshHolds(sspaEl.Xmlns));

			var statementChecks = checkResults.Where(x => !x.NodeId.HasValue && x.StatementId > 0).GroupBy(x => new Tuple<int, int>(x.StatementId, x.NodeId.GetValueOrDefault(0)));
			AddChecksToPlan((statementid, nodeid) =>
				plan.Descendants().First(x =>
					el.Groups.StatementElements.Contains(x.Name)
					&& x.AttributeInt32(att.StatementId, -1) == statementid)
				, statementChecks);

			var nodeChecks = checkResults.Where(x => x.NodeId.HasValue && x.StatementId > 0).GroupBy(x => new Tuple<int, int>(x.StatementId, x.NodeId.GetValueOrDefault(0)));

			AddChecksToPlan((statementid, nodeid) =>
			{
				var statement = plan.Descendants().First(x => el.Groups.StatementElements.Contains(x.Name) && x.AttributeInt32(att.StatementId, -1) == statementid);
				var relOp = statement.Descendants(el.RelOp).FirstOrDefault(x => x.AttributeInt32(att.NodeId, -1) == nodeid);
				return relOp;
			}, nodeChecks);

			return doc.Root;
		}

		/// <summary>
		/// Adds the checks to plan.
		/// </summary>
		/// <param name="func">The function.</param>
		/// <param name="checks">The checks.</param>
		private static void AddChecksToPlan(Func<int, int, XElement> func, IEnumerable<IGrouping<Tuple<int, int>, IPlanIssueResult>> checks)
		{
			foreach (var check in checks)
			{
				var element = func.Invoke(check.Key.Item1, check.Key.Item2);
				if (element != null)
				{
					var errorChecks = check.Where(x => x.Category == PlanCategory.Error).ToList();
					var warningChecks = check.Where(x => x.Category == PlanCategory.Warning).ToList();

					if (errorChecks.Any() || warningChecks.Any())
					{
						var resultElement = new XElement(sspaEl.Result);
						element.Add(resultElement);

						if (errorChecks.Any())
						{
							var errorsElement = new XElement(sspaEl.Errors);
							resultElement.Add(errorsElement);
							foreach (var errorCheck in errorChecks)
							{
								errorsElement.Add(new XElement(sspaEl.Error, errorCheck.Result.ToString()));
							}
						}
						if (warningChecks.Any())
						{
							var warningsElement = new XElement(sspaEl.Warnings);
							resultElement.Add(warningsElement);
							foreach (var warningCheck in warningChecks)
							{
								warningsElement.Add(new XElement(sspaEl.Warning, warningCheck.Result.ToString()));
							}
						}

					}
				}
			}
		}

		/// <summary>
		/// Gets the thresh holds.
		/// </summary>
		/// <param name="xmlns">The XMLNS.</param>
		/// <returns>XElement.</returns>
		private XElement GetThreshHolds(XNamespace xmlns = null)
		{
			var threshHolds = xmlns != null ? new XElement(XName.Get("ThreshHolds", xmlns.ToString())) : new XElement("ThreshHolds");

			threshHolds.Add(
				new XAttribute("fatPipeRowCount", this.CheckParameters.FatPipeRowCount),
				new XAttribute("mediumCostOperator", this.CheckParameters.MediumCostOperator),
				new XAttribute("highCostOperator", this.CheckParameters.HighCostOperator),
				new XAttribute("highDesiredMemoryKB", this.CheckParameters.HighDesiredMemoryKb),
				new XAttribute("highRewindCount", this.CheckParameters.HighRewindCount),
				new XAttribute("highScanCount", this.CheckParameters.HighScanCount),
				new XAttribute("highSortCost", this.CheckParameters.HighSortCost),
				new XAttribute("highSortCount", this.CheckParameters.HighSortCount),
				new XAttribute("highJoinCount", this.CheckParameters.HighJoinCount),
				new XAttribute("veryHighJoinCount", this.CheckParameters.VeryHighJoinCount)
			);
			return threshHolds;
		}
	}
}
