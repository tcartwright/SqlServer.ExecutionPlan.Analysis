// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 07-13-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-13-2016
// ***********************************************************************
// <copyright file="CheckParameters.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.IO;
using System.Xml.Linq;
using SqlServer.ExecutionPlan.Analysis.Internals;
using SqlServer.ExecutionPlan.Analysis.Properties;

namespace SqlServer.ExecutionPlan.Analysis.Checks
{
	/// <summary>
	/// Class CheckParameters.
	/// </summary>
	public class CheckParameters
	{

		/// <summary>
		/// Gets or sets the fat pipe row count.
		/// </summary>
		/// <value>The fat pipe row count.</value>
		public long FatPipeRowCount { get; set; }
		/// <summary>
		/// Gets or sets the medium cost operator.
		/// </summary>
		/// <value>The medium cost operator.</value>
		public int MediumCostOperator { get; set; }
		/// <summary>
		/// Gets or sets the high cost operator.
		/// </summary>
		/// <value>The high cost operator.</value>
		public int HighCostOperator { get; set; }
		/// <summary>
		/// Gets or sets the high desired memory kb.
		/// </summary>
		/// <value>The high desired memory kb.</value>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Kb")]
		public long HighDesiredMemoryKb { get; set; }
		/// <summary>
		/// Gets or sets the high rewind count.
		/// </summary>
		/// <value>The high rewind count.</value>
		public int HighRewindCount { get; set; }
		/// <summary>
		/// Gets or sets the high join count.
		/// </summary>
		/// <value>The high join count.</value>
		public int HighJoinCount { get; set; }

		/// <summary>
		/// Gets or sets the very high join count.
		/// </summary>
		/// <value>The very high join count.</value>
		public int VeryHighJoinCount { get; set; }

		/// <summary>
		/// Gets or sets the high sort cost.
		/// </summary>
		/// <value>The high sort cost.</value>
		public int HighSortCost { get; set; }

		/// <summary>
		/// Gets or sets the high sort count.
		/// </summary>
		/// <value>The high sort count.</value>
		public long HighSortCount { get; set; }

		/// <summary>
		/// Gets or sets the high scan count.
		/// </summary>
		/// <value>The high scan count.</value>
		public long HighScanCount { get; set; }

		/// <summary>
		/// Sets the values.
		/// </summary>
		/// <param name="xmlConfig">The XML configuration.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">xmlConfig</exception>
		public void SetValues(string xmlConfig)
		{
			using (var sr = new StringReader(xmlConfig))
			{
				var root = XElement.Load(sr);

				this.MediumCostOperator = root.AttributeInt32(XName.Get("mediumCostOperator"), this.MediumCostOperator);
				this.HighCostOperator = root.AttributeInt32(XName.Get("highCostOperator"), this.HighCostOperator);
				if (this.MediumCostOperator >= this.HighCostOperator) { throw new ArgumentOutOfRangeException("xmlConfig", Resources.PlanValidator_ValidateSqlPlan_The_MediumCostOperator_must_be_lower_than_the_HighCostOperator); }

				this.FatPipeRowCount = root.AttributeInt64(XName.Get("fatPipeRowCount"), this.FatPipeRowCount);
				this.HighDesiredMemoryKb = root.AttributeInt64(XName.Get("highDesiredMemoryKB"), this.HighDesiredMemoryKb);
				this.HighRewindCount = root.AttributeInt32(XName.Get("highRewindCount"), this.HighRewindCount);
				this.HighScanCount = root.AttributeInt64(XName.Get("highScanCount"), this.HighScanCount);
				this.HighSortCost = Math.Min(100, Math.Max(10, root.AttributeInt32(XName.Get("highSortCost"), this.HighSortCost)));
				this.HighSortCount = root.AttributeInt64(XName.Get("highSortCount"), this.HighSortCount);
				this.HighJoinCount = root.AttributeInt32(XName.Get("highJoinCount"), this.HighJoinCount);
				this.VeryHighJoinCount = root.AttributeInt32(XName.Get("veryHighJoinCount"), this.VeryHighJoinCount);

			}
		}
	}
}
