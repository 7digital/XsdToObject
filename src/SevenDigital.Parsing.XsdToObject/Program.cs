using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SevenDigital.Parsing.XsdToObject
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            string progName = AppDomain.CurrentDomain.FriendlyName;
            if (args.Length < 3)
            {
                Console.WriteLine("Usage:\n {0} [output.cs] [namespace] [input.xsd ...]", progName);
                Environment.Exit(-1);
            }

            Console.WriteLine("Running {0}...", progName);

            var generator = new ClassGenerator();
            string output = args[0];
            string ns = args[1];

            foreach (string file in args.Skip(2))
            {
                Console.WriteLine(" Parsing {0}...", file);
                using (Stream stream = File.OpenRead(file))
                    generator.Parse(stream);
            }

            Console.WriteLine(" Writing classes to {0}...", output);
            using (Stream stream = File.Open(output, FileMode.Create, FileAccess.Write))
                WriteClasses(stream, generator.Generate(), ns);
        }

        private static void WriteClasses(Stream stream, IEnumerable<ClassInfo> classes, string ns)
        {
            using (var writer = new ClassWriter(stream, ns))
                foreach (var classInfo in classes)
                    writer.Write(classInfo);
        }
    }
}
