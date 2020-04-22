using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace PdbAndXmlExtractor {
    class Processor {

        private readonly AppConfig _appConfig;

        public Processor(AppConfig appConfig) {
            _appConfig = appConfig;
        }

        private void MakeCache(string packageName, string version, out string pdb, out string xml) {
            //check if cache contains the xml and pdb
            string cachePathOfThisVersion = Path.Combine(_appConfig.CachePath, packageName.ToLower(), version);
            string cacheXml = Path.Combine(cachePathOfThisVersion, packageName + ".xml");
            string cachePdb = Path.Combine(cachePathOfThisVersion, packageName + ".pdb");
            pdb = null;
            xml = null;
            //Try to extract pdb and xml file only once
            if (!Directory.Exists(cachePathOfThisVersion)) {
                Directory.CreateDirectory(cachePathOfThisVersion);
                //Try to extract xml and from ~/.nuget
                string nupkgFile = Path.Combine(_appConfig.NugetPath, "packages", packageName.ToLower(), version, packageName.ToLower() + "." + version + ".nupkg");
                if (!File.Exists(nupkgFile)) {
                    return;
                }
                ZipArchive zip = ZipFile.OpenRead(nupkgFile);
                foreach (ZipArchiveEntry entry in zip.Entries) {
                    if (string.Equals(entry.Name, packageName + ".pdb", StringComparison.OrdinalIgnoreCase)) {
                        entry.ExtractToFile(cachePdb);
                        pdb = cachePdb;

                    }
                    if (string.Equals(entry.Name, packageName + ".xml", StringComparison.OrdinalIgnoreCase)) {
                        entry.ExtractToFile(cacheXml);
                        xml = cacheXml;

                    }
                }
            }
            if (File.Exists(cachePdb)) {
                pdb = cachePdb;
            }
            if (File.Exists(cacheXml)) {
                xml = cacheXml;
            }
        }

        private void TryGetPdbAndXml(string dllFile, string packageName, string pdbFile, string xmlFile) {
            string version;
            try {
                version = Assembly.LoadFile(dllFile).GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            }
            catch (Exception) {
                return;
            }
            string[] verParts = version.Split(".");
            if (verParts.Length != 4) {
                return;
            }
            version = verParts[0] + "." + verParts[1] + "." + verParts[2];
            MakeCache(packageName, version, out string cachePdbFile, out string cacheXmlFile);
            if (cachePdbFile != null) {
                File.Copy(cachePdbFile, pdbFile);
                Console.WriteLine(" => " + pdbFile);
            }
            if (cacheXmlFile != null) {
                File.Copy(cacheXmlFile, xmlFile);
                Console.WriteLine(" => " + xmlFile);
            }
        }

        public void Process(string targetPath) {
            string[] files = Directory.GetFiles(targetPath, _appConfig.TargetDllPattern, SearchOption.TopDirectoryOnly);
            foreach (string dllFile in files) {
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(dllFile);
                string pdbFile = Path.Combine(targetPath, fileNameWithoutExt + ".pdb");
                string xmlFile = Path.Combine(targetPath, fileNameWithoutExt + ".xml");
                if (!File.Exists(pdbFile) && !File.Exists(xmlFile)) {
                    TryGetPdbAndXml(dllFile, fileNameWithoutExt, pdbFile, xmlFile);
                }
            }
        }
    }
}
