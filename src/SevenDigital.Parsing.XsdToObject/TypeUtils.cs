using System.Collections.Generic;

namespace SevenDigital.Parsing.XsdToObject
{
	public static class TypeUtils
	{
		private static readonly IDictionary<string, string> _parsableTypesMap = new Dictionary<string, string>
		{
			{"boolean","bool?"},
			{"integer","int?"},
			{"int","int?"},
			{"decimal","decimal?"},
			{"dateTime","DateTime?"},
			{"date","DateTime?"}
		};

		public static bool IsParsable(string xmlType)
		{
			return _parsableTypesMap.ContainsKey(xmlType.ToLower());
		}

		public static string ToCodeType(string xmlType)
		{
			string result;
			return _parsableTypesMap.TryGetValue(xmlType.ToLower(), out result)
				       ? result
				       : xmlType;
		}

		public static bool IsSimpleType(string xmlType)
		{
			return xmlType.ToLower() == "string" || IsParsable(xmlType);
		}
	}
}