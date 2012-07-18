using System;
using System.IO;

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
            _writer.WriteLine("using System.Collections.Generic;");
            _writer.WriteLine("using System.Linq;");
            _writer.WriteLine("using System.Xml.Linq;");
            _writer.WriteLine();
        }

        public void Dispose()
        {
            if (_writer == null)
                return;
            WriteNamespaceEnd();
            _writer.Flush();
            _writer = null;
        }

        public void Write(ClassInfo classInfo)
        {
            _writer.WriteLine("\tpublic class {0}{1}\t{{", classInfo.GetCodeName(), Environment.NewLine);

            GenerateProperties(classInfo);
            _writer.WriteLine();

            GenerateConstructors(classInfo);

            _writer.WriteLine("\t}}{0}", Environment.NewLine);
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
            foreach (PropertyInfo property in classInfo.Properties)
            {
                GeneratePropertyInitialization(property, property.IsList
                    ? "ToList()"
                    : "SingleOrDefault()");
            }
			foreach (string attr in classInfo.Attributes)
            {
                GenerateAttributeInitialization(attr);
            }
            _writer.WriteLine("\t\t}");
        }

		void GenerateAttributeInitialization(string attr)
		{
			_writer.WriteLine("\t\t\t{0} = element.Attributes().Where(a=>a.Name==\"{0}\").Select(a=>a.Value).FirstOrDefault();",
				attr);
		}

    	private void GeneratePropertyInitialization(PropertyInfo property, string chainLastMethod)
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
            foreach (PropertyInfo property in classInfo.Properties)
                _writer.WriteLine("\t\tpublic {0} {1} {{ get; set; }}", property.GetCodeType(), property.GetCodeName());
			
            foreach (string property in classInfo.Attributes)
                _writer.WriteLine("\t\tpublic string {0} {{ get; set; }}", property);
        }

    }
}