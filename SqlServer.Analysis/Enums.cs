// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 06-17-2016
//
// Last Modified By : tdcart
// Last Modified On : 06-17-2016
// ***********************************************************************
// <copyright file="Enums.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace  SqlServer.ExecutionPlan.Analysis
{

	/// <summary>
	/// Enum PlanCategory
	/// </summary>
	public enum PlanCategory
	{
		Trace = 0,
		Warning = 1,
		Error = 2,
	}

	/// <summary>
	/// Enum PlanResult
	/// </summary>
	public enum PlanResult
	{
		Nothing,
		ImplicitConversion,
		MissingIndex,
		KeyLookup,
		IndexScan,
		ClusteredIndexScan,
		FatPipes,
		MediumCostOperator,
		HighCostOperator,
		IndexSpool,
		TableSpool,
		// ReSharper disable once InconsistentNaming
		HighDesiredMemoryKB,
		HighRewindCount,
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "RID")]
		// ReSharper disable once InconsistentNaming
		RIDLookup,
		StatementCost,
		NoLock,
		Cursor,
		CardinalityConvertIssue,
		HighCostSort,
		NoJoinPredicate,
		CompilationTimeout,
		CompilationMemoryLimitExceeded,
		HighNumberOfJoins,
		JoinToTableValueFunction,
		ColumnsWithNoStatistics,
		SpillToTempDb,
		UnmatchedIndexes,
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Nonparameterized")]
		PossibleNonparameterizedQuery
	}
}
