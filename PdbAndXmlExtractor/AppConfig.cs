using System;
using System.IO;
using System.Text.Json;

namespace PdbAndXmlExtractor {
    class AppConfig {

        public static AppConfig _appConfig;

        public static AppConfig Load() {
            string configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            if (File.Exists(configFile)) {
                try {
                    _appConfig = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(configFile));
                }
                catch (Exception) {
                    //ignore
                }
            }
            if (_appConfig == null) {
                _appConfig = new AppConfig();
            }
            if (string.IsNullOrEmpty(_appConfig.NugetPath)) {
                _appConfig.NugetPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget");
            }
            if (string.IsNullOrEmpty(_appConfig.CachePath)) {
                _appConfig.NugetPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "PdbAndXmlCache");
            }
            if (string.IsNullOrEmpty(_appConfig.TargetDllPattern)) {
                _appConfig.TargetDllPattern = "*.dll";
            }
            return _appConfig;
        }

        /// <summary>
        /// Nuget path, default is：~/.nuget
        /// </summary>
        public string NugetPath { get; set; }
        /// <summary>
        /// Cache path, default is：~/PdbAndXmlCache
        /// </summary>
        public string CachePath { get; set; }
        /// <summary>
        /// Target dll pattern, default is: *.dll
        /// </summary>
        public string TargetDllPattern { get; set; }
    }
}
