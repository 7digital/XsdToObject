using System.Linq;
using NUnit.Framework;

namespace SevenDigital.Parsing.XsdToObject.Tests
{
    public static class GeneratorAssertHelper
    {
        public static void AssertProperty(ClassInfo classInfo, string propXmlName, string xmlPropType, bool isList, string propertyName, string propertyType, bool hasBindedClass)
        {
            var property = classInfo.Elements.Single(p => p.XmlName == propXmlName);
            Assert.That(property.IsList, Is.EqualTo(isList), "List or not doesn't match");
            if (hasBindedClass)
                Assert.That(property.XmlType.EndsWith("#" + xmlPropType), Is.True, "Property xml type doesn't match");
            else
                Assert.That(property.XmlType, Is.EqualTo(xmlPropType), "Property xml type doesn't match");
            Assert.That(property.GetCodeName(), Is.EqualTo(propertyName), "Property name doesn't match");
            Assert.That(property.GetCodeType(), Is.EqualTo(propertyType), "Property type doesn't match");
            Assert.That(property.BindedType != null, Is.EqualTo(hasBindedClass), "Binding not right");
        }

        public static void AssertBindedProperty(ClassInfo classInfo, string propXmlName, string xmlPropType, bool isList, string propertyName, string propertyType)
        {
            AssertProperty(classInfo, propXmlName, xmlPropType, isList, propertyName, propertyType, true);
        }

        public static void AssertStringProperty(ClassInfo classInfo, string propXmlName, bool isList, string propertyName)
        {
            AssertProperty(classInfo, propXmlName, "string", isList, propertyName, "string", false);
        }
    }
}
