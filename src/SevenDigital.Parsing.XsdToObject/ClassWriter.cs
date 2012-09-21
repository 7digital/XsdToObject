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
			_writer.WriteLine("using System.Linq;");
			_writer.WriteLine("using System.Xml.Linq;");
			_writer.WriteLine("#pragma warning disable 660,661");

			_writer.WriteLine();
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

		public void Write(ClassInfo classInfo)
		{
			WriteOriginalClass(classInfo);
			WriteNullClass(classInfo);
		}

		private void WriteNullClass(ClassInfo classInfo)
		{
			_writer.WriteLine("\tinternal class Null{0} : {0}{1}\t{{", classInfo.GetCodeName(), Environment.NewLine);
			GenerateThrowingProperties(classInfo);
			_writer.WriteLine("\t}}{0}", Environment.NewLine);
		}

		private void WriteOriginalClass(ClassInfo classInfo)
		{
			_writer.WriteLine("\tpublic class {0}{1}\t{{", classInfo.GetCodeName(), Environment.NewLine);

			GenerateProperties(classInfo);
			_writer.WriteLine();

			GenerateConstructors(classInfo);
			_writer.WriteLine();

			GenerateEqualityMembers(classInfo);
			_writer.WriteLine("\t}}{0}", Environment.NewLine);
		}

		private void GenerateEqualityMembers(ClassInfo classInfo)
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

		private void GenerateConstructors(ClassInfo classInfo)
		{
			GenerateParseConstructor(classInfo);
			_writer.WriteLine();
			GenerateDefaultConstructor(classInfo);
		}

		private void GenerateDefaultConstructor(ClassInfo classInfo)
		{
			_writer.WriteLine("\t\tpublic {0}(){1}\t\t{{ }}", classInfo.GetCodeName(), Environment.NewLine);
		}

		private void GenerateParseConstructor(ClassInfo classInfo)
		{
			_writer.WriteLine("\t\tpublic {0}(XElement element){1}\t\t{{", classInfo.GetCodeName(), Environment.NewLine);
			foreach (PropertyInfo property in classInfo.Properties.Union(classInfo.Attributes))
			{
				var lastMethod = "SingleOrDefault()";
				if (property.IsList)
					lastMethod = "ToList()";
				else if (property.BindedType != null)
					lastMethod = string.Format("SingleOrDefault() ?? new Null{0}()", property.GetCodeType());

				GeneratePropertyInitialization(property, lastMethod);
			}
		/*	foreach (PropertyInfo attr in )
			{
				GenerateAttributeInitialization(attr.XmlName);
			}*/
			_writer.WriteLine("\t\t}");
		}

		void GenerateAttributeInitialization(string attr)
		{
			_writer.WriteLine("\t\t\t{0} = element.Attributes().Where(a => a.Name == \"{1}\").Select(a => a.Value).SingleOrDefault();",
				NameUtils.ToCodeName(attr, false), attr);
		}

		private void GeneratePropertyInitialization(PropertyInfo property, string chainLastMethod)
		{
			if (property.IsElementValue)
			{
				_writer.WriteLine("\t\t\t{0} = element.Value;",
								  property.GetCodeName());
			}
			else
			{
				if (property.IsParsable) InitialiseByParse(property, chainLastMethod);
				else InitialiseString(property, chainLastMethod);
			}
		}

		void InitialiseByParse(PropertyInfo property, string chainLastMethod)
		{
			_writer.WriteLine("\t\t\t{0} = {4}.Parse(element.Elements().Where(e => e.Name == \"{1}\").Select(e => {2}).{3});",
				property.GetCodeName(),
				property.XmlName,
				XElementToValue(property, "e"),
				chainLastMethod,
				property.XmlType);
		}

		void InitialiseString(PropertyInfo property, string chainLastMethod)
		{
			_writer.WriteLine("\t\t\t{0} = element.Elements().Where(e => e.Name == \"{1}\").Select(e => {2}).{3};",
				property.GetCodeName(),
				property.XmlName,
				XElementToValue(property, "e"),
				chainLastMethod);
		}

		private string XElementToValue(PropertyInfo property, string varName)
		{
			return property.BindedType != null
				? string.Format("new {0}({1})", property.BindedType.GetCodeName(), varName)
				: string.Format("{0}.Value", varName);
		}

		private void GenerateProperties(ClassInfo classInfo)
		{
			foreach (var property in classInfo.Properties.Union(classInfo.Attributes))
			{
				if (property.IsElementValue) WriteImplicitStringCast(classInfo);

				_writer.WriteLine("\t\tpublic virtual {0} {1} {{ get; set; }}", property.GetCodeType(), property.GetCodeName());
			}

			/*foreach (PropertyInfo property in )
			{
				_writer.WriteLine("\t\tpublic virtual string {0} {{ get; set; }}", NameUtils.ToCodeName(property.XmlName, false));
			}*/
		}

		private void GenerateThrowingProperties(ClassInfo classInfo)
		{
			foreach (PropertyInfo property in classInfo.Properties.Union(classInfo.Attributes))
			{
				_writer.WriteLine("\t\tpublic override {0} {1} {{ get {{ throw this.NullAccess(\"{1}\"); }} }}", property.GetCodeType(), property.GetCodeName());
			}
		}

		void WriteImplicitStringCast(ClassInfo classInfo)
		{
			_writer.WriteLine("\t\tpublic override string ToString(){return Value;}");
			_writer.WriteLine("\t\tpublic static implicit operator string(" + classInfo.GetCodeName() + " obj){return obj.Value;}");
			_writer.WriteLine();
		}
	}
}