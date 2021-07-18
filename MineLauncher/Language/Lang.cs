using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MineLauncher.Language
{
    public static class Lang
    {
        static CultureInfo _culture;
        public static CultureInfo Culture { get { return _culture; } }

        static List<JObject> _domainList = new List<JObject>();
        static List<string> _domainNames = new List<string>();

        public static string[] GetDomainNames()
        {
            return _domainNames.ToArray();
        }

        public static string __(string index, string domain)
        {
            var domainId = _domainNames.FindIndex(s => s == domain);
            try
            {
                index = _domainList[domainId][index].ToString();
            }
            catch { }

            return index;
        }

        public static void SetLanguage(CultureInfo cultureInfo)
        {
            // Set culture if available or load defaults
            _culture = cultureInfo;
        }

        public static void LoadLanguageDomain(string domain)
        {
            // Check if already loaded
            if (!_domainNames.Contains(domain))
            {
                // Get list of available language files
                var assembly = Assembly.GetExecutingAssembly();
                var resourceList = assembly.GetManifestResourceNames();

                // Check if language is available or load defaults
                var resourceName = domain;
                if (resourceList.Contains(GetLanguageResourceName(domain, _culture.TwoLetterISOLanguageName)))
                {
                    resourceName = GetLanguageFullName(domain, _culture.TwoLetterISOLanguageName);
                }

                // Load resource file
                var addedDomain = new JObject();
                LoadLanguageResourceFile(resourceName, out addedDomain);

                // Setup strings
                _domainList.Add(addedDomain);
                _domainNames.Add(domain);

                Console.WriteLine($"Language domain \"{domain}\" loaded.");
            }
            else
            {
                Console.WriteLine($"Language domain \"{domain}\" already loaded. Skipping.");
            }
        }

        static void LoadLanguageResourceFile(string fullName, out JObject target)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = GetLanguageResourceName(fullName);

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    string jsonData = reader.ReadToEnd();
                    target = JObject.Parse(jsonData);
                }
            }
            catch {
                target = null;
            }
        }

        static string GetLanguageFullName(string name, string twoLetterCulture)
        {
            if (string.IsNullOrEmpty(twoLetterCulture))
            {
                return name;
            }
            else
            {
                return $"{name}-{twoLetterCulture}";
            }
        }

        static string GetLanguageResourceName(string name, string twoLetterCulture = "")
        {
            return GetLanguageResourceName(GetLanguageFullName(name, twoLetterCulture));
        }

        static string GetLanguageResourceName(string fullname)
        {
            return $"MineLauncher.Language.{fullname}.json";
        }
    }
}
