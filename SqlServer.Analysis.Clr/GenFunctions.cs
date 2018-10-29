using System;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;
using SqlServer.ExecutionPlan.Analysis;
using Microsoft.SqlServer.Server;
// ReSharper disable InconsistentNaming
// ReSharper disable LocalizableElement
// ReSharper disable CheckNamespace
// ReSharper disable once PartialTypeWithSinglePart

public partial class UserDefinedFunctions
{
	[SqlFunction(IsDeterministic = true, SystemDataAccess = SystemDataAccessKind.None)]
	public static SqlXml xfnCheckPlanXml(
		[SqlFacet(MaxSize = 512, IsNullable = true)]
		SqlString validationConfig,
		[SqlFacet(MaxSize = 128, IsNullable = false)]
		SqlString dbName,
		[SqlFacet(MaxSize = -1, IsNullable = true)]
		SqlXml planXml
	)
	{
		if (planXml.IsNull) { return SqlXml.Null; }

		var xml = planXml.Value;
		string config = null;
		if (!validationConfig.IsNull) { config = validationConfig.Value; }

		var planValidator = new PlanValidator(config);
		var results = planValidator.ValidateSqlPlan(xml).ToList();

		// ReSharper disable once SimplifyLinqExpression
		if (!results.Any(r => r.Category != PlanCategory.Trace))
		{
			return SqlXml.Null;
		}

		var resultElement = planValidator.GenerateResultsElement(results);
		resultElement.Add(new XAttribute("dbName", dbName.Value));

		SqlXml ret;

		using (var reader = new StringReader(resultElement.ToString()))
		using (var xmlreader = new XmlTextReader(reader))
		{
			ret = new SqlXml(xmlreader);
		}
		return ret;
	}

	[SqlFunction(IsDeterministic = true, SystemDataAccess = SystemDataAccessKind.None)]
	public static SqlBinary xfnComputeHash(
		[SqlFacet(MaxSize = 16, IsNullable = false)]
		SqlString algorithm,
		[SqlFacet(MaxSize = -1, IsNullable = true)]
		SqlBytes value
	)
	{
		if (value.IsNull) { return SqlBinary.Null; }

		switch (algorithm.Value.ToUpperInvariant())
		{
			case "MD5":
				return new SqlBinary(MD5.Create().ComputeHash(value.Stream));
			case "SHA1":
				return new SqlBinary(SHA1.Create().ComputeHash(value.Stream));
			case "SHA2_256":
				return new SqlBinary(SHA256.Create().ComputeHash(value.Stream));
			case "SHA2_512":
				return new SqlBinary(SHA512.Create().ComputeHash(value.Stream));
			default:
				throw new ArgumentException("HashType", "Unrecognized hashtype: " + algorithm.Value);
		}
	}

}
