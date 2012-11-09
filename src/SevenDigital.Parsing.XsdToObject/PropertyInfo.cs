namespace SevenDigital.Parsing.XsdToObject
{
	public class PropertyInfo
	{
		private readonly ClassInfo _targetClass;
		public string XmlName { get; set; }
		public string XmlType { get; set; }
		public bool IsList { get; set; }
		public ClassInfo BindedType { get; set; }
		public bool IsElementValue { get; set; }
		public bool IsParsable { get { return TypeUtils.IsParsable(XmlType); } }

		public PropertyInfo(ClassInfo targetClass)
		{
			_targetClass = targetClass;
		}

		public string GetCodeName()
		{
			string classType = _targetClass.GetCodeName();
			string result = NameUtils.ToCodeName(XmlName, IsList);
			return (classType == result)
				? NameUtils.ToCodeName(XmlName + "Prop", IsList)
				: result;
		}

		public string GetCodeType()
		{
			string type = BindedType != null ? BindedType.GetCodeName() : TypeUtils.ToCodeType(XmlType);
			return !IsList ? type : string.Format("IList<{0}>", type);
		}

		public string InitialiseFromCollection(string collectionName)
		{
			var propertyEquals = string.Format("\t\t\t{0} = ", GetCodeName());
			var accessorMethod = "SingleOrDefault()";
			if (BindedType != null)
				accessorMethod = string.Format("SingleOrDefault() ?? new Null{0}()", GetCodeType());
			if (IsList)
				accessorMethod = "ToList()";

			string propertyInitialisationStatment;
			if (IsElementValue)
			{
				propertyInitialisationStatment = "element.Value";
			}
			else
			{
				propertyInitialisationStatment = string.Format("element.{3}().Where(e => e.Name == \"{0}\").Select(e => {1}).{2}",
					XmlName,
					XElementToValue("e"),
					accessorMethod,
					collectionName
					);
			}
			if (IsParsable)
			{
				return propertyEquals + string.Format("string.IsNullOrEmpty({0}) ? null : ({1}?){1}.Parse({0});",
				propertyInitialisationStatment,
				GetCodeType().TrimEnd('?')
				);
			}
			else
			{
				propertyInitialisationStatment = propertyEquals + propertyInitialisationStatment;
			}

			propertyInitialisationStatment += ";";
			return propertyInitialisationStatment;
		}

		string XElementToValue(string varName)
		{
			return BindedType != null
				? string.Format("new {0}({1})", BindedType.GetCodeName(), varName)
				: string.Format("{0}.Value", varName);
		}

		public override string ToString()
		{
			return string.Format("XmlType: {0}, XmlName: {1}, IsList: {2}, IsElementValue: {3}", XmlType, XmlName, IsList, IsElementValue);
		}

		#region Equality members
		private bool Equals(PropertyInfo other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.XmlName, XmlName) && Equals(other.XmlType, XmlType) && other.IsList.Equals(IsList) && Equals(other._targetClass, _targetClass);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof(PropertyInfo)) return false;
			return Equals((PropertyInfo)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (XmlName != null ? XmlName.GetHashCode() : 0);
				result = (result * 397) ^ (XmlType != null ? XmlType.GetHashCode() : 0);
				result = (result * 397) ^ IsList.GetHashCode();
				result = (result * 397) ^ (_targetClass != null ? _targetClass.GetHashCode() : 0);
				return result;
			}
		}
		#endregion
	}
}