// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 07-13-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-13-2016
// ***********************************************************************
// <copyright file="SqlPlanXNames.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Xml.Linq;

// ReSharper disable InconsistentNaming

namespace SqlServer.ExecutionPlan.Analysis.Internals
{
	/// <summary>
	/// Class SqlPlanElementNames.
	/// </summary>
	internal static class SqlPlanElementNames
	{
		/// <summary>
		/// Class Groups.
		/// </summary>
		public static class Groups
		{
			/// <summary>
			/// The statement elements
			/// </summary>
			public static readonly XName[] StatementElements = new XName[] { StmtSimple, StmtCursor, StmtCond, StmtReceive, StmtUseDb };
		}
		/// <summary>
		/// The XMLNS
		/// </summary>
		public static readonly XNamespace Xmlns = XNamespace.Get("http://schemas.microsoft.com/sqlserver/2004/07/showplan");
		/// <summary>
		/// The batch sequence
		/// </summary>
		public static readonly XName BatchSequence = Xmlns.GetName("BatchSequence");
		/// <summary>
		/// The batch
		/// </summary>
		public static readonly XName Batch = Xmlns.GetName("Batch");
		/// <summary>
		/// The statements
		/// </summary>
		public static readonly XName Statements = Xmlns.GetName("Statements");
		/// <summary>
		/// The statement simple
		/// </summary>
		public static readonly XName StmtSimple = Xmlns.GetName("StmtSimple");
		/// <summary>
		/// The statement cursor
		/// </summary>
		public static readonly XName StmtCursor = Xmlns.GetName("StmtCursor");
		/// <summary>
		/// The statement cond
		/// </summary>
		public static readonly XName StmtCond = Xmlns.GetName("StmtCond");
		/// <summary>
		/// The statement receive
		/// </summary>
		public static readonly XName StmtReceive = Xmlns.GetName("StmtReceive");
		/// <summary>
		/// The statement use database
		/// </summary>
		public static readonly XName StmtUseDb = Xmlns.GetName("StmtUseDb");
		/// <summary>
		/// The query plan
		/// </summary>
		public static readonly XName QueryPlan = Xmlns.GetName("QueryPlan");
		/// <summary>
		/// The missing indexes
		/// </summary>
		public static readonly XName MissingIndexes = Xmlns.GetName("MissingIndexes");
		/// <summary>
		/// The warnings
		/// </summary>
		public static readonly XName Warnings = Xmlns.GetName("Warnings");
		/// <summary>
		/// The plan affecting convert
		/// </summary>
		public static readonly XName PlanAffectingConvert = Xmlns.GetName("PlanAffectingConvert");
		/// <summary>
		/// The relative op
		/// </summary>
		public static readonly XName RelOp = Xmlns.GetName("RelOp");
		/// <summary>
		/// The index scan
		/// </summary>
		public static readonly XName IndexScan = Xmlns.GetName("IndexScan");
		/// <summary>
		/// The predicate
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly XName Predicate = Xmlns.GetName("Predicate");
		/// <summary>
		/// The scalar operator
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly XName ScalarOperator = Xmlns.GetName("ScalarOperator");
		/// <summary>
		/// The object
		/// </summary>
		public static readonly XName Object = Xmlns.GetName("Object");
		/// <summary>
		/// The memory grant information
		/// </summary>
		public static readonly XName MemoryGrantInfo = Xmlns.GetName("MemoryGrantInfo");
		/// <summary>
		/// The stored proc
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly XName StoredProc = Xmlns.GetName("StoredProc");
		/// <summary>
		/// The columns with no statistics
		/// </summary>
		public static readonly XName ColumnsWithNoStatistics = Xmlns.GetName("ColumnsWithNoStatistics");
		/// <summary>
		/// The spill to temporary database
		/// </summary>
		public static readonly XName SpillToTempDb = Xmlns.GetName("SpillToTempDb");
		/// <summary>
		/// The unmatched indexes
		/// </summary>
		public static readonly XName UnmatchedIndexes = Xmlns.GetName("UnmatchedIndexes");
		/// <summary>
		/// The convert
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly XName Convert = Xmlns.GetName("Convert");
		/// <summary>
		/// The constant
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly XName Const = Xmlns.GetName("Const");
	}

