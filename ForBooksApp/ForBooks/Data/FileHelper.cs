using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ForBooks.Data
{
    public static class FileHelper
    {
        private static readonly string basePath;

        static FileHelper()
        {
            // Početna putanja odakle se program pokreće
            string currentPath = AppContext.BaseDirectory;

            // Ako sadrži "bin", idi do korijena projekta
            string? projectRoot = currentPath;

            if (currentPath.Contains("bin"))
            {
                var dir = new DirectoryInfo(currentPath);
                while (dir != null && dir.Name != "ForBooks") 
                {
                    dir = dir.Parent;
                }

                if (dir != null)
                    projectRoot = dir.FullName;
            }

            // Postavi konačnu putanju do DataFiles
            basePath = Path.Combine(projectRoot, "DataFiles");

            //  Ako folder ne postoji, kreiraj ga
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            Console.WriteLine($"JSON folder: {basePath}");
        }

        private static string GetPath(string fileName)
        {
            return Path.Combine(basePath, $"{fileName}.json");
        }

        // Učitavanje podataka
      
        public static List<T> LoadFromFile<T>(string fileName)
        {
            string path = GetPath(fileName);
            if (!File.Exists(path)) return new List<T>();

            string json = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<List<T>>(json);
            return data ?? new List<T>();
        }

        // Čuvanje podataka
        public static void SaveToFile<T>(string fileName, List<T> data)
        {
            string path = GetPath(fileName);
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }
}
