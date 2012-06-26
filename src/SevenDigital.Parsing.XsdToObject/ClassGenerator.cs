using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Schema;

namespace SevenDigital.Parsing.XsdToObject
{
    public class ClassGenerator
    {
        readonly IDictionary<string, ClassInfo> _classes = new Dictionary<string, ClassInfo>();

        public void Generate(Stream stream)
        {
            XmlSchema schema = XmlSchema.Read(stream, null);
            string nsCode = GenerateNSCode(schema.TargetNamespace ?? "");

            foreach (var elem in schema.Items.OfType<XmlSchemaElement>())
                GenerateElement(elem, nsCode);
            foreach (var complexType in schema.Items.OfType<XmlSchemaComplexType>())
                GenerateType(complexType, complexType.Name, nsCode);
        }

        public IEnumerable<ClassInfo> Create()
        {
            BindClasses();
            CleanUnknownClasses();
            IEnumerable<ClassInfo> result = GenerateUniqueClasses();
            _classes.Clear();
            return result;
        }

        private string GenerateNSCode(string ns)
        {
            return string.Format("{0}#", ns.GetHashCode());
        }

        private IEnumerable<ClassInfo> GenerateUniqueClasses()
        {
            List<ClassInfo> classes = new List<ClassInfo>();
            foreach (var classInfo in _classes.Values)
            {
                RenameClassIfNecesarry(classInfo, classes);
                classes.Add(classInfo);
            }
            return classes;
        }

        private void RenameClassIfNecesarry(ClassInfo classInfo, List<ClassInfo> classes)
        {
            int suffix = 0;
            while (true)
            {
                if (!classes.Any(c => (c.XmlName == classInfo.XmlName) && (c.NameSuffix == suffix)))
                {
                    classInfo.NameSuffix = suffix;
                    return;
                }
                ++suffix;
            }
        }

        private void BindClasses()
        {
            foreach (var property in _classes.Values.SelectMany(c => c.Properties))
            {
                ClassInfo classInfo;
                property.BindedType = _classes.TryGetValue(property.XmlType, out classInfo) ? classInfo : null;
            }
        }

        private void CleanUnknownClasses()
        {
            foreach (var classInfo in _classes.Values)
                CleanUnknownClasses(classInfo);
        }

        private void CleanUnknownClasses(ClassInfo classInfo)
        {
            foreach (var property in classInfo.Properties.Where(property => !_classes.ContainsKey(property.XmlType)))
                property.XmlType = "string";
        }

        private bool GenerateType(XmlSchemaComplexType type, string name, string nsCode)
        {
            ClassInfo classInfo = new ClassInfo { XmlName = name };
            GenerateComplex(classInfo, type, nsCode);

            if (classInfo.Properties.Count == 0)
                return false;

            _classes.Add(nsCode + name, classInfo);
            return true;
        }

        private bool GenerateElement(XmlSchemaElement elem, string nsCode)
        {
            return (elem.SchemaType is XmlSchemaComplexType) &&
                   GenerateType((XmlSchemaComplexType)elem.SchemaType, elem.Name, nsCode);
        }

        private void GenerateComplex(ClassInfo classInfo, XmlSchemaComplexType complex, string nsCode)
        {
            GenerateParticle(classInfo, complex.Particle, nsCode);
        }

        private void GenerateParticle(ClassInfo classInfo, XmlSchemaParticle particle, string nsCode)
        {
            XmlSchemaGroupBase group = particle as XmlSchemaGroupBase;
            if (group == null)
                return;

            bool isList = (group.MaxOccurs > 1);
            foreach (XmlSchemaParticle groupMember in group.Items)
            {
                if (groupMember is XmlSchemaElement)
                    GenerateElementProperty(classInfo, (XmlSchemaElement)groupMember, (groupMember.MaxOccurs > 1) || isList, nsCode);
                else
                    GenerateParticle(classInfo, groupMember, nsCode);
            }
        }

        private void GenerateElementProperty(ClassInfo classInfo, XmlSchemaElement elem, bool isList, string nsCode)
        {
            PropertyInfo propInfo = new PropertyInfo(classInfo)
            {
                IsList = isList,
                XmlName = elem.Name,
                XmlType = ResolveTypeName(elem, nsCode)
            };

            if (elem.SchemaType != null && !GenerateElement(elem, nsCode))
                propInfo.XmlType = "string";

            if (!classInfo.Properties.Contains(propInfo))
                classInfo.Properties.Add(propInfo);
        }

        private string ResolveTypeName(XmlSchemaElement elem, string nsCode)
        {
            if (elem.SchemaType != null)
                return nsCode + elem.Name;

            if (!elem.SchemaTypeName.IsEmpty)
                return (GenerateNSCode(elem.SchemaTypeName.Namespace ?? "") + elem.SchemaTypeName.Name);

            return elem.Name;
        }
    }
}
