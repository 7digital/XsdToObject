using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace SevenDigital.Parsing.XsdToObject.Tests
{
    [TestFixture]
    public class ClassGeneratorTest
    {
        private IEnumerable<ClassInfo> _classes;

        [SetUp]
        public void SetUp()
        {
            ClassGenerator generator = new ClassGenerator();

            using (Stream schemaStream = File.OpenRead(Path.Combine("res", "schema.xsd")))
                generator.Generate(schemaStream);

            _classes = generator.Create();
        }

        [Test]
        public void ShouldHaveVehicles()
        {
            ClassInfo vehicle = _classes.Single(c => c.XmlName == "vehicles");
            Assert.That(vehicle.Properties.Count, Is.EqualTo(1));
            GeneratorAssertHelper.AssertBindedProperty(vehicle, "car", "car", true, "Cars", "IList<Car>");
        }

        [Test]
        public void ShouldHaveCar()
        {
            ClassInfo car = _classes.Single(c => c.XmlName == "car");

            Assert.That(car.Properties.Count, Is.EqualTo(2));
            GeneratorAssertHelper.AssertStringProperty(car, "brand", false, "Brand");
            GeneratorAssertHelper.AssertBindedProperty(car, "color", "color", false, "Color", "Color");
        }

        [Test]
        public void ShouldHaveColor()
        {
            ClassInfo color = _classes.Single(c => c.XmlName == "color");
            Assert.That(color.Properties.Count, Is.EqualTo(2));

            GeneratorAssertHelper.AssertStringProperty(color, "hue", false, "Hue");
            //decimal is not known
            GeneratorAssertHelper.AssertStringProperty(color, "rgb", false, "Rgb");
        }

        [Test]
        public void ShouldVechiclesCarPropertyHaveBindedType()
        {
            ClassInfo vehicle = _classes.Single(c => c.XmlName == "vehicles");
            Assert.That(vehicle.Properties.Single(p => p.XmlName == "car").BindedType,
                Is.EqualTo(_classes.Single(c => c.XmlName == "car")));
        }

        [Test]
        public void ShouldCarColorPropertyHaveBindedType()
        {
            ClassInfo car = _classes.Single(c => c.XmlName == "car");
            Assert.That(car.Properties.Single(p => p.XmlName == "color").BindedType,
                Is.EqualTo(_classes.Single(c => c.XmlName == "color")));
        }
    }
}
