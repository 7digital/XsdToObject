using NUnit.Framework;

namespace SevenDigital.Parsing.XsdToObject.Tests
{
    [TestFixture]
    public class PropertyInfoTest
    {
        [Test]
        public void ShouldGenerateNameForList()
        {
        	var info = new PropertyInfo(new ClassInfo {XmlName = "someClass"});
			info.XmlName = "test";
			info.IsList = true;

        	Assert.That(info.GetCodeName(), Is.EqualTo("Tests"));
        }

        [Test]
        public void ShouldGenerateNameForListWithSAtTheEnd()
        {
            var info = new PropertyInfo(new ClassInfo { XmlName = "someClass" });
            info.XmlName = "tests";
            info.IsList = true;
            Assert.That(info.GetCodeName(), Is.EqualTo("Tests"));
        }

        [Test]
        public void ShouldGenerateNameForSimpleType()
        {
            var info = new PropertyInfo(new ClassInfo { XmlName = "someClass" });
            info.XmlName = "test";
            Assert.That(info.GetCodeName(), Is.EqualTo("Test"));
        }

        [Test]
        public void ShouldGenerateNameWithPropForList()
        {
            var info = new PropertyInfo(new ClassInfo { XmlName = "someClass" });
            info.XmlName = "someClass";
            info.IsList = true;
            Assert.That(info.GetCodeName(), Is.EqualTo("SomeClassProps"));
        }

        [Test]
        public void ShouldGenerateNameWithPropForSimpleType()
        {
            var info = new PropertyInfo(new ClassInfo { XmlName = "someClass" });
            info.XmlName = "someClass";
            Assert.That(info.GetCodeName(), Is.EqualTo("SomeClassProp"));
        }
    }
}