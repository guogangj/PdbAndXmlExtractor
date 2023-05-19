using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace PdbAndXmlExtractor;
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

    private void TryGetPdbAndXml(string dllFile, string version, string packageName, string pdbFile, string xmlFile) {

        if (version == null || version.Split('.').Length != 3) {
            Console.WriteLine("Failed to get version of dll file: " + dllFile);
            return;
        }
        MakeCache(packageName, version, out string cachePdbFile, out string cacheXmlFile);
        if (cachePdbFile != null) {
            File.Copy(cachePdbFile, pdbFile, true);
            Console.WriteLine(" => " + pdbFile);
        }
        if (cacheXmlFile != null) {
            File.Copy(cacheXmlFile, xmlFile, true);
            Console.WriteLine(" => " + xmlFile);
        }
    }

    public void Process(string targetPath, bool forceUpdate) {
        string[] files = Directory.GetFiles(targetPath, _appConfig.TargetDllPattern, SearchOption.TopDirectoryOnly);
        foreach (string dllFile in files) {

            //Get DLL version
            FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(dllFile);
            string version = fileVersion.ProductVersion;

            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(dllFile);
            string pdbFile = Path.Combine(targetPath, fileNameWithoutExt + ".pdb");
            string xmlFile = Path.Combine(targetPath, fileNameWithoutExt + ".xml");
            bool pdbExists = File.Exists(pdbFile);
            bool xmlExists = File.Exists(xmlFile);

            bool needToExtract = false;
            if (forceUpdate) {
                needToExtract = true;
            }

            if (!needToExtract && (!pdbExists || !xmlExists)) {
                needToExtract = true;
            }

            if (needToExtract) {
                TryGetPdbAndXml(dllFile, version, fileNameWithoutExt, pdbFile, xmlFile);
            }
        }
    }
}
