using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace SevenDigital.Parsing.XsdToObject.Tests
{
    [TestFixture]
    public class DdexC_ClassGeneratorTest
    {

        private IEnumerable<ClassInfo> _classes;

        [SetUp]
        public void SetUp()
        {
            ClassGenerator generator = new ClassGenerator();

            using (Stream schemaStream = File.OpenRead(Path.Combine("res", "ddexC.xsd")))
                generator.Generate(schemaStream);

            _classes = generator.Create();
        }

        [Test]
        public void ShouldGenerateFileTest()
        {
            ClassInfo file = _classes.Single(c => c.XmlName == "File");

            GeneratorAssertHelper.AssertStringProperty(file, "FileName", false, "FileName");
            GeneratorAssertHelper.AssertStringProperty(file, "FilePath", false, "FilePath");
            GeneratorAssertHelper.AssertStringProperty(file, "URL", false, "URL");
            GeneratorAssertHelper.AssertBindedProperty(file, "HashSum", "HashSum", false, "HashSum", "HashSum");
        }

    }
}
