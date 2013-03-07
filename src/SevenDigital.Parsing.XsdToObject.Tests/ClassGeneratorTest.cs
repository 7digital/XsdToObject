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
			var generator = new ClassGenerator();

			using (Stream schemaStream = File.OpenRead(Path.Combine("res", "schema.xsd")))
				generator.Parse(schemaStream);

			_classes = generator.Generate();
		}

		[Test]
		public void ShouldHaveVehicles()
		{
			ClassInfo vehicle = _classes.Single(c => c.XmlName == "vehicles");
			Assert.That(vehicle.Elements.Count, Is.EqualTo(1));
			GeneratorAssertHelper.AssertBindedProperty(vehicle, "car", "car", true, "Cars", "IList<Car>");
		}

		[Test]
		public void ShouldHaveCar()
		{
			ClassInfo car = _classes.Single(c => c.XmlName == "car");

			Assert.That(car.Elements.Count, Is.EqualTo(5));
			GeneratorAssertHelper.AssertStringProperty(car, "brand", false, "Brand");
			GeneratorAssertHelper.AssertBindedProperty(car, "color", "color", false, "Color", "Color");
			GeneratorAssertHelper.AssertBindedProperty(car, "manufacturer", "manufacturerName", false, "Manufacturer", "ManufacturerName");
			GeneratorAssertHelper.AssertProperty(car, "productionDate", "date", false, "ProductionDate", "DateTime?", false);
			GeneratorAssertHelper.AssertProperty(car, "modelVersion", "decimal", false, "ModelVersion", "decimal?", false);
		}

		[Test]
		public void ShouldHaveColor()
		{
			ClassInfo color = _classes.Single(c => c.XmlName == "color");
			Assert.That(color.Elements.Count, Is.EqualTo(3));

			GeneratorAssertHelper.AssertStringProperty(color, "hue", false, "Hue");
			GeneratorAssertHelper.AssertProperty(color, "rgb", "int", false, "Rgb", "int?", false);
			GeneratorAssertHelper.AssertBindedProperty(color, "description", "colorDescription", false, "Description", "ColorDescription");
		}

		[Test]
		public void Title_should_have_TitleType_property()
		{
			ClassInfo title = _classes.Single(c => c.XmlName == "colorDescription");
			Assert.That(title.Elements.Count, Is.EqualTo(2));

			Assert.That(title.Attributes.Select(a => a.XmlName), Contains.Item("descriptionType"));
		}

		[Test]
		public void Title_should_have_LabelName_property()
		{
			ClassInfo labelName = _classes.Single(c => c.XmlName == "manufacturerName");
			Assert.That(labelName.Attributes.Count, Is.EqualTo(2));

			Assert.That(labelName.Attributes.Select(a => a.XmlName), Contains.Item("nameType"));
		}


		[Test]
		public void ShouldVechiclesCarPropertyHaveBindedType()
		{
			ClassInfo vehicle = _classes.Single(c => c.XmlName == "vehicles");
			Assert.That(vehicle.Elements.Single(p => p.XmlName == "car").BindedType,
				Is.EqualTo(_classes.Single(c => c.XmlName == "car")));
		}

		[Test]
		public void ShouldCarColorPropertyHaveBindedType()
		{
			ClassInfo car = _classes.Single(c => c.XmlName == "car");
			Assert.That(car.Elements.Single(p => p.XmlName == "color").BindedType,
				Is.EqualTo(_classes.Single(c => c.XmlName == "color")));
		}
	}
}
