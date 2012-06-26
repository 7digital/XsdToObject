using System.Text;

namespace SevenDigital.Parsing.XsdToObject
{
    public static class NameUtils
    {
        public static string ToCodeName(string input,bool isPlural)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(input.Substring(0, 1).ToUpper());
            sb.Append(input.Substring(1));
            if (isPlural && input[input.Length - 1] != 's')
                sb.Append('s');
            return sb.ToString();
        }
    }
}
