using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace SevenDigital.Parsing.XsdToObject.Tests
{
	[TestFixture]
	public class MultipleSourcesClassGeneratorTest
	{
		private IEnumerable<ClassInfo> _classes;

		[SetUp]
		public void SetUp()
		{
			ClassGenerator generator = new ClassGenerator();

			using (Stream schemaStream = File.OpenRead(Path.Combine("res", "ddexC.xsd")))
				generator.Generate(schemaStream);

			using (Stream schemaStream = File.OpenRead(Path.Combine("res", "release-notification.xsd")))
				generator.Generate(schemaStream);

			_classes = generator.Create();
		}

		[Test]
		public void ShouldHaveDdexCVersionOfImageDetailsByTerritory()
		{
			ClassInfo detailsByTerritoryClass = _classes.Single(c => c.GetCodeName() == "ImageDetailsByTerritory");

			Assert.That(
				detailsByTerritoryClass.Properties.SingleOrDefault(p => p.XmlName == "TechnicalImageDetails"),
				Is.Null);
		}

		[Test]
		public void ShouldCatalogItemHasDisplayArtistNameAsString()
		{
			ClassInfo catalogItem = _classes.Single(c => c.XmlName == "CatalogItem");
			GeneratorAssertHelper.AssertBindedProperty(catalogItem, "DisplayArtistName", "Name", false, "DisplayArtistName", "Name");
		}

		[Test]
		public void ShouldCatalogItemHasDisplayTitle()
		{
			ClassInfo catalogItem = _classes.Single(c => c.XmlName == "CatalogItem");
			GeneratorAssertHelper.AssertBindedProperty(catalogItem, "DisplayTitle", "ReferenceTitle", false, "DisplayTitle", "ReferenceTitle");
		}

		[Test]
		public void ShouldAdministratingRecordCompanyHasOnePartyId()
		{
			ClassInfo catalogItem = _classes.Single(c => c.XmlName == "AdministratingRecordCompany");
			GeneratorAssertHelper.AssertBindedProperty(catalogItem, "PartyId", "PartyId", false, "PartyId", "PartyId");
		}

		[Test]
		public void ShouldCollectionResourceReferenceHasCollectionResourceReferencePropOfStringType()
		{
			ClassInfo catalogItem = _classes.Single(c => c.XmlName == "CollectionResourceReference");
			GeneratorAssertHelper.AssertStringProperty(catalogItem, "CollectionResourceReference", false, "CollectionResourceReferenceProp");
		}

		[Test]
		public void ShouldTechnicalImageDetailsHaveMultipleFiles()
		{
			ClassInfo techDetails = _classes.Single(c => c.XmlName == "TechnicalImageDetails");
			GeneratorAssertHelper.AssertBindedProperty(techDetails, "File", "File", true, "Files", "IList<File>");
		}

		[Test]
		public void ShouldImageHaveImageDetailsByTerritory()
		{
			ClassInfo image = _classes.Single(c => c.XmlName == "Image");
			var prop = image.Properties.Single(p => p.XmlName == "ImageDetailsByTerritory");
			var detailsByTerritoryClass = prop.BindedType;

			Assert.That(
				detailsByTerritoryClass.Properties.SingleOrDefault(p => p.XmlName == "TechnicalImageDetails"),
				Is.Not.Null);
		}
	}
}
