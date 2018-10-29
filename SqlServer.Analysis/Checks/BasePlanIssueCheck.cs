using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SqlServer.ExecutionPlan.Analysis.Checks.RelOpChecks;
using SqlServer.ExecutionPlan.Analysis.Checks.StatementChecks;
using att = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanAttributeNames;
using el = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanElementNames;

namespace SqlServer.ExecutionPlan.Analysis.Checks
{
	/// <summary>
	/// Class BasePlanIssueCheck.
	/// </summary>
	public abstract class BasePlanIssueCheck
	{
		/// <summary>
		/// The comparer
		/// </summary>
		protected StringComparer Comparer = StringComparer.InvariantCultureIgnoreCase;

		/// <summary>
		/// Gets the matches.
		/// </summary>
		/// <value>The matches.</value>
		public IList<XName> Matches { get; private set; }

		/// <summary>
		/// Gets the check parameters.
		/// </summary>
		/// <value>The check parameters.</value>
		protected CheckParameters CheckParameters { get; private set; }

		/// <summary>
		/// Gets the statement arguments.
		/// </summary>
		/// <value>The statement arguments.</value>
		protected StatementArgs StatementArgs { get; private set; }

		/// <summary>
		/// Gets the relative op arguments.
		/// </summary>
		/// <value>The relative op arguments.</value>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rel")]
		protected RelOpArgs RelOpArgs { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="BasePlanIssueCheck" /> class.
		/// </summary>
		/// <param name="matches">The matches.</param>
		protected BasePlanIssueCheck(params XName[] matches)
		{
			this.Matches = new List<XName>(matches);
		}

		#region checks
		// ReSharper disable once InconsistentNaming
		/// <summary>
		/// The _statement checks
		/// </summary>
		private static readonly List<BasePlanIssueCheck> _statementChecks = new List<BasePlanIssueCheck>
		{
			new CheckForCursor(),
			new CheckForNoLock(),
			new CheckForTooManyJoins(),
			new CheckForHighDesiredMemory(),
			new CheckForWarnings(),
			new CheckForMissingIndexes(),
			new CheckForCompileIssues(),
			new CheckForConvertImplicits(),
			new CheckForParameterizedSql(),
			new CheckForCosts(),
			new CheckForFatPipes(),
			new CheckForStatementCost()
		};

		// ReSharper disable once InconsistentNaming
		/// <summary>
		/// The _rel op checks
		/// </summary>
		private static readonly List<BasePlanIssueCheck> _relOpChecks = new List<BasePlanIssueCheck>
		{
			new CheckForRelOpWarnings(),
			new CheckForTableFunctionJoins(),
			new CheckForKeyLookups(),
			new CheckForScans(),
			new CheckForSpools(),
			new CheckForHighCostSort()
		};
		#endregion checks

		/// <summary>
		/// Runs the statement checks.
		/// </summary>
		/// <param name="statement">The x element.</param>
		/// <param name="checkParameters">The check parameters.</param>
		/// <returns>IEnumerable&lt;IEnumerable&lt;IPlanIssueResult&gt;&gt;.</returns>
		/// <exception cref="System.ArgumentNullException">xElement
		/// or
		/// checkParameters</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x")]
		public static IEnumerable<IEnumerable<IPlanIssueResult>> RunChecks(XElement statement, CheckParameters checkParameters)
		{
			if (statement == null) { throw new ArgumentNullException("statement"); }
			if (checkParameters == null) { throw new ArgumentNullException("checkParameters"); }
			var statementArgs = new StatementArgs(statement);

			foreach (var check in _statementChecks)
			{
				yield return check.Check(statement, checkParameters, statementArgs);
			}

			if (statement.Name != el.StmtSimple) { yield break; }
			var relOps = statement.Descendants(el.RelOp);
			foreach (var relOp in relOps)
			{
				var relOpArgs = new RelOpArgs(relOp);
				foreach (var check in _relOpChecks)
				{
					yield return check.Check(relOp, checkParameters, statementArgs, relOpArgs);
				}
			}
		}

		/// <summary>
		/// Checks the specified x element.
		/// </summary>
		/// <param name="xElement">The x element.</param>
		/// <param name="checkParameters">The check parameters.</param>
		/// <param name="statementArgs">The statement arguments.</param>
		/// <param name="relOpArgs">The relative op arguments.</param>
		/// <returns>IEnumerable&lt;IPlanIssueResult&gt;.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rel"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x")]
		public virtual IEnumerable<IPlanIssueResult> Check(XElement xElement, CheckParameters checkParameters, StatementArgs statementArgs = null, RelOpArgs relOpArgs = null)
		{
			// ReSharper disable once SimplifyLinqExpression
			if (!this.Matches.Any(x => x == xElement.Name)) { return Enumerable.Empty<IPlanIssueResult>(); }

			this.CheckParameters = checkParameters;
			this.StatementArgs = statementArgs;
			this.RelOpArgs = relOpArgs;

			return this.GetIssues(xElement);
		}

		/// <summary>
		/// Gets the issues.
		/// </summary>
		/// <param name="xElement">The x element.</param>
		/// <returns>IEnumerable&lt;IPlanIssueResult&gt;.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x")]
		protected abstract IEnumerable<IPlanIssueResult> GetIssues(XElement xElement);

	}
}
