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

		void WriteNamespace(string namespaceName)
		{
			_writer.WriteLine("namespace {0}{1}{{", namespaceName, Environment.NewLine);
		}

		void WriteNamespaceEnd()
		{
			_writer.Write("}");
		}

		void WriteUsings()
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

		void WriteUtilityClasses()
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

		void WriteNullClass(ClassInfo classInfo)
		{
			_writer.WriteLine("\tinternal class Null{0} : {0}{1}\t{{", classInfo.GetCodeName(), Environment.NewLine);
			GenerateThrowingProperties(classInfo);
			_writer.WriteLine("\t}}{0}", Environment.NewLine);
		}

		void WriteOriginalClass(ClassInfo classInfo)
		{
			_writer.WriteLine("\tpublic class {0}{1}\t{{", classInfo.GetCodeName(), Environment.NewLine);

			WriteAutoProperties(classInfo);
			_writer.WriteLine();

			WriteConstructors(classInfo);
			_writer.WriteLine();

			WriteEqualityMembers(classInfo);
			_writer.WriteLine("\t}}{0}", Environment.NewLine);
		}

		void WriteEqualityMembers(ClassInfo classInfo)
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

		void WriteConstructors(ClassInfo classInfo)
		{
			WriteXElementConstructor(classInfo, _writer);
			_writer.WriteLine();
			WriteEmptyConstructor(classInfo);
		}

		void WriteEmptyConstructor(ClassInfo classInfo)
		{
			_writer.WriteLine("\t\tpublic {0}(){1}\t\t{{ }}", classInfo.GetCodeName(), Environment.NewLine);
		}

		void WriteXElementConstructor(ClassInfo classInfo, StreamWriter writer)
		{
			writer.WriteLine("\t\tpublic {0}(XElement element){1}\t\t{{", classInfo.GetCodeName(), Environment.NewLine);
			WritePropertyInitialisationStatements(classInfo, writer);
			writer.WriteLine("\t\t}");
		}

		void WriteAutoProperties(ClassInfo classInfo)
		{
			foreach (var property in classInfo.Elements.Union(classInfo.Attributes))
			{
				if (property.IsElementValue) WriteImplicitStringCast(classInfo);

				_writer.WriteLine("\t\tpublic virtual {0} {1} {{ get; set; }}", property.GetCodeType(), property.GetCodeName());
			}
		}

		void GenerateThrowingProperties(ClassInfo classInfo)
		{
			foreach (PropertyInfo property in classInfo.Elements.Union(classInfo.Attributes))
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

		void WritePropertyInitialisationStatements(ClassInfo classInfo, StreamWriter writer)
		{
			foreach (PropertyInfo property in classInfo.Elements)
			{
				writer.WriteLine(property.InitialiseFromCollection("Elements"));
			}
			foreach (PropertyInfo property in classInfo.Attributes)
			{
				writer.WriteLine(property.InitialiseFromCollection("Attributes"));
			}
		}
	}
}