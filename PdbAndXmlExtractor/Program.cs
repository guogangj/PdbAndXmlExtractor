using System;
using System.IO;

namespace PdbAndXmlExtractor {
    class Program {
        static void Main(string[] args) {
            if (args.Length != 1) {
                Console.WriteLine("It takes one (and only one) parameter to specify the target path.");
                return;
            }
            string targetPath = args[0];
            if (!Directory.Exists(targetPath)) {
                Console.WriteLine("The specified target path does not exist: " + targetPath);
                return;
            }
            Processor processor = new Processor(AppConfig.Load());
            processor.Process(targetPath);
        }
    }
}
