using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    public class ConfigInfo
    {
        /// <summary>
        /// Идентификатор приложения Телеграм
        /// </summary>
        public int ApiId { get; set; }

        /// <summary>
        /// Хэш приложения Телеграм
        /// </summary>
        public string ApiHash { get; set; }

        /// <summary>
        /// Номер телефона
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Имя пользователя в Телеграм, от которого получаем сообщения
        /// </summary>
        public string UsernameFrom { get; set; }

        /// <summary>
        /// Имя пользователя в Телеграм, которому отправляем сообщения
        /// </summary>
        public string UsernameTo { get; set; }

        /// <summary>
        /// Флаг чтения от всех пользователей
        /// </summary>
        public bool IsReadAll { get; set; }
    }

    /// <summary>
    /// Класс работы с файлом конфигурации
    /// </summary>
    static class ConfigManager
    {
        public static ConfigInfo Config { get; }

        static ConfigManager()
        {
            Config = new ConfigInfo();
            try
            {
                System.Xml.Serialization.XmlSerializer reader =
                    new System.Xml.Serialization.XmlSerializer(typeof(ConfigInfo));
                System.IO.StreamReader file = new System.IO.StreamReader(
                    Path.GetFullPath("Config.xml"));
                Config = (ConfigInfo)reader.Deserialize(file);
                file.Close();
            }
            catch
            {
                // Значения по умолчанию
                Config.ApiId = 10000001;
                Config.ApiHash = "abc";
                Config.PhoneNumber = "+70000000001";
                Config.UsernameFrom = "UserFrom";
                Config.UsernameTo = "UserTo";
                Config.IsReadAll = true;
                //
                WriteConfig();
                Console.WriteLine("Файл Config.xml создан. Заполните его данными и перезапустите программу");
                Thread.Sleep(5000);
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Записывает в файл конфиг текущую конфигурацию
        /// </summary>
        static void WriteConfig()
        {
            System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(ConfigInfo));

            var path = Path.GetFullPath("Config.xml");
            System.IO.FileStream file = System.IO.File.Create(path);

            writer.Serialize(file, Config);
            file.Close();
        }
    }
}
