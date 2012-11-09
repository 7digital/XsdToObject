using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SevenDigital.Parsing.XsdToObject
{
	public class ClassGenerator
	{
		private readonly ClassParser _parser = new ClassParser();

		public void Parse(Stream stream)
		{
			_parser.Parse(stream);
		}

		public IEnumerable<ClassInfo> Generate()
		{
			var classes = _parser.GetParsedClasses();
			BindClasses(classes);
			CleanUnknownClasses(classes);
			return GenerateUniqueClasses(classes.Values);
		}

		private IEnumerable<ClassInfo> GenerateUniqueClasses(IEnumerable<ClassInfo> parsedClasses)
		{
			var finalClasses = new List<ClassInfo>();
			foreach (var classInfo in parsedClasses)
			{
				RenameClassIfNecesarry(classInfo, finalClasses);
				finalClasses.Add(classInfo);
			}
			return finalClasses;
		}

		private void RenameClassIfNecesarry(ClassInfo classInfo, List<ClassInfo> existingClasses)
		{
			int suffix = 0;
			while (true)
			{
				if (!existingClasses.Any(c => (c.XmlName == classInfo.XmlName) && (c.NameSuffix == suffix)))
				{
					classInfo.NameSuffix = suffix;
					return;
				}
				++suffix;
			}
		}

		private void BindClasses(IDictionary<string, ClassInfo> classes)
		{
			foreach (var property in classes.Values.SelectMany(c => c.AllMembers))
			{
				ClassInfo classInfo;
				property.BindedType = classes.TryGetValue(property.XmlType, out classInfo) ? classInfo : null;
			}
		}

		private void CleanUnknownClasses(IDictionary<string, ClassInfo> classes)
		{
			foreach (var classInfo in classes.Values)
				CleanUnknownClasses(classInfo, classes);
		}

		private void CleanUnknownClasses(ClassInfo classInfo, IDictionary<string, ClassInfo> classes)
		{
			foreach (var property in classInfo.AllMembers.Where(property => !classes.ContainsKey(property.XmlType)))
			{
				property.XmlType = property.IsParsable
					? property.XmlType
					: "string";
			}
		}
	}
}