	/// <summary>
	/// Class SqlPlanAttributeNames.
	/// </summary>
	internal static class SqlPlanAttributeNames
	{
		/// <summary>
		/// The statement identifier
		/// </summary>
		public static readonly XName StatementId = XName.Get("StatementId");
		/// <summary>
		/// The statement text
		/// </summary>
		public static readonly XName StatementText = XName.Get("StatementText");
		/// <summary>
		/// The serial desired memory
		/// </summary>
		public static readonly XName SerialDesiredMemory = XName.Get("SerialDesiredMemory");
		/// <summary>
		/// The query hash
		/// </summary>
		public static readonly XName QueryHash = XName.Get("QueryHash");
		/// <summary>
		/// The statement sub tree cost
		/// </summary>
		public static readonly XName StatementSubTreeCost = XName.Get("StatementSubTreeCost");
		/// <summary>
		/// The query plan hash
		/// </summary>
		public static readonly XName QueryPlanHash = XName.Get("QueryPlanHash");
		/// <summary>
		/// The estimated total subtree cost
		/// </summary>
		public static readonly XName EstimatedTotalSubtreeCost = XName.Get("EstimatedTotalSubtreeCost");
		/// <summary>
		/// The estimate rows
		/// </summary>
		public static readonly XName EstimateRows = XName.Get("EstimateRows");
		/// <summary>
		/// The estimate rebinds
		/// </summary>
		public static readonly XName EstimateRebinds = XName.Get("EstimateRebinds");
		/// <summary>
		/// The physical op
		/// </summary>
		public static readonly XName PhysicalOp = XName.Get("PhysicalOp");
		/// <summary>
		/// The estimate rewinds
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly XName EstimateRewinds = XName.Get("EstimateRewinds");
		/// <summary>
		/// The node identifier
		/// </summary>
		public static readonly XName NodeId = XName.Get("NodeId");
		/// <summary>
		/// The lookup
		/// </summary>
		public static readonly XName Lookup = XName.Get("Lookup");
		/// <summary>
		/// The scalar string
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly XName ScalarString = XName.Get("ScalarString");
		/// <summary>
		/// The index
		/// </summary>
		public static readonly XName Index = XName.Get("Index");
		/// <summary>
		/// The estimate cpu
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly XName EstimateCPU = XName.Get("EstimateCPU");

		/// <summary>
		/// The estimate io
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly XName EstimateIO = XName.Get("EstimateIO");
		/// <summary>
		/// The no join predicate
		/// </summary>
		public static readonly XName NoJoinPredicate = XName.Get("NoJoinPredicate");
		/// <summary>
		/// The statement optm early abort reason
		/// </summary>
		public static readonly XName StatementOptmEarlyAbortReason = XName.Get("StatementOptmEarlyAbortReason");
		/// <summary>
		/// The logical op
		/// </summary>
		public static readonly XName LogicalOp = XName.Get("LogicalOp");
		/// <summary>
		/// The proc name
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly XName ProcName = XName.Get("ProcName");
		/// <summary>
		/// The expression
		/// </summary>
		public static readonly XName Expression = XName.Get("Expression");
		/// <summary>
		/// The implicit
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly XName Implicit = XName.Get("Implicit");
		/// <summary>
		/// The constant value
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly XName ConstValue = XName.Get("ConstValue");
		/// <summary>
		/// The statement type
		/// </summary>
		public static readonly XName StatementType = XName.Get("StatementType");
		/// <summary>
		/// The statement est rows
		/// </summary>
		public static readonly XName StatementEstRows = XName.Get("StatementEstRows");
		
	}
}
