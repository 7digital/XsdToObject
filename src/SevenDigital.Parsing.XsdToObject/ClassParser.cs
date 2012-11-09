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
				GenerateElement(elem, nsCode);
			foreach (var complexType in schema.Items.OfType<XmlSchemaComplexType>())
				GenerateType(complexType, complexType.Name, nsCode);
		}

		public IDictionary<string, ClassInfo> GetParsedClasses()
		{
			var result = _classes;
			_classes=new Dictionary<string, ClassInfo>();
			return result;
		}

		private string GenerateNSCode(string ns)
		{
			return string.Format("{0}#", ns.GetHashCode());
		}

		private bool GenerateType(XmlSchemaComplexType type, string name, string nsCode)
		{
			var classInfo = new ClassInfo { XmlName = name };
			GenerateComplex(classInfo, type, nsCode);

			if (classInfo.Elements.Count == 0 && classInfo.Attributes.Count == 0)
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

			if (complex.Attributes.Count > 0)
				AddAttributes(classInfo, complex.Attributes);

			AddExtensionAttributes(classInfo, complex);
		}

		private void AddExtensionAttributes(ClassInfo classInfo, XmlSchemaComplexType complex)
		{
			if (complex.ContentModel != null
			    && complex.ContentModel.Content is XmlSchemaSimpleContentExtension)
			{
				var sce = complex.ContentModel.Content as XmlSchemaSimpleContentExtension;

				if (sce.Attributes.Count > 0)
				{
					AddAttributes(classInfo, sce.Attributes);

					var propInfo = new PropertyInfo(classInfo)
					{
						IsList = false,
						XmlName = "Value",
						XmlType = "string",
						IsElementValue = true
					};
					TrySettingElementType(null, sce.BaseTypeName, propInfo);

					if (!classInfo.Elements.Contains(propInfo))
						classInfo.Elements.Add(propInfo);
				}
			}
		}

		private void AddAttributes(ClassInfo classInfo, XmlSchemaObjectCollection attributes)
		{
			foreach (XmlSchemaAttribute attribute in attributes)
			{
				classInfo.Attributes.Add(PropertyFromAttribute(classInfo, attribute));
			}
		}

		private PropertyInfo PropertyFromAttribute(ClassInfo classInfo, XmlSchemaAttribute attribute)
		{
			var prop = new PropertyInfo(classInfo) { XmlName = attribute.Name, XmlType = "string" };
			TrySettingElementType(attribute.SchemaType, attribute.SchemaTypeName, prop);
			return prop;
		}

		private void GenerateParticle(ClassInfo classInfo, XmlSchemaParticle particle, string nsCode)
		{
			var group = particle as XmlSchemaGroupBase;
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
			var propInfo = new PropertyInfo(classInfo)
			{
				IsList = isList,
				XmlName = elem.Name,
				XmlType = ResolveTypeName(elem, nsCode)
			};

			var generatedElement = GenerateElement(elem, nsCode);
			if (!generatedElement)
				TrySettingElementType(elem.SchemaType, elem.SchemaTypeName, propInfo);

			if (!classInfo.Elements.Contains(propInfo))
				classInfo.Elements.Add(propInfo);
		}

		private void TrySettingElementType(XmlSchemaType schemaType, XmlQualifiedName schemaTypeName, PropertyInfo propInfo)
		{
			var guessedType = GuessXmlType(schemaTypeName);
			var dotNetType = GuessParsableDotNetType(guessedType);

			if (LooksLikeParsableSimpleType(guessedType, dotNetType))
			{
				propInfo.XmlType = dotNetType;
				propInfo.IsParsable = true;
			}
			else if (schemaType != null)
			{
				propInfo.XmlType = "string";
			}
		}

		private bool LooksLikeParsableSimpleType(string guessedType, string dotNetType)
		{
			return !string.IsNullOrEmpty(guessedType) && dotNetType != null;
		}

		private string GuessXmlType(XmlQualifiedName schemaTypeName)
		{
			if (schemaTypeName == null || schemaTypeName.Name == null) return null;
			var name = schemaTypeName.Name;
			int lc = name.LastIndexOf(':');
			return lc <= 0 ? name : name.Substring(lc + 1);
		}

		private string GuessParsableDotNetType(string xmlTypeName)
		{
			switch (xmlTypeName.ToLower())
			{
				case "boolean":
					return "bool?";
				case "integer":
				case "int":
					return "int?";
				case "dateTime":
					return "DateTime?";
				case "date":
					return "DateTime?";
				default: return null;
			}
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