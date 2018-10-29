// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 07-13-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-13-2016
// ***********************************************************************
// <copyright file="RelOpArgs.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Linq;
using System.Xml.Linq;
using SqlServer.ExecutionPlan.Analysis.Internals;
using SqlServer.ExecutionPlan.Analysis.Properties;
using el = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanElementNames;
using att = SqlServer.ExecutionPlan.Analysis.Internals.SqlPlanAttributeNames;

namespace SqlServer.ExecutionPlan.Analysis.Checks
{
    /// <summary>
    /// Class RelOpArgs.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rel")]
    public class RelOpArgs
    {
        /// <summary>
        /// The _x element
        /// </summary>
        private readonly XElement _relopElement;

        /// <summary>
        /// The _estimate rows
        /// </summary>
        private readonly double _estimateRows;
        /// <summary>
        /// The _rebinds
        /// </summary>
        private readonly double _rebinds;
        /// <summary>
        /// The _physical op
        /// </summary>
        private readonly string _physicalOp;
        /// <summary>
        /// The _logical op
        /// </summary>
        private readonly string _logicalOp;
        /// <summary>
        /// The _operator cost
        /// </summary>
        private readonly double _operatorCost;
        /// <summary>
        /// The _node identifier
        /// </summary>
        private readonly int _nodeId;

        /// <summary>
        /// Gets the estimate rows.
        /// </summary>
        /// <value>The estimate rows.</value>
        public double EstimateRows { get { return _estimateRows; } }

        /// <summary>
        /// Gets the rebinds.
        /// </summary>
        /// <value>The rebinds.</value>
        public double Rebinds { get { return _rebinds; } }

        /// <summary>
        /// Gets the row count.
        /// </summary>
        /// <value>The row count.</value>
        public double RowCount { get { return this.EstimateRows * this.Rebinds; } }

        /// <summary>
        /// Gets the physical op.
        /// </summary>
        /// <value>The physical op.</value>
        public string PhysicalOp { get { return _physicalOp; } }

        /// <summary>
        /// Gets the logical op.
        /// </summary>
        /// <value>The logical op.</value>
        public string LogicalOp { get { return _logicalOp; } }

        /// <summary>
        /// Gets the operator cost.
        /// </summary>
        /// <value>The operator cost.</value>
        public double OperatorCost { get { return _operatorCost; } }

        /// <summary>
        /// Gets the node identifier.
        /// </summary>
        /// <value>The node identifier.</value>
        public int NodeId { get { return _nodeId; } }


        /// <summary>
        /// Gets the operator cost percent.
        /// </summary>
        /// <param name="statementCost">The statement cost.</param>
        /// <returns>System.Double.</returns>
        public double GetOperatorCostPercent(double statementCost)
        {
            if (statementCost <= 0) return 0;
            return this.OperatorCost / statementCost * 100;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelOpArgs"/> class.
        /// </summary>
        /// <param name="relopElement">The x element.</param>
        /// <exception cref="System.ArgumentNullException">xElement</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">xElement</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "relop")]
        public RelOpArgs(XElement relopElement)
        {
            if (relopElement == null) { throw new ArgumentNullException("relopElement"); }
            if (relopElement.Name != el.RelOp) { throw new ArgumentOutOfRangeException("relopElement", Resources.RelOpArgs_RelOpArgs_Invalid_element_type); }

            _relopElement = relopElement;

            _estimateRows = _relopElement.AttributeDouble(att.EstimateRows, 1);
            _rebinds = Math.Max(1, _relopElement.AttributeDouble(att.EstimateRebinds, 1));
            _physicalOp = _relopElement.AttributeString(att.PhysicalOp, "").ToLower();
            _logicalOp = _relopElement.AttributeString(att.LogicalOp, "").ToLower();
            _operatorCost = GetOperatorCost(_relopElement);
            _nodeId = _relopElement.AttributeInt32(att.NodeId, 0);

        }

        private static double GetOperatorCost(XElement relopElement)
        {
            var cost = relopElement.AttributeDouble(att.EstimatedTotalSubtreeCost, 0);
            var firstDescendant = relopElement.Descendants(el.RelOp).FirstOrDefault();
            if (firstDescendant == null || firstDescendant.Parent == null) { return cost; }

            var childRelOps = firstDescendant.Parent.Elements(el.RelOp);
            cost -= childRelOps.Sum(cr => cr.AttributeDouble(att.EstimatedTotalSubtreeCost, 0));
            return cost;
        }
    }
}
