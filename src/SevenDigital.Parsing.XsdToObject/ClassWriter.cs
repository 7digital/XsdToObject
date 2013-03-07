using System;
using System.IO;
using System.Linq;

namespace SevenDigital.Parsing.XsdToObject
{
	public class ClassWriter : IDisposable
	{
		private StreamWriter _writer;

		public ClassWriter(Stream stream, string namespaceName)
		{
			_writer = new StreamWriter(stream);
			WriteUsings();
			WriteNamespace(namespaceName);
		}

		public void Dispose()
		{
			if (_writer == null)
				return;
			WriteUtilityClasses();
			WriteNamespaceEnd();
			_writer.Flush();
			_writer = null;
		}

		public void Write(ClassInfo classInfo)
		{
			WriteOriginalClass(classInfo);
			WriteNullClass(classInfo);
		}

		private void WriteNamespace(string namespaceName)
		{
			_writer.WriteLine("namespace {0}{1}{{", namespaceName, Environment.NewLine);
		}

		private void WriteNamespaceEnd()
		{
			_writer.Write("}");
		}

		private void WriteUsings()
		{
			_writer.WriteLine("using System;");
			_writer.WriteLine("using System.Collections.Generic;");
			_writer.WriteLine("using System.Globalization;");
			_writer.WriteLine("using System.Linq;");
			_writer.WriteLine("using System.Xml.Linq;");
			_writer.WriteLine("#pragma warning disable 660,661");

			_writer.WriteLine();
		}

		private void WriteUtilityClasses()
		{
			_writer.WriteLine(@"
	internal static class Utils
	{
		public static Exception NullAccess<T>(this T src, string name)
		{
			return new NullReferenceException(""Property '"" + name + ""' was accessed from a null '"" + 
				typeof(T).BaseType.Name + ""' object"");
		}

		public static bool ValidatedEquals<T>(object d1, object d2)
		{
			if (d1 is T || d2 is T) return (d1 == null || d2 == null);
			return ReferenceEquals(d1, d2);
		}
	}");
		}

		private void WriteNullClass(ClassInfo classInfo)
		{
			_writer.WriteLine("\tinternal class Null{0} : {0}{1}\t{{", classInfo.GetCodeName(), Environment.NewLine);
			WriteThrowingProperties(classInfo);
			_writer.WriteLine("\t}}{0}", Environment.NewLine);
		}

		private void WriteOriginalClass(ClassInfo classInfo)
		{
			_writer.WriteLine("\tpublic partial class {0}{1}\t{{", classInfo.GetCodeName(), Environment.NewLine);

			WriteImplicitStringCast(classInfo);
			WriteAutoProperties(classInfo);
			_writer.WriteLine();

			WriteConstructors(classInfo);
			_writer.WriteLine();

			WriteEqualityMembers(classInfo);
			_writer.WriteLine("\t}}{0}", Environment.NewLine);
		}

		private void WriteEqualityMembers(ClassInfo classInfo)
		{
			_writer.WriteLine(
@"		public static bool operator ==({0} left, {0} right)
		{{
			return Utils.ValidatedEquals<Null{0}>(left, right);
		}}

		public static bool operator !=({0} left, {0} right)
		{{
			return !(left == right);
		}}", classInfo.GetCodeName());
		}

		private void WriteConstructors(ClassInfo classInfo)
		{
			WriteXElementConstructor(classInfo, _writer);
			_writer.WriteLine();
			WriteEmptyConstructor(classInfo);
		}

		private void WriteEmptyConstructor(ClassInfo classInfo)
		{
			_writer.WriteLine("\t\tpublic {0}(){1}\t\t{{ }}", classInfo.GetCodeName(), Environment.NewLine);
		}

		private void WriteXElementConstructor(ClassInfo classInfo, StreamWriter writer)
		{
			writer.WriteLine("\t\tpublic {0}(XElement element){1}\t\t{{", classInfo.GetCodeName(), Environment.NewLine);
			WritePropertyInitializationStatements(classInfo, writer);
			writer.WriteLine("\t\t}");
		}

		private void WriteAutoProperties(ClassInfo classInfo)
		{
			foreach (var property in classInfo.AllMembers)
				_writer.WriteLine("\t\tpublic virtual {0} {1} {{ get; set; }}", property.GetCodeType(), property.GetCodeName());
		}

		private void WriteThrowingProperties(ClassInfo classInfo)
		{
			foreach (PropertyInfo property in classInfo.AllMembers)
				_writer.WriteLine("\t\tpublic override {0} {1} {{ get {{ throw this.NullAccess(\"{1}\"); }} }}",
					property.GetCodeType(), property.GetCodeName());
		}

		private void WriteImplicitStringCast(ClassInfo classInfo)
		{
			if (!classInfo.AllMembers.Any(m => m.IsElementValue && m.GetCodeType() == "string"))
				return;

			_writer.WriteLine("\t\tpublic override string ToString(){return Value;}");
			_writer.WriteLine("\t\tpublic static implicit operator string(" + classInfo.GetCodeName() + " obj){return obj.Value;}");
			_writer.WriteLine();
		}

		private void WritePropertyInitializationStatements(ClassInfo classInfo, StreamWriter writer)
		{
			foreach (PropertyInfo property in classInfo.Elements)
				WritePropertyInitialization(writer, property, "Elements");
			foreach (PropertyInfo property in classInfo.Attributes)
				WritePropertyInitialization(writer, property, "Attributes");
		}

		private void WritePropertyInitialization(StreamWriter writer, PropertyInfo property, string collectionName)
		{
			if (property.IsElementValue)
				WriteValuePropertyInitialization(writer, property);
			else
				writer.WriteLine("\t\t\t{0} = {1}.{2};", property.GetCodeName(), GetPropertyValueRetriever(property, collectionName), GetPropertyValueAccessorMethod(property));
		}

		private void WriteValuePropertyInitialization(StreamWriter writer, PropertyInfo property)
		{
			var instanceCreation = property.IsParsable
				? GetParsedPropertyValue(property, "element")
				: "element.Value";

			writer.WriteLine("\t\t\t{0} = {1};", property.GetCodeName(), instanceCreation);
		}

		private string GetPropertyValueRetriever(PropertyInfo property, string collectionName)
		{
			string instanceCreation = property.IsParsable
				? GetParsedPropertyValue(property, "e")
				: GetXElementToPropertyValue(property, "e");

			return string.Format("element.{0}().Where(e => e.Name == \"{1}\").Select(e => {2})",
				collectionName,
				property.XmlName,
				instanceCreation);
		}

		private static string GetParsedPropertyValue(PropertyInfo property, string variableName)
		{
			var valueToParse = variableName + ".Value";

			return string.Format("string.IsNullOrEmpty({0}) ? ({1})null : {2}",
				valueToParse,
				property.GetCodeType(),
				TypeUtils.GetParsableType(property.XmlType).ConstructParseCall(valueToParse));
		}

		private string GetPropertyValueAccessorMethod(PropertyInfo property)
		{
			var accessorMethod = "SingleOrDefault()";
			if (property.BindedType != null)
				accessorMethod = string.Format("SingleOrDefault() ?? new Null{0}()", property.GetCodeType());
			if (property.IsList)
				accessorMethod = "ToList()";
			return accessorMethod;
		}

		private string GetXElementToPropertyValue(PropertyInfo property, string varName)
		{
			return property.BindedType != null
				? string.Format("new {0}({1})", property.BindedType.GetCodeName(), varName)
				: string.Format("{0}.Value", varName);
		}
	}
}