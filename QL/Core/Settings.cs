/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  Settings
 *
 * ***********************************************/


namespace QL.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Configuration;
    using System.IO;
    using System.Collections.Specialized;
    using System.Xml;
    using QL.Core.Extensions;
    using QL.Core.Caching;

    /// <summary>
    /// 参数配置,主要用于代替.NET中的AppSettings
    /// </summary>

    public static class Settings
    {
        public static NameValueCollection From(string fileName)
        {
            fileName = ToRelativeBasePath(fileName);
            if (!File.Exists(fileName))
            {
                return new NameValueCollection();
            }
            string key = "QL.SETTINGS.FILES." + fileName;
            NameValueCollection values = MemoryCaching.Default.Get(key) as NameValueCollection;
            if (values == null)
            {
                List<string> dependFiles = new List<string>();
                values = LoadFromFile(fileName, dependFiles);
                MemoryCaching.Default.Set<string[]>(key, dependFiles.ToArray());
            }
            return values;
        }

        public static T GetFrom<T>(string fileName) where T : class, new()
        {
            fileName = ToRelativeBasePath(fileName);
            if (!File.Exists(fileName))
            {
                return default(T);
            }
            string key = "QL.SETTINGS.CLASSES." + fileName;
            T local = MemoryCaching.Default.Get(key) as T;
            if (local == null)
            {
                List<string> dependFiles = new List<string>();
                NameValueCollection collection = LoadFromFile(fileName, dependFiles);
                local = Activator.CreateInstance<T>();
                collection.CopyTo<T>(local);
                MemoryCaching.Default.Set<T>(key, local, dependFiles.ToArray());
            }
            return local;
        }

        private static NameValueCollection LoadFromFile(string fileName, List<string> dependFiles)
        {
            dependFiles.Add(fileName);
            NameValueCollection values = new NameValueCollection();
            using (XmlTextReader reader = new XmlTextReader(fileName))
            {
                while (reader.Read())
                {
                    if (((reader.Depth == 1) && (reader.NodeType == XmlNodeType.Element)) && !string.IsNullOrEmpty(reader.Name))
                    {
                        string name = reader.Name;
                        string attribute = null;
                        if ((reader.HasAttributes && name.Equals("add", StringComparison.InvariantCultureIgnoreCase)) && !string.IsNullOrEmpty(reader.GetAttribute("key")))
                        {
                            name = reader.GetAttribute("key");
                            attribute = reader.GetAttribute("value");
                        }
                        else
                        {
                            if ((reader.HasAttributes && name.Equals("include", StringComparison.InvariantCultureIgnoreCase)) && !string.IsNullOrEmpty(reader.GetAttribute("file")))
                            {
                                string path = reader.GetAttribute("file");
                                if (!Utility.IsAbsolutePath(path))
                                {
                                    path = Path.Combine(Path.GetDirectoryName(fileName), path);
                                }
                                if (File.Exists(path))
                                {
                                    NameValueCollection values2 = LoadFromFile(path, dependFiles);
                                    for (int i = 0; i < values2.Count; i++)
                                    {
                                        string key = values2.GetKey(i);
                                        string str5 = values2[key];
                                        if ((key != null) && (str5 != null))
                                        {
                                            values.Set(key, str5);
                                        }
                                    }
                                }
                                continue;
                            }
                            attribute = reader.ReadString();
                        }
                        if (!string.IsNullOrEmpty(name))
                        {
                            values.Set(name, attribute);
                        }
                    }
                }
            }
            return values;
        }

        public static string ToRelativeBasePath(string fileName)
        {
            if (Utility.IsAbsolutePath(fileName))
            {
                return fileName;
            }
            return Path.Combine(BasePath, fileName.TrimStart(new char[] { '~', '\\', '/' }));
        }

        public static string BasePath
        {
            get
            {
                string str = ConfigurationManager.AppSettings["QL.SETTINGS.BASEPATH"];
                if (string.IsNullOrEmpty(str))
                {
                    str = "~/config/";
                }
                return Utility.ToAbsolutePath(str);
            }
        }

        public static NameValueCollection Default
        {
            get
            {
                return From("settings.config");
            }
        }
    }

}
