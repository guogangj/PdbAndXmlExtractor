# Summary
A small tool to extract xml and pdb files from Nuget Packages.

Microsoft dotnet core sdk has a feature flaw that XML and PDB files not be copied from nuget package files to target folder during building or publishing. As we know, the XML and PDB files are very important for debuging and issue tracking in both development and production. If my memory serves me right, this feature flaw comed with dotnet version 2.2, perhaps a new version of 2.1, but it still exists in the latest version 3.1.

I described this feature flaw in there: https://github.com/dotnet/core/issues/3587, And a relative post is there: https://github.com/dotnet/sdk/issues/1458

This small tool is a workaroud for this issue.

The priciple of this tool is simple. It's based on these facts and presuppositions:

- We have a ```.nuget``` folder that stores all nuget packages referenced by our projects. This folder is managed by dotnet SDK. (It is usally ```~/.nuget``` or ```C:\Users\username\.nuget```)
- Nuget package files (*nupkg) are actually zip files and I can easily extract content form them.
- The DLL has strict same version to the Nuget package. That's important. Considering most DLLs I care about are my own projects, it fits.

Now, my job is clear. Find the output path for the DLL we care about, if it does not have a PDB or XML, then try to find the same version nupkg in ```.nuget``` folder, extract the PDB and XML file to the output folder. That's it. In adddition, I make a cache folder for the PDB and XML files to make this process faster.

# Usage

The config file ```config.json```
```
{
  "NugetPath": "C:\\Users\\guoga\\.nuget",
  "CachePath": "C:\\Users\\guoga\\PdbAndXmlCache",
  "TargetDllPattern": "Njt.*.dll"
}
```
- NugetPath is the ```.nuget``` folder's location. It's under the user's home folder by default.
- CachePath is the cache folder to store PDB and XML to make this job faster.
- TargetDllPattern is the DLL names' pattern you care about. It's "*.dll" by default.

Run it like this:
```
dotnet PdbAndXmlExtractor.dll C:\work\my-project\bin\Debug\netcoreapp3.1
```

Usually, if the PDB or XML files are missing, they will be attempted to be obtained from the NuGet packages, otherwise they will be skipped. However, you can also use the `--force` parameter to forcibly obtain the PDB and XML files from the NuGet packages.
```
dotnet PdbAndXmlExtractor.dll C:\work\my-project\bin\Debug\net6.0 --force
```

If you want to run it every time you build the project, you can write this command in the project's ```Post-build event command line``` configuration. For example:
```
"C:\\Program Files\\dotnet\\dotnet.exe" "D:\\MyTools\\Njt.PdbAndXmlExtractor.dll" $(TargetDir)
```
