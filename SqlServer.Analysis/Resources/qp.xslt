<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
		xmlns:msxsl="urn:schemas-microsoft-com:xslt"
		xmlns:s="http://schemas.microsoft.com/sqlserver/2004/07/showplan"
		xmlns:fn="http://Tim Cartwright.com/jsfunctions"
		xmlns:sspa="http://Tim Cartwright.com/plananalysis"
		exclude-result-prefixes="msxsl s xsl sspa fn">
	<xsl:output method="html" indent="yes" omit-xml-declaration="yes" />

	<xsl:variable name="lowercase" select="'abcdefghijklmnopqrstuvwxyz'" />
	<xsl:variable name="uppercase" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />

	<!-- Disable built-in recursive processing templates -->
	<xsl:template match="*|/|text()|@*" mode="NodeLabel2" />
	<xsl:template match="*|/|text()|@*" mode="ToolTipDescription" />
	<xsl:template match="*|/|text()|@*" mode="ToolTipDetails" />

	<!-- Default template -->
	<xsl:template match="/">
		<xsl:apply-templates select="s:ShowPlanXML" />
	</xsl:template>

	<!-- Outermost div that contains all statement plans. -->
	<xsl:template match="s:ShowPlanXML">
		<div class="qp-root">
			<hr class="qp-statement-break"></hr>
			<xsl:apply-templates select="s:BatchSequence/s:Batch/s:Statements/s:StmtSimple|s:BatchSequence/s:Batch/s:Statements/s:StmtCursor|s:BatchSequence/s:Batch/s:Statements/s:StmtCond|s:BatchSequence/s:Batch/s:Statements/s:StmtUseDb|s:BatchSequence/s:Batch/s:Statements/s:StmtReceive" />
		</div>
	</xsl:template>

	<!-- Matches a branch in the query plan (either an operation or a statement) -->
	<xsl:template match="s:RelOp|s:StmtSimple|s:StmtCursor|s:StmtCond|s:StmtUseDb|s:StmtReceive">
		<xsl:variable name="hasParentStatements" select="count(ancestor::s:StmtSimple|ancestor::s:StmtCursor|ancestor::s:StmtCond)"/>
		
		<xsl:if test="name() != 'RelOp' and $hasParentStatements = 0">
			<xsl:variable name="cost" select="fn:Max(number(@StatementSubTreeCost), sum(.//s:StmtSimple[@StatementSubTreeCost]/@StatementSubTreeCost))"></xsl:variable>

			<span class="qp-info-header">Query: <xsl:value-of select="position()"/>, Batch Relative Cost:<xsl:choose>
					<xsl:when test="number($cost) > 0">
						<xsl:variable name="totalCost" select="sum(//s:StmtSimple[@StatementSubTreeCost]/@StatementSubTreeCost)"></xsl:variable>
						<xsl:value-of select="format-number(number($cost) div $totalCost, '#0.0%')"/>
					</xsl:when>
					<xsl:otherwise>0%</xsl:otherwise>
				</xsl:choose>
			</span>
		</xsl:if>

		<xsl:if test="name() != 'RelOp' and $hasParentStatements = 0 and .//s:QueryPlan/s:MissingIndexes/s:MissingIndexGroup">
		<div class="missing-indexes">
			<table>
				<caption>Possible Missing Indexes</caption>
				<tr>
					<th>Table</th>
					<th>Potential Impact</th>
					<th>Columns</th>
					<th>Includes</th>
				</tr>
			<xsl:for-each select=".//s:QueryPlan/s:MissingIndexes/s:MissingIndexGroup">
				<tr>
					<td><xsl:value-of select="./s:MissingIndex/@Table"/></td>
					<td><xsl:value-of select="format-number((number(@Impact) div 100), '##.00%')"/></td>
					<td><xsl:apply-templates select="./s:MissingIndex/s:ColumnGroup[contains(@Usage, 'EQUALITY')]/s:Column" mode="WriteColumns" />&#160;</td>
					<td><xsl:apply-templates select="./s:MissingIndex/s:ColumnGroup[@Usage = 'INCLUDE']/s:Column" mode="WriteColumns" />&#160;</td>
				</tr>
			</xsl:for-each>
			</table>
		</div>
		</xsl:if>
		<div class="qp-tr">
			<xsl:if test="name() != 'RelOp' and $hasParentStatements = 0">
				<xsl:attribute name="id">statement<xsl:value-of select="position()"></xsl:value-of></xsl:attribute>
				<canvas class="qp-tr-canvas"></canvas>
			</xsl:if>
				<div>
				<div class="qp-node" >
					<xsl:choose>
						<xsl:when test="count(./sspa:Result/sspa:Errors/sspa:Error) > 0">
							<xsl:attribute name="class">qp-node qp-error</xsl:attribute>
						</xsl:when>
						<xsl:when test="count(./sspa:Result/sspa:Warnings/sspa:Warning) > 0">
							<xsl:attribute name="class">qp-node qp-warning</xsl:attribute>
						</xsl:when>
					</xsl:choose>

					<xsl:attribute name="statementId">
						<xsl:value-of select='@StatementId|./ancestor::s:StmtSimple[1]/@StatementId|./ancestor::s:StmtCursor[1]/@StatementId|./ancestor::s:StmtCond[1]/@StatementId|./ancestor::s:StmtUseDb[1]/@StatementId|./ancestor::s:StmtReceive[1]/@StatementId' />
					</xsl:attribute>
					<xsl:if test="name() = 'RelOp'">
						<xsl:attribute name="nodeId">
							<xsl:value-of select='@NodeId' />
						</xsl:attribute>
					</xsl:if>
					<xsl:apply-templates select="." mode="NodeIcon" />
					<xsl:if test="name() = 'RelOp' and (@Parallel = '1' or @Parallel = 'true')">
					<div class="qp-icon-parallel"></div>
					</xsl:if>
					<xsl:apply-templates select="." mode="NodeLabel" />
					<xsl:apply-templates select="." mode="NodeLabel2" />
					<xsl:apply-templates select="." mode="NodeCostLabel" />
					<xsl:call-template name="ToolTip" />
				</div>
			</div>
			<div>
				<xsl:apply-templates select="*/s:RelOp|*//s:StmtSimple|*//s:StmtCursor|*//s:StmtCond" />
			</div>
		</div>
		<xsl:if test="name() != 'RelOp' and count(ancestor::s:StmtSimple|ancestor::s:StmtCursor|ancestor::s:StmtCond) = 0">
			<hr class="qp-statement-break" />
		</xsl:if>
	</xsl:template>

	<xsl:template match="s:MissingIndex/s:ColumnGroup/s:Column" mode="WriteColumns">
		<xsl:value-of select="@Name"/>
		<xsl:if test="not(position() = last())">
			<xsl:text>, </xsl:text>
		</xsl:if>
	</xsl:template>
	
	<!-- Writes the tool tip -->
	<xsl:template name="ToolTip">
		<div class="qp-tt">
			<div class="qp-tt-header">
				<xsl:choose>
					<xsl:when test="(@PhysicalOp = 'Clustered Index Seek' or @PhysicalOp = 'Clustered Index Scan') and ./s:IndexScan[@Lookup = '1' or @Lookup = 'true']">Key Lookup (Clustered)</xsl:when>
					<xsl:when test="(@PhysicalOp = 'Index Seek' or @PhysicalOp = 'Index Scan') and ./s:IndexScan[@Lookup = '1' or @Lookup = 'true']">Key Lookup</xsl:when>
					<xsl:when test="./@StatementType = 'DROP OBJECT' and contains(translate(./@StatementText, $lowercase, $uppercase), 'DROP TABLE')">DROP TABLE</xsl:when>
					<xsl:when test="./@StatementType = 'COND' and contains(translate(./@StatementText, $lowercase, $uppercase), 'WHILE')">WHILE</xsl:when>
					<xsl:when test="./@StatementType = 'COND' and contains(translate(./@StatementText, $lowercase, $uppercase), 'IF')">IF</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="@PhysicalOp | @StatementType" />
					</xsl:otherwise>
				</xsl:choose>
			</div>
			<div>
				<xsl:apply-templates select="." mode="ToolTipDescription" />
			</div>
			<xsl:call-template name="ToolTipGrid" />
			<xsl:apply-templates select="* | @* | */* | */@*" mode="ToolTipDetails" />
		</div>
	</xsl:template>


	<!-- Writes the grid of node properties to the tool tip -->
	<xsl:template name="ToolTipGrid">
		<xsl:variable name="totalRows" select="number(fn:ToDecimal(string(@StatementEstRows | @EstimateRows))) * fn:Max(number(@EstimateRebinds), 1)"/>
		<xsl:variable name="dataSize" select="fn:ToDecimal(string($totalRows)) * fn:ToDecimal(number(@AvgRowSize))"></xsl:variable>
		<table class="qp-toolTip">
			<xsl:call-template name="ToolTipRow">
				<xsl:with-param name="Condition" select="s:QueryPlan/@CachedPlanSize" />
				<xsl:with-param name="Label">Cached plan size</xsl:with-param>
				<xsl:with-param name="Value" select="concat(s:QueryPlan/@CachedPlanSize, ' B')" />
			</xsl:call-template>
			<xsl:choose>
				<xsl:when test="(contains(@PhysicalOp, 'Index Seek') or contains(@PhysicalOp, 'Index Scan')) and ./s:IndexScan[@Lookup = '1' or @Lookup = 'true']">
					<xsl:call-template name="ToolTipRow">
						<xsl:with-param name="Label">Physical Operation</xsl:with-param>
						<xsl:with-param name="Value" select="'Key Lookup'" />
					</xsl:call-template>
				</xsl:when>
				<xsl:otherwise>
					<xsl:call-template name="ToolTipRow">
						<xsl:with-param name="Label">Physical Operation</xsl:with-param>
						<xsl:with-param name="Value" select="@PhysicalOp" />
					</xsl:call-template>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:choose>
				<xsl:when test="(contains(@PhysicalOp, 'Index Seek') or contains(@PhysicalOp, 'Index Scan')) and ./s:IndexScan[@Lookup = '1' or @Lookup = 'true']">
					<xsl:call-template name="ToolTipRow">
						<xsl:with-param name="Label">Logical Operation</xsl:with-param>
						<xsl:with-param name="Value" select="'Key Lookup'" />
					</xsl:call-template>
				</xsl:when>
				<xsl:otherwise>
					<xsl:call-template name="ToolTipRow">
						<xsl:with-param name="Label">Logical Operation</xsl:with-param>
						<xsl:with-param name="Value" select="@LogicalOp" />
					</xsl:call-template>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:call-template name="ToolTipRow">
				<xsl:with-param name="Label">Actual Number of Rows</xsl:with-param>
				<xsl:with-param name="Value" select="s:RunTimeInformation/s:RunTimeCountersPerThread/@ActualRows" />
			</xsl:call-template>
			<xsl:call-template name="ToolTipRow">
				<xsl:with-param name="Condition" select="@EstimateIO" />
				<xsl:with-param name="Label">Estimated I/O Cost</xsl:with-param>
				<xsl:with-param name="Value">
					<xsl:call-template name="round">
						<xsl:with-param name="value" select="@EstimateIO" />
					</xsl:call-template>
				</xsl:with-param>
			</xsl:call-template>
			<xsl:call-template name="ToolTipRow">
				<xsl:with-param name="Condition" select="@EstimateCPU" />
				<xsl:with-param name="Label">Estimated CPU Cost</xsl:with-param>
				<xsl:with-param name="Value">
					<xsl:call-template name="round">
						<xsl:with-param name="value" select="@EstimateCPU" />
					</xsl:call-template>
				</xsl:with-param>
			</xsl:call-template>
			<!-- TODO: Estimated Number of Executions -->
			<xsl:call-template name="ToolTipRow">
				<xsl:with-param name="Label">Number of Executions</xsl:with-param>
				<xsl:with-param name="Value" select="s:RunTimeInformation/s:RunTimeCountersPerThread/@ActualExecutions" />
			</xsl:call-template>
			<xsl:call-template name="ToolTipRow">
				<xsl:with-param name="Label">Degree of Parallelism</xsl:with-param>
				<xsl:with-param name="Value" select="s:QueryPlan/@DegreeOfParallelism" />
			</xsl:call-template>
			<xsl:call-template name="ToolTipRow">
				<xsl:with-param name="Label">Memory Grant</xsl:with-param>
				<xsl:with-param name="Value" select="s:QueryPlan/@MemoryGrant" />
			</xsl:call-template>
			<xsl:call-template name="ToolTipRow">
				<xsl:with-param name="Condition" select="@EstimateIO | @EstimateCPU" />
				<xsl:with-param name="Label">Estimated Operator Cost</xsl:with-param>
				<xsl:with-param name="Value">
					<xsl:variable name="EstimatedOperatorCost">
						<xsl:call-template name="EstimatedOperatorCost" />
					</xsl:variable>
					<xsl:variable name="TotalCost">
						<xsl:call-template name="StatementSubTreeCost" />
					</xsl:variable>

					<xsl:call-template name="round">
						<xsl:with-param name="value" select="$EstimatedOperatorCost" />
					</xsl:call-template>
					(<xsl:value-of select="format-number($EstimatedOperatorCost div $TotalCost, '0.0%')" />)
				</xsl:with-param>
			</xsl:call-template>
			<xsl:call-template name="ToolTipRow">
				<xsl:with-param name="Condition" select="@StatementSubTreeCost | @EstimatedTotalSubtreeCost" />
				<xsl:with-param name="Label">Estimated Subtree Cost</xsl:with-param>
				<xsl:with-param name="Value">
					<xsl:call-template name="round">
						<xsl:with-param name="value" select="@StatementSubTreeCost | @EstimatedTotalSubtreeCost" />
					</xsl:call-template>
				</xsl:with-param>
			</xsl:call-template>
			<xsl:call-template name="ToolTipRow">
				<xsl:with-param name="Condition" select="@StatementEstRows | @EstimateRows" />
				<xsl:with-param name="Label">Estimated Number of Rows</xsl:with-param>
				<xsl:with-param name="Value" select="format-number(fn:ToDecimal(string(@StatementEstRows | @EstimateRows)), '###,###.0')" />
			</xsl:call-template>
			<xsl:call-template name="ToolTipRow">
				<xsl:with-param name="Condition" select="@StatementEstRows | @EstimateRows" />
				<xsl:with-param name="Label">Estimated Complete Number of Rows</xsl:with-param>
				<xsl:with-param name="Value" select="format-number($totalRows, '###,###.0')" />
			</xsl:call-template>
			<xsl:call-template name="ToolTipRow">
				<xsl:with-param name="Condition" select="@AvgRowSize" />
				<xsl:with-param name="Label">Estimated Row Size</xsl:with-param>
				<xsl:with-param name="Value" select="concat(@AvgRowSize, ' B')" />
			</xsl:call-template>
			<xsl:call-template name="ToolTipRow">
				<xsl:with-param name="Condition" select="(@StatementEstRows | @EstimateRows) and number(@AvgRowSize) > 0" />
				<xsl:with-param name="Label">Estimated Data Size</xsl:with-param>
				<xsl:with-param name="Value" select="fn:ToDataSize($dataSize)" />
			</xsl:call-template>

			<xsl:call-template name="ToolTipRow">
				<xsl:with-param name="Condition" select="@EstimateRebinds > 0" />
				<xsl:with-param name="Label">Estimated Rebinds</xsl:with-param>
				<xsl:with-param name="Value" select="@EstimateRebinds" />
			</xsl:call-template>
			<xsl:call-template name="ToolTipRow">
				<xsl:with-param name="Condition" select="@EstimateRewinds > 0" />
				<xsl:with-param name="Label">Estimated Rewinds</xsl:with-param>
				<xsl:with-param name="Value" select="@EstimateRewinds" />
			</xsl:call-template>
			
			<xsl:call-template name="ToolTipRow">
				<xsl:with-param name="Condition" select="s:IndexScan/@Ordered" />
				<xsl:with-param name="Label">Ordered</xsl:with-param>
				<xsl:with-param name="Value">
					<xsl:choose>
						<xsl:when test="s:IndexScan/@Ordered = 1">True</xsl:when>
						<xsl:otherwise>False</xsl:otherwise>
					</xsl:choose>
				</xsl:with-param>
			</xsl:call-template>
			<xsl:call-template name="ToolTipRow">
				<xsl:with-param name="Label">Node ID</xsl:with-param>
				<xsl:with-param name="Value" select="@NodeId" />
			</xsl:call-template>
		</table>
	</xsl:template>

	<!-- Calculates the estimated operator cost. -->
	<xsl:template name="EstimatedOperatorCost">
		<xsl:value-of select="fn:Max(fn:ToDecimal(string(@EstimatedTotalSubtreeCost)) - sum(./descendant::s:RelOp[1]/../s:RelOp/@EstimatedTotalSubtreeCost), 0)" />
	</xsl:template>

	<xsl:template name="StatementSubTreeCost">
		<xsl:value-of select="fn:ToDecimal(string(./ancestor::s:StmtSimple[1]/@StatementSubTreeCost))"/>
	</xsl:template>

	<!-- Renders a row in the tool tip details table. -->
	<xsl:template name="ToolTipRow">
		<xsl:param name="Label" />
		<xsl:param name="Value" />
		<xsl:param name="Condition" select="$Value" />
		<xsl:if test="$Condition">
			<tr><th><xsl:value-of select="$Label" /></th><td><label><xsl:attribute name="propertyType"><xsl:value-of select="$Label" /></xsl:attribute><xsl:value-of select="$Value" /></label></td></tr>
		</xsl:if>
	</xsl:template>

	<!-- Prints the name of an object. -->
	<xsl:template match="s:Object | s:ColumnReference" mode="ObjectName">
		<xsl:param name="ExcludeDatabaseName" select="false()" />
		<xsl:choose>
			<xsl:when test="$ExcludeDatabaseName">
				<xsl:for-each select="@Table | @Index | @Column | @Alias">
					<xsl:value-of select="." />
					<xsl:if test="position() != last()">.</xsl:if>
				</xsl:for-each>
			</xsl:when>
			<xsl:otherwise>
				<xsl:for-each select="@Database | @Schema | @Table | @Index | @Column | @Alias">
					<xsl:value-of select="." />
					<xsl:if test="position() != last()">.</xsl:if>
				</xsl:for-each>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- Displays the node cost label. -->
	<xsl:template match="s:RelOp" mode="NodeCostLabel">
		<xsl:variable name="EstimatedOperatorCost">
			<xsl:call-template name="EstimatedOperatorCost" />
		</xsl:variable>

		<xsl:variable name="TotalCost">
			<xsl:call-template name="StatementSubTreeCost" />
		</xsl:variable>
		<div>Cost: <xsl:value-of select="format-number($EstimatedOperatorCost div $TotalCost, '0.0%')" /></div>
	</xsl:template>

	
	<!-- Dont show the node cost for statements. -->
	<xsl:template match="s:StmtSimple|s:StmtCursor|s:StmtCond|s:StmtUseDb|s:StmtReceive" mode="NodeCostLabel" />

	<!-- 
		================================
		Tool tip detail sections
		================================
		The following section contains templates used for writing the detail sections at the bottom of the tool tip,
		for example listing outputs, or information about the object to which an operator applies.
	-->

	<xsl:template match="*/s:Object" mode="ToolTipDetails">
		<!-- TODO: Make sure this works all the time -->
		<div class="qp-bold qp-tt-details">Object</div>
		<div>
			<xsl:apply-templates select="." mode="ObjectName" />
		</div>
	</xsl:template>

	<xsl:template match="s:SetPredicate[s:ScalarOperator/@ScalarString]" mode="ToolTipDetails">
		<div class="qp-bold qp-tt-details">Predicate</div>
		<div class="qp-tt-largetext">
			<xsl:value-of select="s:ScalarOperator/@ScalarString" />
		</div>
	</xsl:template>

	<xsl:template match="s:OutputList[count(s:ColumnReference) > 0]" mode="ToolTipDetails">
		<div class="qp-bold qp-tt-details">Output List</div>
		<div class="qp-tt-largetext">
			<ul class="qp-tt-list">
				<xsl:for-each select="s:ColumnReference">
					<li>
						<xsl:apply-templates select="." mode="ObjectName" />
					</li>
				</xsl:for-each>
			</ul>
		</div>
	</xsl:template>

	<xsl:template match="s:NestedLoops/s:OuterReferences[count(s:ColumnReference) > 0]" mode="ToolTipDetails">
		<div class="qp-bold qp-tt-details">Outer References</div>
		<div class="qp-tt-largetext">
			<ul class="qp-tt-list">
				<xsl:for-each select="s:ColumnReference">
					<li>
						<xsl:apply-templates select="." mode="ObjectName" />
					</li>
				</xsl:for-each>
			</ul>
		</div>
	</xsl:template>

	<xsl:template match="@StatementText" mode="ToolTipDetails">
		<div class="qp-bold qp-tt-details">Statement</div>
		<div class="qp-tt-largetext">
			<xsl:value-of select="fn:AddSpaces(string(.))" />
		</div>
	</xsl:template>

	<xsl:template match="s:Sort/s:OrderBy[count(s:OrderByColumn/s:ColumnReference) > 0]" mode="ToolTipDetails">
		<div class="qp-bold qp-tt-details">Order By</div>
		<div class="qp-tt-largetext">
			<ul class="qp-tt-list">
				<xsl:for-each select="s:OrderByColumn">
					<li>
						<xsl:apply-templates select="s:ColumnReference" mode="ObjectName" />
						<xsl:choose>
							<xsl:when test="@Ascending = 1"> ASC</xsl:when>
							<xsl:otherwise> DESC</xsl:otherwise>
						</xsl:choose>
					</li>
				</xsl:for-each>
			</ul>
		</div>
	</xsl:template>

	<xsl:template match="sspa:Result/sspa:Errors" mode="ToolTipDetails">
		<div class="qp-bold qp-tt-details">Errors</div>
		<div class="qp-tt-largetext">
			<ul class="qp-tt-checks-list">
				<xsl:for-each select="sspa:Error">
					<li><xsl:apply-templates select="." mode="CheckDescription" /></li>
				</xsl:for-each>
			</ul>
		</div>
	</xsl:template>

	<xsl:template match="sspa:Result/sspa:Warnings" mode="ToolTipDetails">
		<div class="qp-bold qp-tt-details">Warnings</div>
		<div class="qp-tt-largetext">
			<ul class="qp-tt-checks-list">
				<xsl:for-each select="sspa:Warning">
					<li><xsl:apply-templates select="." mode="CheckDescription" /></li>
				</xsl:for-each>
			</ul>
		</div>
	</xsl:template>

	<!-- TODO: Seek Predicates -->

	<!-- 
	  ================================
	  Node icons
	  ================================
	  The following templates determine what icon should be shown for a given node
	  -->

	<!-- Use the logical operation to determine the icon for the "Parallelism" operators. -->
	<xsl:template match="s:RelOp[@PhysicalOp = 'Parallelism']" mode="NodeIcon" priority="1">
		<xsl:element name="div">
			<xsl:attribute name="class">qp-icon-<xsl:value-of select="translate(translate(@LogicalOp, ' ', ''), $uppercase, $lowercase)" /></xsl:attribute>
		</xsl:element>
	</xsl:template>

	<xsl:template match="s:RelOp[(contains(@PhysicalOp, 'Index Seek') or contains(@PhysicalOp, 'Index Scan')) and ./s:IndexScan[@Lookup = '1' or @Lookup = 'true']]" mode="NodeIcon" priority="1">
		<xsl:element name="div">
			<xsl:attribute name="class">qp-icon-keylookup</xsl:attribute>
		</xsl:element>
	</xsl:template>

	<!-- Use the physical operation to determine icon if it is present. -->
	<xsl:template match="*[@PhysicalOp]" mode="NodeIcon">
		<xsl:element name="div">
			<xsl:attribute name="class">qp-icon-<xsl:value-of select="translate(translate(@PhysicalOp, ' ', ''), $uppercase, $lowercase)" /></xsl:attribute>
		</xsl:element>
	</xsl:template>

	<!-- Matches all statements. -->
	<xsl:template match="s:StmtSimple|s:StmtUseDb|s:StmtReceive" mode="NodeIcon">
		<xsl:element name="div">
			<xsl:choose>
				<xsl:when test="./@StatementType = 'ASSIGN' or ./@StatementType = 'ASSIGN WITH QUERY'">
					<xsl:attribute name="class">qp-icon-assign</xsl:attribute>
				</xsl:when>
				<xsl:when test="./@StatementType = 'INSERT'">
					<xsl:attribute name="class">qp-icon-insert</xsl:attribute>
				</xsl:when>
				<xsl:when test="./@StatementType = 'UPDATE'">
					<xsl:attribute name="class">qp-icon-update</xsl:attribute>
				</xsl:when>
				<xsl:when test="./@StatementType = 'DELETE'">
					<xsl:attribute name="class">qp-icon-delete</xsl:attribute>
				</xsl:when>
				<xsl:otherwise>
					<xsl:attribute name="class">qp-icon-statement</xsl:attribute>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:element>
	</xsl:template>
	
	<xsl:template match="s:StmtCursor" mode="NodeIcon">
		<div class="qp-icon-cursorcatchall"/>
	</xsl:template>

	<xsl:template match="s:StmtCond" mode="NodeIcon">
		<xsl:choose>
			<xsl:when test="contains(translate(./@StatementText, $lowercase, $uppercase), 'WHILE')">
				<div class="qp-icon-while" />
			</xsl:when>
			<xsl:when test="contains(translate(./@StatementText, $lowercase, $uppercase), 'IF')">
				<div class="qp-icon-if" />
			</xsl:when>
			<xsl:otherwise>
				<div class="qp-icon-if" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- Fallback template - show the Bitmap icon. -->
	<xsl:template match="*" mode="NodeIcon">
		<div class="qp-icon-catchall" />
	</xsl:template>

	<!-- 
	  ================================
	  Node labels
	  ================================
	  The following section contains templates used to determine the first (main) label for a node.
	  -->

	<xsl:template match="s:RelOp[(@PhysicalOp = 'Clustered Index Seek' or @PhysicalOp = 'Clustered Index Scan') and ./s:IndexScan[@Lookup = '1' or @Lookup = 'true']]" mode="NodeLabel">
		<div>Key Lookup</div>
	</xsl:template>

	<xsl:template match="s:RelOp[(@PhysicalOp = 'Index Seek' or @PhysicalOp = 'Index Scan') and ./s:IndexScan[@Lookup = '1' or @Lookup = 'true']]" mode="NodeLabel">
		<div>Key Lookup</div>
	</xsl:template>

	<xsl:template match="s:RelOp" mode="NodeLabel">
		<div><xsl:value-of select="@PhysicalOp" /></div>
	</xsl:template>

	<xsl:template match="s:StmtCond" mode="NodeLabel">
		<div><xsl:choose>
				<xsl:when test="contains(translate(./@StatementText, $lowercase, $uppercase), 'WHILE')">WHILE</xsl:when>
				<xsl:when test="contains(translate(./@StatementText, $lowercase, $uppercase), 'IF')">IF</xsl:when>
				<xsl:otherwise><xsl:value-of select="@StatementType" /></xsl:otherwise>
			</xsl:choose></div>
	</xsl:template>

	<xsl:template match="s:StmtCursor|s:StmtUseDb|s:StmtReceive" mode="NodeLabel">
		<div><xsl:value-of select="@StatementType" /></div>
	</xsl:template>

	<xsl:template match="s:StmtSimple" mode="NodeLabel">
		<div><xsl:choose>
				<xsl:when test="contains(translate(./@StatementText, $lowercase, $uppercase), 'DROP TABLE')">DROP TABLE</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="@StatementType" />
				</xsl:otherwise>
			</xsl:choose></div>
		<xsl:choose>
			<xsl:when test="@StatementSubTreeCost >= 1000">
				<div class="qp-cost qp-highcost qp-bold">Cost: <xsl:value-of select="format-number(@StatementSubTreeCost,'#.##')" /></div>
			</xsl:when>
			<xsl:when test="@StatementSubTreeCost >= 200">
				<div class="qp-cost qp-mediumcost qp-bold">Cost: <xsl:value-of select="format-number(@StatementSubTreeCost,'#.##')" /></div>
			</xsl:when>
			<xsl:when test="@StatementSubTreeCost >= 0.005">
				<div class="qp-cost">Cost: <xsl:value-of select="format-number(@StatementSubTreeCost,'#.##')" /></div>
			</xsl:when>
			<xsl:otherwise></xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!--
	================================
	Node alternate labels
	================================
	The following section contains templates used to determine the second label to be displayed for a node.
	-->

	<!-- Display the object for any node that has one -->
	<xsl:template match="*[*/s:Object]" mode="NodeLabel2">
		<xsl:variable name="ObjectName">
			<xsl:apply-templates select="*/s:Object" mode="ObjectName">
				<xsl:with-param name="ExcludeDatabaseName" select="true()" />
			</xsl:apply-templates>
		</xsl:variable>
		<div><xsl:value-of select="substring($ObjectName, 0, 36)" />
			<xsl:if test="string-length($ObjectName) >= 36">…</xsl:if></div>
	</xsl:template>

	<!-- Display the logical operation for any node where it is not the same as the physical operation. -->
	<xsl:template match="s:RelOp[@LogicalOp != @PhysicalOp]" mode="NodeLabel2">
		<div>(<xsl:value-of select="@LogicalOp" />)</div>
	</xsl:template>

	<!-- Disable the default template -->
	<xsl:template match="*" mode="NodeLabel2" />

	<!-- 
	================================
	Analysis Checks Tool tip descriptions
	================================
	The following section contains templates used for writing the description shown in the tool tip.
	-->
	
	<xsl:template match="*[text() = 'MissingIndex']" mode="CheckDescription">Missing index warning</xsl:template>
	<xsl:template match="*[text() = 'KeyLookup' and local-name() = 'Warning']" mode="CheckDescription">Low Cost Key lookup</xsl:template>
	<xsl:template match="*[text() = 'KeyLookup' and local-name() = 'Error']" mode="CheckDescription">High Cost Key lookup</xsl:template>
	<xsl:template match="*[text() = 'IndexScan']" mode="CheckDescription">Index Scan of over <xsl:value-of select="/s:ShowPlanXML/sspa:ThreshHolds[1]/@highScanCount" /> rows</xsl:template>
	<xsl:template match="*[text() = 'ClusteredIndexScan']" mode="CheckDescription">Clustered Index Scan of over <xsl:value-of select="/s:ShowPlanXML/sspa:ThreshHolds[1]/@highScanCount" /> rows</xsl:template>
	<xsl:template match="*[text() = 'FatPipes']" mode="CheckDescription">A high row count difference of over <xsl:value-of select="format-number(/s:ShowPlanXML/sspa:ThreshHolds[1]/@fatPipeRowCount, '###,###')" /> rows found in the pipes</xsl:template>
	<xsl:template match="*[text() = 'MediumCostOperator']" mode="CheckDescription">High cost between <xsl:value-of select="/s:ShowPlanXML/sspa:ThreshHolds[1]/@mediumCostOperator" /> and <xsl:value-of select="/s:ShowPlanXML/sspa:ThreshHolds[1]/@highCostOperator" /> found</xsl:template>
	<xsl:template match="*[text() = 'HighCostOperator']" mode="CheckDescription">Very high cost over <xsl:value-of select="/s:ShowPlanXML/sspa:ThreshHolds[1]/@highCostOperator" /> found</xsl:template>
	<xsl:template match="*[text() = 'IndexSpool']" mode="CheckDescription">Index spool found</xsl:template>
	<xsl:template match="*[text() = 'TableSpool']" mode="CheckDescription">Table spool found</xsl:template>
	<xsl:template match="*[text() = 'HighDesiredMemoryKB']" mode="CheckDescription">Memory over <xsl:value-of select="number(/s:ShowPlanXML/sspa:ThreshHolds[1]/@highDesiredMemoryKB) div 1000 div 1000" /> GB requested</xsl:template>
	<xsl:template match="*[text() = 'HighRewindCount']" mode="CheckDescription">A high number of rewinds found over many rows</xsl:template>
	<xsl:template match="*[text() = 'RIDLookup']" mode="CheckDescription">RID or HEAP lookup</xsl:template>
	<xsl:template match="*[text() = 'NoLock']" mode="CheckDescription">NoLock hint found in query</xsl:template>
	<xsl:template match="*[text() = 'Cursor']" mode="CheckDescription">Cursor found</xsl:template>
	<xsl:template match="*[text() = 'CardinalityConvertIssue']" mode="CheckDescription">Conversion that affects cardinality</xsl:template>
	<xsl:template match="*[text() = 'HighCostSort']" mode="CheckDescription">Sort found that exceeds <xsl:value-of select="/s:ShowPlanXML/sspa:ThreshHolds[1]/@highSortCost" />% of query cost, or sorts over <xsl:value-of select="/s:ShowPlanXML/sspa:ThreshHolds[1]/@highSortCount" /> rows</xsl:template>
	<xsl:template match="*[text() = 'ImplicitConversion']" mode="CheckDescription">Implicit conversion found</xsl:template>
	<xsl:template match="*[text() = 'NoJoinPredicate']" mode="CheckDescription">Cross join. Verify that a cross join is desired here</xsl:template>
	<xsl:template match="*[text() = 'CompilationTimeout']" mode="CheckDescription">Query compilation timed out</xsl:template>
	<xsl:template match="*[text() = 'CompilationMemoryLimitExceeded']" mode="CheckDescription">Query compilation max memory exceeded</xsl:template>
	<xsl:template match="*[text() = 'HighNumberOfJoins' and local-name() = 'Warning']" mode="CheckDescription">High number of joins found &gt; <xsl:value-of select="/s:ShowPlanXML/sspa:ThreshHolds[1]/@highJoinCount" /></xsl:template>
	<xsl:template match="*[text() = 'HighNumberOfJoins' and local-name() = 'Error']" mode="CheckDescription">Very high number of joins found &gt; <xsl:value-of select="/s:ShowPlanXML/sspa:ThreshHolds[1]/@veryHighJoinCount" /></xsl:template>
	<xsl:template match="*[text() = 'JoinToTableValueFunction']" mode="CheckDescription">A join to a table valued function was found</xsl:template>
	<xsl:template match="*[text() = 'ColumnsWithNoStatistics']" mode="CheckDescription">A column has no statistics, but the query could possibly benefit from them</xsl:template>
	<xsl:template match="*[text() = 'SpillToTempDb']" mode="CheckDescription">A spill to temdb occurred</xsl:template>
	<xsl:template match="*[text() = 'UnmatchedIndexes']" mode="CheckDescription">An index was found, but not used</xsl:template>
	<xsl:template match="*[text() = 'PossibleNonparameterizedQuery']" mode="CheckDescription">Possible Non-Parameterized Query</xsl:template>

	
	<!--CheckDescription catchall-->
	<xsl:template match="*" mode="CheckDescription"><xsl:value-of select="./text()" /></xsl:template>

	
	<!-- 
	================================
	Tool tip descriptions
	================================
	The following section contains templates used for writing the description shown in the tool tip.
	-->

	<xsl:template match="*[@PhysicalOp = 'Arithmetic Expression']" mode="ToolTipDescription">The Arithmetic Expression operator computes a new value from existing values in a row.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Assert']" mode="ToolTipDescription">The Assert operator verifies a condition.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Assign'] | *[@StatementType='ASSIGN']" mode="ToolTipDescription">The Assign operator assigns the value of an expression or a constant to a variable. </xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Bitmap']" mode="ToolTipDescription">SQL Server uses the Bitmap operator to implement bitmap filtering in parallel query plans. Bitmap filtering speeds up query execution by eliminating rows with key values that cannot produce any join records before passing rows through another operator such as the Parallelism operator.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Bookmark Lookup']" mode="ToolTipDescription">The Bookmark Lookup operator uses a bookmark (row ID or clustering key) to look up the corresponding row in the table or clustered index. </xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Clustered Index Delete']" mode="ToolTipDescription">The Clustered Index Delete operator deletes rows from the clustered index specified in the Argument column of the query execution plan. </xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Clustered Index Insert']" mode="ToolTipDescription">The Clustered Index Insert Showplan operator inserts rows from its input into the clustered index specified in the Argument column. </xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Clustered Index Merge']" mode="ToolTipDescription">The Clustered Index Merge operator applies a merge data stream to a clustered index. The operator deletes, updates, or inserts rows from the clustered index specified in the Argument column of the operator.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Clustered Index Scan']" mode="ToolTipDescription">The Clustered Index Scan operator scans the clustered index specified in the Argument column of the query execution plan.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Clustered Index Seek']" mode="ToolTipDescription">
		<xsl:choose>
			<xsl:when test="./s:IndexScan[@Lookup = '1' or @Lookup = 'true']">A key lookup occurs when data is found in a non-clustered index, but additional data is needed from the clustered index to satisfy the query so the additional data has to be located in the clustered index.</xsl:when>
			<xsl:otherwise>The Clustered Index Seek operator uses the seeking ability of indexes to retrieve rows from a clustered index.</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Clustered Index Update']" mode="ToolTipDescription">The Clustered Index Update operator updates input rows in the clustered index specified in the Argument column.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Collapse']" mode="ToolTipDescription">The Collapse operator optimizes update processing. When an update is performed, it can be split (using the Split operator) into a delete and an insert.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Compute Scalar']" mode="ToolTipDescription">The Compute Scalar operator evaluates an expression to produce a computed scalar value. This may then be returned to the user, referenced elsewhere in the query, or both.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Concatenation']" mode="ToolTipDescription">The Concatenation operator scans multiple inputs, returning each row scanned. Concatenation is typically used to implement the Transact-SQL UNION ALL construct.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Constant Scan']" mode="ToolTipDescription">The Constant Scan operator introduces one or more constant rows into a query. A Compute Scalar operator is often used after a Constant Scan to add columns to a row produced by the Constant Scan operator.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Convert']" mode="ToolTipDescription">The Convert operator converts one scalar data type to another. </xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Declare']" mode="ToolTipDescription">The Declare Showplan operator allocates a local variable in the query plan. Declare is a language element.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Delete'] | *[@StatementType='DELETE']" mode="ToolTipDescription">The Delete operator deletes from an object rows that satisfy the optional predicate in the Argument column.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Deleted Scan']" mode="ToolTipDescription">The Deleted Scan operator scans the deleted table within a trigger.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Distribute Streams']" mode="ToolTipDescription">The Distribute Streams operator takes a single input stream of records and produces multiple output streams.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Dynamic']" mode="ToolTipDescription">The Dynamic operator uses a cursor that can see all changes made by others.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Eager Spool']" mode="ToolTipDescription">The Eager Spool operator takes the entire input, storing each row in a hidden temporary object stored in the tempdb database. If the operator is rewound (for example, by a Nested Loops operator) but no rebinding is needed, the spooled data is used instead of rescanning the input. If rebinding is needed, the spooled data is discarded and the spool object is rebuilt by rescanning the (rebound) input.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Fetch Query']" mode="ToolTipDescription">The Fetch Query operator retrieves rows when a fetch is issued against a cursor.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Filter']" mode="ToolTipDescription">The Filter operator scans the input, returning only those rows that satisfy the filter expression (predicate) that appears in the Argument column.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Gather Streams']" mode="ToolTipDescription">The Gather Streams operator is only used in parallel query plans. The Gather Streams operator consumes several input streams and produces a single output stream of records by combining the input streams.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Hash Match']" mode="ToolTipDescription">The Hash Match operator builds a hash table by computing a hash value for each row from its build input. A HASH:() predicate with a list of columns used to create a hash value appears in the Argument column. Then, for each probe row (as applicable), it computes a hash value (using the same hash function) and looks in the hash table for matches.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Hash Match Root']" mode="ToolTipDescription">The Hash Match Root operator coordinates the operation of all Hash Match Team operators directly below it. The Hash Match Root operator and all Hash Match Team operators directly below it share a common hash function and partitioning strategy.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Hash Match Team']" mode="ToolTipDescription">The Hash Match Team operator is part of a team of connected hash operators sharing a common hash function and partitioning strategy.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'If']" mode="ToolTipDescription">The If operator carries out conditional processing based on an expression.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Insert'] | *[@StatementType='INSERT']" mode="ToolTipDescription">The Insert logical operator inserts each row from its input into the object specified in the Argument column. The physical operator is either the Table Insert, Index Insert, or Clustered Index Insert operator.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Inserted Scan']" mode="ToolTipDescription">The Inserted Scan operator scans the inserted table within a trigger.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Intrinsic']" mode="ToolTipDescription">The Intrinsic operator invokes an internal Transact-SQL function.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Key Lookup']" mode="ToolTipDescription"> The Key Lookup operator is a bookmark lookup on a table with a clustered index.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Keyset']" mode="ToolTipDescription">The Keyset operator uses a cursor that can see updates, but not inserts made by others.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Lazy Spool']" mode="ToolTipDescription">The Lazy Spool logical operator stores each row from its input in a hidden temporary object stored in the tempdb database. If the operator is rewound (for example, by a Nested Loops operator) but no rebinding is needed, the spooled data is used instead of rescanning the input. If rebinding is needed, the spooled data is discarded and the spool object is rebuilt by rescanning the (rebound) input.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Log Row Scan']" mode="ToolTipDescription">The Log Row Scan operator scans the transaction log.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Merge Interval']" mode="ToolTipDescription">The Merge Interval operator merges multiple (potentially overlapping) intervals to produce minimal, nonoverlapping intervals that are then used to seek index entries. </xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Merge Join']" mode="ToolTipDescription">The Merge Join operator performs the inner join, left outer join, left semi join, left anti semi join, right outer join, right semi join, right anti semi join, and union logical operations.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Nested Loops']" mode="ToolTipDescription">The Nested Loops operator performs the inner join, left outer join, left semi join, and left anti semi join logical operations.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Nonclustered Index Delete']" mode="ToolTipDescription">The Nonclustered Index Delete operator deletes input rows from the nonclustered index specified in the Argument column.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Index Insert']" mode="ToolTipDescription">The Index Insert operator inserts rows from its input into the nonclustered index specified in the Argument column.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Index Scan']" mode="ToolTipDescription">The Index Scan operator retrieves all rows from the nonclustered index specified in the Argument column.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Index Seek']" mode="ToolTipDescription">The Index Seek operator uses the seeking ability of indexes to retrieve rows from a nonclustered index.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Index Spool']" mode="ToolTipDescription">The Index Spool physical operator contains a SEEK:() predicate in the Argument column. The Index Spool operator scans its input rows, placing a copy of each row in a hidden spool file (stored in the tempdb database and existing only for the lifetime of the query), and builds a nonclustered index on the rows.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Nonclustered Index Update']" mode="ToolTipDescription">The Nonclustered Index Update physical operator updates rows from its input in the nonclustered index specified in the Argument column. If a SET:() predicate is present, each updated column is set to this value.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Online Index Insert']" mode="ToolTipDescription">SQL Server permits the index operations CREATE, DROP, and ALTER to occur while the underlying table data remains available to users. Online Index Insert implements these operations.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Parallelism']" mode="ToolTipDescription">The Parallelism operator performs the distribute streams, gather streams, and repartition streams logical operations.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Parameter Table Scan']" mode="ToolTipDescription">The Parameter Table Scan operator scans a table that is acting as a parameter in the current query. </xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Population Query']" mode="ToolTipDescription">The Population Query operator populates the work table of a cursor when the cursor is opened.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Refresh Query']" mode="ToolTipDescription">The Refresh Query operator fetches current data for rows in the fetch buffer.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Remote Delete']" mode="ToolTipDescription">The Remote Delete operator deletes the input rows from a remote object.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Remote Index Seek']" mode="ToolTipDescription">The Remote Index Seek operator uses the seeking ability of a remote index object to retrieve rows.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Remote Index Scan']" mode="ToolTipDescription">The Remote Index Scan operator scans the remote index specified in the Argument column.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Remote Insert']" mode="ToolTipDescription">The Remote Insert operator inserts the input rows into a remote object.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Remote Query']" mode="ToolTipDescription">The Remote Query operator submits a query to a remote source.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Remote Scan']" mode="ToolTipDescription">The Remote Scan operator scans a remote object.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Remote Update']" mode="ToolTipDescription">The Remote Update operator updates the input rows in a remote object.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Repartition Streams']" mode="ToolTipDescription">The Repartition Streams operator consumes multiple streams and produces multiple streams of records. The record contents and format are not changed. If the query optimizer uses a bitmap filter, the number of rows in the output stream is reduced. Each record from an input stream is placed into one output stream. If this operator is order preserving, all input streams must be ordered and merged into several ordered output streams.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Result']" mode="ToolTipDescription">The Result operator is the data returned at the end of a query plan.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'RID Lookup']" mode="ToolTipDescription">RID Lookup is a bookmark lookup on a heap using a supplied row identifier (RID).</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Row Count Spool']" mode="ToolTipDescription">The Row Count Spool operator scans the input, counting how many rows are present and returning the same number of rows without any data in them. This operator is used when it is important to check for the existence of rows, rather than the data contained in the rows.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Select'] | *[@StatementType='SELECT']" mode="ToolTipDescription">Retrieves rows from the database and enables the selection of one or many rows or columns from one or many tables in SQL Server.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Select Into'] | *[@StatementType='SELECT INTO']" mode="ToolTipDescription">SELECT…INTO creates a new table in the default filegroup and inserts the resulting rows from the query into it. </xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Segment']" mode="ToolTipDescription">Segment is a physical and a logical operator. It divides the input set into segments based on the value of one or more columns.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Sequence']" mode="ToolTipDescription">The Sequence operator drives wide update plans. Functionally, it executes each input in sequence (top to bottom). Each input is usually an update of a different object. It returns only those rows that come from its last (bottom) input.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Sequence Project']" mode="ToolTipDescription">The Sequence Project operator adds columns to perform computations over an ordered set. It divides the input set into segments based on the value of one or more columns. The operator then outputs one segment at a time. These columns are shown as arguments in the Sequence Project operator.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Snapshot']" mode="ToolTipDescription">The Snapshot operator creates a cursor that does not see changes made by others.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Sort']" mode="ToolTipDescription">The Sort operator sorts all incoming rows.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Split']" mode="ToolTipDescription">The Split operator is used to optimize update processing. It splits each update operation into a delete and an insert operation.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Spool']" mode="ToolTipDescription">The Spool operator saves an intermediate query result to the tempdb database.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Stream Aggregate']" mode="ToolTipDescription">The Stream Aggregate operator groups rows by one or more columns and then calculates one or more aggregate expressions returned by the query.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Switch']" mode="ToolTipDescription">Switch is a special type of concatenation iterator that has n inputs. An expression is associated with each Switch operator. Depending on the return value of the expression (between 0 and n-1), Switch copies the appropriate input stream to the output stream.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Table Delete']" mode="ToolTipDescription">The Table Delete physical operator deletes rows from the table specified in the Argument column of the query execution plan.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Table Insert']" mode="ToolTipDescription">The Table Insert operator inserts rows from its input into the table specified in the Argument column of the query execution plan.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Table Merge']" mode="ToolTipDescription">The Table Merge operator applies a merge data stream to a heap. The operator deletes, updates, or inserts rows in the table specified in the Argument column of the operator.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Table Scan']" mode="ToolTipDescription">The Table Scan operator retrieves all rows from the table specified in the Argument column of the query execution plan.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Table Spool']" mode="ToolTipDescription">The Table Spool operator scans the input and places a copy of each row in a hidden spool table that is stored in the tempdb database and existing only for the lifetime of the query. If the operator is rewound (for example, by a Nested Loops operator) but no rebinding is needed, the spooled data is used instead of rescanning the input.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Table Update']" mode="ToolTipDescription">The Table Update physical operator updates input rows in the table specified in the Argument column of the query execution plan.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Table-valued Function']" mode="ToolTipDescription">The Table-valued Function operator evaluates a table-valued function (either Transact-SQL or CLR), and stores the resulting rows in the tempdb database. When the parent iterators request the rows, Table-valued Function returns the rows from tempdb.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Top']" mode="ToolTipDescription">The Top operator scans the input, returning only the first specified number or percent of rows, possibly based on a sort order.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'UDX']" mode="ToolTipDescription">Extended Operators (UDX) implement one of many XQuery and XPath operations in SQL Server.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'Update'] | *[@StatementType='UPDATE']" mode="ToolTipDescription">The Update operator updates each row from its input in the object specified in the Argument column of the query execution plan. Update is a logical operator. The physical operator is Table Update, Index Update, or Clustered Index Update.</xsl:template>
	<xsl:template match="*[@PhysicalOp = 'While']" mode="ToolTipDescription">The While operator implements the Transact-SQL while loop.</xsl:template>

	<xsl:template match="*[@StatementType='COND']" mode="ToolTipDescription">
		<xsl:choose>
			<xsl:when test="contains(translate(./@StatementText, $lowercase, $uppercase), 'WHILE')">The While operator implements the Transact-SQL while loop.</xsl:when>
			<xsl:when test="contains(translate(./@StatementText, $lowercase, $uppercase), 'IF')">The If operator carries out conditional processing based on an expression.</xsl:when>
			<xsl:otherwise>A conditional operator.</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<xsl:template match="*[@StatementType='DECLARE CURSOR']" mode="ToolTipDescription">Defines the attributes of a Transact-SQL server cursor, such as its scrolling behavior and the query used to build the result set on which the cursor operates. DECLARE CURSOR accepts both a syntax based on the ISO standard and a syntax using a set of Transact-SQL extensions.</xsl:template>
	<xsl:template match="*[@StatementType='OPEN CURSOR']" mode="ToolTipDescription">Opens a Transact-SQL server cursor and populates the cursor by executing the Transact-SQL statement specified on the DECLARE CURSOR or SET cursor_variable statement.</xsl:template>
	<xsl:template match="*[@StatementType='FETCH CURSOR']" mode="ToolTipDescription">Retrieves a specific row from a Transact-SQL server cursor.</xsl:template>
	<xsl:template match="*[@StatementType='CLOSE CURSOR']" mode="ToolTipDescription">Closes an open cursor by releasing the current result set and freeing any cursor locks held on the rows on which the cursor is positioned.</xsl:template>
	<xsl:template match="*[@StatementType='DEALLOCATE CURSOR']" mode="ToolTipDescription">Removes a cursor reference. When the last cursor reference is deallocated, the data structures comprising the cursor are released by Microsoft SQL Server.</xsl:template>

	<xsl:template match="*[@StatementType='DROP OBJECT']" mode="ToolTipDescription">
		<xsl:choose>
			<xsl:when test="contains(translate(./@StatementText, $lowercase, $uppercase), 'DROP TABLE')">Removes one or more table definitions and all data, indexes, triggers, constraints, and permission specifications for those tables.</xsl:when>
			<xsl:otherwise>A drop object operator.</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<xsl:template match="*[@StatementType='CREATE OBJECT']" mode="ToolTipDescription">
		<xsl:choose>
			<xsl:when test="contains(translate(./@StatementText, $lowercase, $uppercase), 'CREATE TABLE')">Creates a new table in SQL Server.</xsl:when>
			<xsl:otherwise>A drop object operator.</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- 
	  ================================
	  Number handling
	  ================================
	  The following section contains templates used for handling numbers (scientific notation, rounding etc...)
	  -->

	<!-- Outputs a number rounded to 7 decimal places - to be used for displaying all numbers.
	  This template accepts numbers in scientific notation. -->
	<xsl:template name="round">
		<xsl:param name="value" select="0" />
		<xsl:variable name="number">
			<xsl:call-template name="convertSciToNumString">
				<xsl:with-param name="inputVal" select="$value" />
			</xsl:call-template>
		</xsl:variable>
		<xsl:value-of select="round(number($number) * 10000000) div 10000000" />
	</xsl:template>

	<!-- Template for handling of scientific numbers
	See: http://www.orm-designer.com/article/xslt-convert-scientific-notation-to-decimal-number -->
	<xsl:variable name="max-exp">
		<xsl:value-of select="'0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000'" />
	</xsl:variable>

	<!--https://msdn.microsoft.com/en-us/library/ms256042(v=vs.100).aspx-->
	<msxsl:script language="JScript" implements-prefix="fn">
		function AddSpaces(val) {
			//add a space after any comma found with characters immediately after the comma so the tool tip text can be wrapped properly
			return val.replace(/([,\(\)])([A-Za-z\[])/gm, "$1 $2");
		}
		function Max(num1, num2){
			if(!num1) { num1 = 0; }
			if(!num2) { num2 = 0; }
			return Math.max(num1, num2);
		}
		function ToDecimal(num1){
			if(!num1) { num1 = '0'; }
			num1 = String(num1).replace(',', '');
			return parseFloat(num1).toFixed(20);
		}
		function ToDataSize(num1){
			num1 = ToDecimal(num1);
			//1,024 Gigabytes = 1 Terabyte. 1,024 Terabytes = 1 Petabyte. 1,024 Petabytes = 1 Exabyte (In 2000, 3 exabytes of information was created.) 1,024 Exabytes = 1 Zettabyte.
			if (num1 >= 1125899906842624) {
				return (num1 / 1125899906842624).toFixed(2) + ' PB';
			} else if (num1 >= 1099511627776) {
				return (num1 / 1099511627776).toFixed(2) + ' TB';
			} else if (num1 >= 1073741824) {
				return (num1 / 1073741824).toFixed(2) + ' GB';
			} else if (num1 >= 1048576) {
				return (num1 / 1048576).toFixed(2) + ' MB';
			} else if (num1 >= 1024) {
				return (num1 / 1024).toFixed(2) + ' KB';
			} else {
				return parseFloat(num1).toFixed(2) + ' B';
			}
		}
		function Round(num1, len){
			if(!len) { len = 0; }
			num1 = ToDecimal(num1);
			return parseFloat(num1).toFixed(len);
		}
		function Abs(num1){
			if(!num1) { num1 = 0; }
			num1 = ToDecimal(num1);
			return Math.abs(num1);
		}
	</msxsl:script>

	<xsl:template name="convertSciToNumString">
		<xsl:param name="inputVal" select="0" />

		<xsl:variable name="numInput">
			<xsl:value-of select="translate(string($inputVal),'e','E')" />
		</xsl:variable>

		<xsl:choose>
			<xsl:when test="number($numInput) = $numInput">
				<xsl:value-of select="$numInput" />
			</xsl:when>
			<xsl:otherwise>
				<!-- ==== Mantisa ==== -->
				<xsl:variable name="numMantisa">
					<xsl:value-of select="number(substring-before($numInput,'E'))" />
				</xsl:variable>

				<!-- ==== Exponent ==== -->
				<xsl:variable name="numExponent">
					<xsl:choose>
						<xsl:when test="contains($numInput,'E+')">
							<xsl:value-of select="substring-after($numInput,'E+')" />
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="substring-after($numInput,'E')" />
						</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>

				<!-- ==== Coefficient ==== -->
				<xsl:variable name="numCoefficient">
					<xsl:choose>
						<xsl:when test="$numExponent > 0">
							<xsl:text>1</xsl:text>
							<xsl:value-of select="substring($max-exp, 1, number($numExponent))" />
						</xsl:when>
						<xsl:when test="$numExponent &lt; 0">
							<xsl:text>0.</xsl:text>
							<xsl:value-of select="substring($max-exp, 1, -number($numExponent)-1)" />
							<xsl:text>1</xsl:text>
						</xsl:when>
						<xsl:otherwise>1</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<xsl:value-of select="number($numCoefficient) * number($numMantisa)" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
</xsl:stylesheet>
