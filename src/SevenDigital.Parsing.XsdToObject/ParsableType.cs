using System;
using System.Collections.Generic;

namespace SevenDigital.Parsing.XsdToObject
{
	public class ParsableType
	{
		private static readonly Dictionary<Type, string> _customParsingCalls = new Dictionary<Type, string>
		{
			{typeof(DateTime), "XmlConvert.ToDateTime({0}, XmlDateTimeSerializationMode.RoundtripKind)"},
			{typeof(Int32), "XmlConvert.ToInt32({0})"},
			{typeof(Boolean), "XmlConvert.ToBoolean({0})"},
			{typeof(Decimal), "XmlConvert.ToDecimal({0})"},
		};

		private readonly string _parseCallFormat;

		public ParsableType(string xmlTypeName, string netTypeName, Type type)
		{
			XmlTypeName = xmlTypeName;
			NetTypeName = netTypeName;
			_parseCallFormat = FormatParseCall(type);
		}

		private string FormatParseCall(Type type)
		{
			if (_customParsingCalls.ContainsKey(type))
				return _customParsingCalls[type];

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