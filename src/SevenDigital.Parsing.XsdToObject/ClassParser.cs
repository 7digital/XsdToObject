using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace SevenDigital.Parsing.XsdToObject
{
	public class ClassParser
	{
		private IDictionary<string, ClassInfo> _classes = new Dictionary<string, ClassInfo>();

		public void Parse(Stream stream)
		{
			XmlSchema schema = XmlSchema.Read(stream, null);
			string nsCode = GenerateNSCode(schema.TargetNamespace ?? "");

			foreach (var elem in schema.Items.OfType<XmlSchemaElement>())
				ParseElement(elem, nsCode);
			foreach (var complexType in schema.Items.OfType<XmlSchemaComplexType>())
				ParseType(complexType, complexType.Name, nsCode);
		}

		public IDictionary<string, ClassInfo> GetParsedClasses()
		{
			var result = _classes;
			_classes = new Dictionary<string, ClassInfo>();
			return result;
		}

		private string GenerateNSCode(string ns)
		{
			return string.Format("{0}#", ns.GetHashCode());
		}

		private void ParseType(XmlSchemaComplexType type, string name, string nsCode)
		{
			var classInfo = ParseComplex(type, name, nsCode);

			if (classInfo.Elements.Count != 0 || classInfo.Attributes.Count != 0)
				_classes.Add(nsCode + name, classInfo);
		}

		private void ParseElement(XmlSchemaElement elem, string nsCode)
		{
			if (elem.SchemaType is XmlSchemaComplexType)
				ParseType((XmlSchemaComplexType)elem.SchemaType, elem.Name, nsCode);
		}

		private ClassInfo ParseComplex(XmlSchemaComplexType complex, string name, string nsCode)
		{
			var classInfo = new ClassInfo { XmlName = name };
			ParseParticle(classInfo, complex.Particle, nsCode);

			if (complex.Attributes.Count > 0)
				AddAttributes(classInfo, complex.Attributes);

			AddExtensionAttributes(classInfo, complex);
			GenerateUniquePropertyNames(classInfo);
			return classInfo;
		}

		private void GenerateUniquePropertyNames(ClassInfo classInfo)
		{
			var attributesToRename = classInfo.Attributes.Where(attribute => classInfo.Elements.Any(p => p.XmlName == attribute.XmlName)).ToList();
			foreach (var attribute in attributesToRename)
			{
				classInfo.Attributes.Remove(attribute);
				attribute.XmlName += "Attribute";
				classInfo.Attributes.Add(attribute);
			}
		}

		private void AddExtensionAttributes(ClassInfo classInfo, XmlSchemaComplexType complex)
		{
			if (complex.ContentModel == null || !(complex.ContentModel.Content is XmlSchemaSimpleContentExtension))
				return;

			var sce = complex.ContentModel.Content as XmlSchemaSimpleContentExtension;

			if (sce.Attributes.Count == 0)
				return;

			AddAttributes(classInfo, sce.Attributes);
			AddValueProperty(classInfo, sce);
		}

		private void AddValueProperty(ClassInfo classInfo, XmlSchemaSimpleContentExtension sce)
		{
			var propInfo = new PropertyInfo(classInfo)
			{
				IsList = false,
				XmlName = "Value",
				XmlType = ResolveSimpleTypeName(sce.BaseTypeName.Name),
				IsElementValue = true
			};

			if (!classInfo.Elements.Contains(propInfo))
				classInfo.Elements.Add(propInfo);
		}

		private void AddAttributes(ClassInfo classInfo, XmlSchemaObjectCollection attributes)
		{
			foreach (XmlSchemaAttribute attribute in attributes)
				classInfo.Attributes.Add(PropertyFromAttribute(classInfo, attribute));
		}

		private PropertyInfo PropertyFromAttribute(ClassInfo classInfo, XmlSchemaAttribute attribute)
		{
			var prop = new PropertyInfo(classInfo)
			{
				XmlName = attribute.Name,
				XmlType = ResolveSimpleTypeName(attribute.SchemaTypeName.Name)
			};
			return prop;
		}

		private void ParseParticle(ClassInfo classInfo, XmlSchemaParticle particle, string nsCode)
		{
			var group = particle as XmlSchemaGroupBase;
			if (group == null)
				return;

			bool isList = (group.MaxOccurs > 1);
			foreach (XmlSchemaParticle groupMember in group.Items)
			{
				if (groupMember is XmlSchemaElement)
					ParseElementProperty(classInfo, (XmlSchemaElement)groupMember, (groupMember.MaxOccurs > 1) || isList, nsCode);
				else
					ParseParticle(classInfo, groupMember, nsCode);
			}
		}

		private void ParseElementProperty(ClassInfo classInfo, XmlSchemaElement elem, bool isList, string nsCode)
		{
			var propInfo = new PropertyInfo(classInfo)
			{
				IsList = isList,
				XmlName = elem.Name,
				XmlType = ResolveTypeName(elem, nsCode)
			};

			ParseElement(elem, nsCode);

			if (!classInfo.Elements.Contains(propInfo))
				classInfo.Elements.Add(propInfo);
		}

		private string ResolveTypeName(XmlSchemaElement elem, string nsCode)
		{
			if (elem.SchemaType is XmlSchemaSimpleType)
				return ResolveSimpleTypeName(elem.Name);

			if (elem.SchemaType != null)
				return ResolveTypeName(elem.Name, nsCode);

			if (!elem.SchemaTypeName.IsEmpty)
				return ResolveTypeName(elem.SchemaTypeName);

			throw new ArgumentException("XmlSchemaElement does not have SchemaType nor SchemaTypeName.");
		}

		private string ResolveSimpleTypeName(string name)
		{
			return TypeUtils.IsParsable(name) ? name : "string";
		}

		private string ResolveTypeName(string name, string nsCode)
		{
			if (TypeUtils.IsSimpleType(name))
				return name;
			return nsCode + name;
		}

		private string ResolveTypeName(XmlQualifiedName name)
		{
			return ResolveTypeName(name.Name, (GenerateNSCode(name.Namespace ?? "")));
		}
	}
}