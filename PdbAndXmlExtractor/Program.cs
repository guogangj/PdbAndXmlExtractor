using System;
using System.IO;

namespace PdbAndXmlExtractor;

class Program {
    static void Main(string[] args) {
        //Handle the options

        string targetPath = null;
        bool forceUpdate = false;

        for (int i = 0; i < args.Length; i++) {
            if (args[i] == "--force") {
                forceUpdate = true;
                continue;
            }
            if (targetPath == null) {
                targetPath = args[i];
                continue;
            }
            else {
                Console.WriteLine("Unknown parameter: " + args[i]);
                return;
            }
        }

        if (targetPath == null) {
            Console.WriteLine("Must specify the target path.");
            return;
        }

        if (!Directory.Exists(targetPath)) {
            Console.WriteLine("The specified target path does not exist: " + targetPath);
            return;
        }
        Processor processor = new Processor(AppConfig.Load());
        processor.Process(targetPath, forceUpdate);
    }
}
