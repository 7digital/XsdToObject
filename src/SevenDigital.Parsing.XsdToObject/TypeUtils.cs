using System;
using System.Collections.Generic;

namespace SevenDigital.Parsing.XsdToObject
{
	public static class TypeUtils
	{
		private static readonly IDictionary<string, ParsableType> _parsableTypesMap =
			new Dictionary<string, ParsableType>(StringComparer.OrdinalIgnoreCase);

		static TypeUtils()
		{
			AddType("boolean", "bool?", typeof(bool));
			AddType("integer", "int?", typeof(int));
			AddType("int", "int?", typeof(int));
			AddType("decimal", "decimal?", typeof(decimal));
			AddType("datetime", "DateTime?", typeof(DateTime));
			AddType("date", "DateTime?", typeof(DateTime));
		}

		private static void AddType(string xmlTypeName, string codeTypeName, Type netType)
		{
			_parsableTypesMap.Add(xmlTypeName, new ParsableType(xmlTypeName, codeTypeName, netType));
		}

		public static bool IsParsable(string xmlType)
		{
			return _parsableTypesMap.ContainsKey(xmlType);
		}

		public static ParsableType GetParsableType(string xmlType)
		{
			return _parsableTypesMap[xmlType];
		}

		public static string ToNetTypeName(string xmlType)
		{
			ParsableType result;
			return _parsableTypesMap.TryGetValue(xmlType.ToLower(), out result)
				? result.NetTypeName
				: xmlType;
		}

		public static bool IsSimpleType(string xmlType)
		{
			return xmlType.ToLower() == "string" || IsParsable(xmlType);
		}
	}
}