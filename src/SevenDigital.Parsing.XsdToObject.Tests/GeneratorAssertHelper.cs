using System.Linq;
using NUnit.Framework;

namespace SevenDigital.Parsing.XsdToObject.Tests
{
    public static class GeneratorAssertHelper
    {
        public static void AssertProperty(ClassInfo classInfo, string propXmlName, string xmlPropType, bool isList, string propertyName, string propertyType, bool hasBindedClass)
        {
            var property = classInfo.Properties.Single(p => p.XmlName == propXmlName);
            Assert.That(property.IsList, Is.EqualTo(isList));
            if (hasBindedClass)
                Assert.That(property.XmlType.EndsWith("#" + xmlPropType), Is.True);
            else
                Assert.That(property.XmlType, Is.EqualTo(xmlPropType));
            Assert.That(property.GetCodeName(), Is.EqualTo(propertyName));
            Assert.That(property.GetCodeType(), Is.EqualTo(propertyType));
            Assert.That(property.BindedType != null, Is.EqualTo(hasBindedClass));
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
