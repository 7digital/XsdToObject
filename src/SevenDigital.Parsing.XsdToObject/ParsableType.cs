using System;

namespace SevenDigital.Parsing.XsdToObject
{
	public class ParsableType
	{
		private readonly string _parseCallFormat;

		public ParsableType(string xmlTypeName, string netTypeName, Type type)
		{
			XmlTypeName = xmlTypeName;
			NetTypeName = netTypeName;
			_parseCallFormat = FormatParseCall(type);
		}

		private string FormatParseCall(Type type)
		{
			var formatParseCall = type.GetMethod("Parse", new[] { typeof(string), typeof(IFormatProvider) }) != null
				? ".Parse({0}, CultureInfo.InvariantCulture)"
				: ".Parse({0})";
			return NetTypeName.TrimEnd('?') + formatParseCall;
		}

		public string XmlTypeName { get; private set; }
		public string NetTypeName { get; private set; }
		public string ConstructParseCall(string value)
		{
			return string.Format(_parseCallFormat, value);
		}
	}
}