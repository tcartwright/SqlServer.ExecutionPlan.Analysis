// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis
// Author           : tdcart
// Created          : 07-13-2016
//
// Last Modified By : tdcart
// Last Modified On : 07-13-2016
// ***********************************************************************
// <copyright file="XElementExtensionMethods.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Xml.Linq;

namespace SqlServer.ExecutionPlan.Analysis.Internals
{
	/// <summary>
	/// Class XElementExtensionMethods.
	/// </summary>
	public static class XElementExtensionMethods
	{
		/// <summary>
		/// Attributes the string.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns>System.String.</returns>
		public static string AttributeString(this XElement element, XName attributeName, string defaultValue)
		{
			if (element == null) { return defaultValue; }
			var a = element.Attribute(attributeName);

			return a == null ? defaultValue : a.Value;
		}
		/// <summary>
		/// Attributes the int32.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns>System.Int32.</returns>
		public static int AttributeInt32(this XElement element, XName attributeName, int defaultValue)
		{
			if (element == null) { return defaultValue; }
			var a = element.Attribute(attributeName);

			if (a == null) { return defaultValue; }

			double result;
			return double.TryParse(a.Value, out result) ? Convert.ToInt32(result) : defaultValue;
		}

		/// <summary>
		/// Attributes the int64.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns>Int64.</returns>
		public static Int64 AttributeInt64(this XElement element, XName attributeName, Int64 defaultValue)
		{
			if (element == null) { return defaultValue; }
			var a = element.Attribute(attributeName);

			if (a == null) { return defaultValue; }

			double result;
			return double.TryParse(a.Value, out result) ? Convert.ToInt64(result) : defaultValue;
		}

		/// <summary>
		/// Attributes the double.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns>System.Double.</returns>
		public static double AttributeDouble(this XElement element, XName attributeName, double defaultValue)
		{
			if (element == null) { return defaultValue; }
			var a = element.Attribute(attributeName);

			if (a == null) { return defaultValue; }

			double result;
			return double.TryParse(a.Value, out result) ? result : defaultValue;
		}

		/// <summary>
		/// Gets the element.
		/// </summary>
		/// <param name="el">The el.</param>
		/// <param name="names">The names.</param>
		/// <returns>XElement.</returns>
		/// <exception cref="System.ArgumentNullException">el</exception>
		public static XElement GetElement(this XElement el, params XName[] names)
		{
			if (el == null) { throw new ArgumentNullException("el"); }
			if (names == null || names.Length == 0) { return el; }
			foreach (var name in names)
			{
				el = el.Element(name);
				if (el == null) { break; }
			}
			return el;
		}

	}
}
