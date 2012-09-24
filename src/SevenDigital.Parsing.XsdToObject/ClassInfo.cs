using System.Collections.Generic;
using System.Globalization;

namespace SevenDigital.Parsing.XsdToObject
{
    public class ClassInfo
    {
        public ClassInfo()
        {
            Elements = new List<PropertyInfo>();
			Attributes = new List<PropertyInfo>();
        }

    	public string XmlName { get; set; }
        public int NameSuffix { get; set; }

		public List<PropertyInfo> Attributes { get; set; }
        public List<PropertyInfo> Elements { get; private set; }

        public string GetCodeName()
        {
            return NameUtils.ToCodeName(XmlName, false)
                + ((NameSuffix == 0) ? "" : NameSuffix.ToString(CultureInfo.InvariantCulture));
        }

        private bool Equals(ClassInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.XmlName, XmlName) && Equals(other.Elements, Elements) && other.NameSuffix == NameSuffix;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ClassInfo)) return false;
            return Equals((ClassInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (XmlName != null ? XmlName.GetHashCode() : 0);
                result = (result * 397) ^ (Elements != null ? Elements.GetHashCode() : 0);
                result = (result * 397) ^ NameSuffix;
                return result;
            }
        }

		public override string ToString()
		{
			return XmlName + "P:"+Elements.Count+"; A:"+Attributes.Count;
		}
    }
}
