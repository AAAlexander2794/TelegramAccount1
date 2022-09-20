using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    public class Configuration
    {
        public int ApiId { get; set; }

        public string ApiHash { get; set; }

        public string PhoneNumber { get; set; }

        public string Code { get; set; }

        public string UserNameFrom { get; set; }

        public string UserNameTo { get; set; }
    }

    /// <summary>
    /// Класс работы с файлом конфигурации
    /// </summary>
    static class ConfigManager
    {
        public static Configuration Config { get; }

        static ConfigManager()
        {
            Config = new Configuration();
            try
            {
                System.Xml.Serialization.XmlSerializer reader =
        new System.Xml.Serialization.XmlSerializer(typeof(Configuration));
                System.IO.StreamReader file = new System.IO.StreamReader(
                    Path.GetFullPath("Config.xml"));
                Config = (Configuration)reader.Deserialize(file);
                file.Close();
            }
            catch
            {
                // Значения по умолчанию
                Config.ApiId = 10000001;
                Config.ApiHash = "abc";
                Config.PhoneNumber = "+70000000001";
                Config.Code = "10001";
                Config.UserNameFrom = "UserFrom";
                Config.UserNameTo = "UserTo";
                //
                WriteConfig();
                Console.WriteLine("Файл Config.xml создан. Заполните его данными и перезапустите программу");
            }
        }

        /// <summary>
        /// Записывает в файл конфиг текущую конфигурацию
        /// </summary>
        static void WriteConfig()
        {
            System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(Configuration));

            var path = Path.GetFullPath("Config.xml");
            System.IO.FileStream file = System.IO.File.Create(path);

            writer.Serialize(file, Config);
            file.Close();
        }
    }
}
