using System;
using System.IO;

namespace SevenDigital.Parsing.XsdToObject
{
	public class PropertySetter
	{
		readonly StreamWriter _writer;

		public PropertySetter(StreamWriter writer)
		{
			_writer = writer;
		}

		public void GenerateParseConstructor(ClassInfo classInfo)
		{
			_writer.WriteLine("\t\tpublic {0}(XElement element){1}\t\t{{", classInfo.GetCodeName(), Environment.NewLine);
			foreach (PropertyInfo property in classInfo.Elements)
			{
				var lastMethod = "SingleOrDefault()";
				if (property.IsList)
					lastMethod = "ToList()";
				else if (property.BindedType != null)
					lastMethod = String.Format("SingleOrDefault() ?? new Null{0}()", property.GetCodeType());

				GeneratePropertyInitialization(property, lastMethod);
			}
			foreach (PropertyInfo attr in classInfo.Attributes)
			{
				GenerateAttributeInitialization(attr, _writer);
			}
			_writer.WriteLine("\t\t}");
		}

		void GenerateAttributeInitialization(PropertyInfo attr, StreamWriter streamWriter)
		{
			if (attr.IsParsable)
			{
				InitialiseByParse(attr, "SingleOrDefault()","Attributes");
			}
			else
			{
				streamWriter.WriteLine("\t\t\t{0} = element.Attributes().Where(a => a.Name == \"{1}\").Select(a => a.Value).SingleOrDefault();",
					NameUtils.ToCodeName(attr.XmlName, false), attr.XmlName);
			}
			
		}

		void GeneratePropertyInitialization(PropertyInfo property, string chainLastMethod)
		{
			if (property.IsElementValue)
			{
				_writer.WriteLine("\t\t\t{0} = element.Value;",
					property.GetCodeName());
			}
			else
			{
				if (property.IsParsable) InitialiseByParse(property, chainLastMethod, "Elements");
				else InitialiseString(property, chainLastMethod);
			}
		}

		void InitialiseByParse(PropertyInfo property, string chainLastMethod, string propertyListName)
		{
			_writer.WriteLine("\t\t\t{0} = {4}.Parse(element.{5}().Where(e => e.Name == \"{1}\").Select(e => {2}).{3});",
				property.GetCodeName(),
				property.XmlName,
				XElementToValue(property, "e"),
				chainLastMethod,
				property.XmlType,
				propertyListName);
		}

		void InitialiseString(PropertyInfo property, string chainLastMethod)
		{
			_writer.WriteLine("\t\t\t{0} = element.Elements().Where(e => e.Name == \"{1}\").Select(e => {2}).{3};",
				property.GetCodeName(),
				property.XmlName,
				XElementToValue(property, "e"),
				chainLastMethod);
		}

		static string XElementToValue(PropertyInfo property, string varName)
		{
			return property.BindedType != null
				? String.Format("new {0}({1})", property.BindedType.GetCodeName(), varName)
				: String.Format("{0}.Value", varName);
		}
	}
}