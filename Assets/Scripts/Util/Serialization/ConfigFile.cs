using System;
using System.IO;
using UnityEngine;

namespace Util.Serialization
{
    public class ConfigFile
    {
        public ConfigFile(Stream content)
        {
            using (StreamReader reader = new StreamReader(content))
            {
                int lineIndex = 0;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine().TrimStart();
                    lineIndex++;

                    if (line.StartsWith("#") || line == "")
                        continue;

                    if (!line.Contains("="))
                    {
                        Debug.LogWarning("Line #" + lineIndex + " did not have an = sign. Ignoring.");
                        continue;
                    }

                    int eqIndex = line.IndexOf("=", StringComparison.InvariantCulture);
                    string key = line.Substring(0, eqIndex);
                    Debug.Log("Key: " + key);

                    string value = line.Substring(eqIndex + 1);
                    Debug.Log("Value: " + value);

                    if (value.StartsWith("["))
                    {
                        value.Split(",");
                    }
                }
            }
        }

        /// <summary>
        /// Loads or creates a config file.
        /// </summary>
        /// <param name="configPath">Path relative to <see cref="Paths.DATA_DIR"/></param>
        public static ConfigFile CreateOrLoadConfigFile(string configPath)
        {
            string fullPath = GetFullPath(configPath);
            if (File.Exists(fullPath))
            {
                return new ConfigFile(File.Open(fullPath, FileMode.Open));
            }

            string parentDir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(parentDir))
            {
                Directory.CreateDirectory(fullPath);
            }
            return new ConfigFile(File.Create(fullPath));
        }

        private static string GetFullPath(string configPath)
        {
            return Path.Combine(Paths.DATA_DIR, configPath);
        }
    }
}