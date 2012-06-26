using System;
using System.IO;

namespace SevenDigital.Parsing.XsdToObject
{
    public class ClassWriter : IDisposable
    {
        private StreamWriter _writter;

        public ClassWriter(Stream stream, string namespaceName)
        {
            _writter = new StreamWriter(stream);
            WriteUsings();
            WriteNamespace(namespaceName);
        }

        private void WriteNamespace(string namespaceName)
        {
            _writter.WriteLine("namespace {0}{1}{{", namespaceName, Environment.NewLine);
        }

        private void WriteNamespaceEnd()
        {
            _writter.Write("}");
        }

        private void WriteUsings()
        {
            _writter.WriteLine("using System.Collections.Generic;");
            _writter.WriteLine("using System.Linq;");
            _writter.WriteLine("using System.Xml.Linq;");
            _writter.WriteLine();
        }

        public void Dispose()
        {
            if (_writter == null)
                return;
            WriteNamespaceEnd();
            _writter.Flush();
            _writter = null;
        }

        public void Write(ClassInfo classInfo)
        {
            _writter.WriteLine("\tpublic class {0}{1}\t{{", classInfo.GetCodeName(), Environment.NewLine);

            GenerateProperties(classInfo);
            _writter.WriteLine();

            GenerateConstructors(classInfo);

            _writter.WriteLine("\t}}{0}", Environment.NewLine);
        }

        private void GenerateConstructors(ClassInfo classInfo)
        {
            GenerateParseConstructor(classInfo);
            _writter.WriteLine();
            GenerateDefaultConstructor(classInfo);
        }

        private void GenerateDefaultConstructor(ClassInfo classInfo)
        {
            _writter.WriteLine("\t\tpublic {0}(){1}\t\t{{ }}", classInfo.GetCodeName(), Environment.NewLine);
        }

        private void GenerateParseConstructor(ClassInfo classInfo)
        {
            _writter.WriteLine("\t\tpublic {0}(XElement element){1}\t\t{{", classInfo.GetCodeName(), Environment.NewLine);
            foreach (PropertyInfo property in classInfo.Properties)
            {
                GeneratePropertyInitialization(property, property.IsList
                    ? "ToList()"
                    : "SingleOrDefault()");
            }
            _writter.WriteLine("\t\t}");
        }

        private void GeneratePropertyInitialization(PropertyInfo property, string chainLastMethod)
        {
            _writter.WriteLine("\t\t\t{0} = element.Elements().Where(e => e.Name == \"{1}\").Select(e => {2}).{3};",
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
                _writter.WriteLine("\t\tpublic {0} {1} {{ get; set; }}", property.GetCodeType(), property.GetCodeName());
        }

    }
}