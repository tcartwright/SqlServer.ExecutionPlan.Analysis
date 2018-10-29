// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 06-29-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-04-2016
// ***********************************************************************
// <copyright file="PlanTransformer.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using SqlServer.ExecutionPlan.Analysis.Properties;

namespace SqlServer.ExecutionPlan.Analysis
{
	/// <summary>
	/// Class PlanTransformer.
	/// </summary>
	public static class PlanTransformer
	{
		/// <summary>
		/// Extracts the resources.
		/// </summary>
		/// <param name="path">The path.</param>
		public static void ExtractResources(string path)
		{
			File.WriteAllText(Path.Combine(path, "jquery.min.js"), Resources.jquery_min);
			File.WriteAllText(Path.Combine(path, "jquery-ui.min.js"), Resources.jquery_ui_min);
			File.WriteAllText(Path.Combine(path, "qp.css"), Resources.qp_css);
			File.WriteAllText(Path.Combine(path, "qp.js"), Resources.qp_js);
			File.WriteAllText(Path.Combine(path, "qp-tooltip.js"), Resources.qp_tooltip);
		}


		/// <summary>
		/// Transforms the HTML fragment.
		/// </summary>
		/// <param name="planAnalysisXml">The plan analysis XML.</param>
		/// <returns>System.String.</returns>
		public static string TransformHtmlFragment(string planAnalysisXml)
		{
			return TransformHtmlFragment(XElement.Parse(planAnalysisXml));
		}

		/// <summary>
		/// Transforms the HTML fragment.
		/// </summary>
		/// <param name="planAnalysis">The plan analysis.</param>
		/// <returns>System.String.</returns>
		/// <exception cref="System.ArgumentNullException">planAnalysis</exception>
		public static string TransformHtmlFragment(XNode planAnalysis)
		{
			if (planAnalysis == null) { throw new ArgumentNullException("planAnalysis"); }

			using (var results = new StringWriter())
			using (var reader = XmlReader.Create(new StringReader(Resources.qp_xslt)))
			{
				// Load the style sheet.
				var xslt = new XslCompiledTransform();
				xslt.Load(reader, XsltSettings.TrustedXslt, new XmlUrlResolver());

				// Execute the transform and output the results to a writer.
				xslt.Transform(planAnalysis.CreateReader(), null, results);
				return results.ToString();
			}
		}

		/// <summary>
		/// Transforms the HTML page.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="planAnalysisXml">The plan analysis XML.</param>
		/// <returns>System.String.</returns>
		public static string TransformHtmlPage(string name, string planAnalysisXml)
		{
			return TransformHtmlPage(name, XElement.Parse(planAnalysisXml));
		}

		/// <summary>
		/// Transforms the HTML page.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="planAnalysis">The plan analysis.</param>
		/// <returns>System.String.</returns>
		public static string TransformHtmlPage(string name, XNode planAnalysis)
		{
			var body = TransformHtmlFragment(planAnalysis);

			name = GetName(name);

			if (name.Length > 0) { name += " "; }

			return Resources.AnalysisTemplate.Replace("{name}", name).Replace("{body}", body);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private static string GetName(string name)
		{
			name = (name ?? "").Trim();

			if (string.IsNullOrWhiteSpace(name)) { return name; }

			try
			{
				var fi = new FileInfo(name);
				if (!string.IsNullOrWhiteSpace(fi.Extension) && (name.IndexOf(Path.VolumeSeparatorChar) > 0 || name.IndexOf(Path.PathSeparator) > 0))
				{
					name = Path.GetFileNameWithoutExtension(name);
				}
				return name;
			}
			catch (Exception)
			{
				return name;
			}
		}
	}
}
