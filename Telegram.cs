using System;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;
using static System.Net.Mime.MediaTypeNames;


namespace ConsoleApp2
{
    internal static class Telegram
    {
        static int ApiId { get; }

        static string ApiHash { get; }

        static string PhoneNumber { get; }

        static string Code { get; }

        static string UsernameFrom { get; }

        static string UsernameTo { get; }

        public static TelegramClient Client { get; }

        static TLUser UserFrom { get; set; }

        static TLUser UserTo { get; set; }

        static string AuthHash { get; set; }

        static List<TLMessage> Messages { get; set;}

        static Telegram()
        {
            try
            {
                //
                var config = ConfigManager.Config;
                ApiId = config.ApiId;
                ApiHash = config.ApiHash;
                PhoneNumber = config.PhoneNumber;
                Code = config.Code;
                UsernameFrom = config.UserNameFrom;
                UsernameTo = config.UserNameTo;
                //
                Messages = new List<TLMessage>();
                // Записываем адрес сессии, чтобы можно было чекнуть авторизацию
                var store = new FileSessionStore();
                Client = new TelegramClient(ApiId, ApiHash, store, "session");
            }
            catch
            {
                Console.WriteLine("Ошибка при создании подключения. Проверьте заполнение файла Config.");
            }
        }

        public static async Task ConnectAsync()
        {
            await Client.ConnectAsync();
        }

        /// <summary>
        /// Проверка авторизации (для этого надо, чтобы <see cref="Client"/> был создан с указанием <see cref="ISessionStore"/> и названия)
        /// </summary>
        /// <returns></returns>
        public static bool IsUserAuth()
        {
            return Client.IsUserAuthorized();
        }
       
        /// <summary>
        /// Запрос кода с телефона
        /// </summary>
        public static async Task AuthRequestAsync()
        {
            AuthHash = await Client.SendCodeRequestAsync(PhoneNumber);
        }

        /// <summary>
        /// Авторизация по коду с телефона, требует сначала выполнения запроса на получение кода <see cref="AuthRequestAsync"/>
        /// </summary>
        /// <param name="code">Код с телефона</param>
        public static async Task AuthAsync(string code)
        {
            await Client.MakeAuthAsync(PhoneNumber, AuthHash, code);
        }

        /// <summary>
        /// Устанавливает пользователя-отправителя и пользователя-реципиента из файла конфигурации.
        /// </summary>
        /// <returns></returns>
        public static async Task SetUsersAsync()
        {
            await SetUsersAsync(UsernameFrom, UsernameTo);
        }

        /// <summary>
        /// Задает свойства <see cref="UserFrom"/> и <see cref="UserTo"/>
        /// </summary>
        /// <param name="usernameFrom"></param>
        /// <param name="usernameTo"></param>
        public static async Task SetUsersAsync(string usernameFrom, string usernameTo)
        {
            var result = await Client.GetContactsAsync(); //overflow error?
            //
            UserFrom = result.Users
                .Where(x => x.GetType() == typeof(TLUser))
                .Cast<TLUser>()
                .Where(x => x.Username == usernameFrom).First();
            //
            UserTo = result.Users
                .Where(x => x.GetType() == typeof(TLUser))
                .Cast<TLUser>()
                .Where(x => x.Username == usernameTo).First();
        }

        /// <summary>
        /// Отправляет сообщение
        /// </summary>
        /// <param name="user"></param>
        /// <param name="text"></param>
        static async void SendMessageAsync(TLUser user, string text)
        {
            await Client.SendMessageAsync(new TLInputPeerUser() { UserId = user.Id }, text);
        }

        /// <summary>
        /// Получает непрочитанные сообщения от пользователя <see cref="UserFrom"/>.
        /// </summary>
        /// <returns></returns>
        public static async Task GetUnreadMessages()
        {
            //
            var dialogs = await Client.GetUserDialogsAsync() as TLDialogsSlice;;
            // Получать будем сообщения от UserFrom
            var target = new TLInputPeerUser { UserId = UserFrom.Id };
            if (target == null)
            {
                Console.WriteLine("Не найдено, от кого читать");
                return;
            }
            foreach (var dia in dialogs.Dialogs.Where(x => x.UnreadCount > 0))
            {
                TLPeerUser peer = dia.Peer as TLPeerUser;
                if (peer == null || peer.UserId != target.UserId) continue;
                var hist = await Client.GetHistoryAsync(target, 0, -1, 0, dia.UnreadCount);
                var some2 = (TLMessagesSlice)hist;
                Messages.Clear();
                foreach (TLMessage m in some2.Messages)
                {
                    Messages.Add(m);
                }
            }
            Console.WriteLine("Got the messages");
        }

        /// <summary>
        /// Получает сообщения от пользователя <see cref="UserFrom"/>.
        /// </summary>
        /// <param name="numberOfMessages">Количество сообщений на вывод</param>
        /// <returns></returns>
        public static async Task GetMessages(int numberOfMessages)
        {
            //
            //var dialogs = await Client.GetUserDialogsAsync() as TLDialogsSlice; ;
            // Получать будем сообщения от UserFrom
            var target = new TLInputPeerUser { UserId = UserFrom.Id };
            if (target == null)
            {
                Console.WriteLine("Не найдено, от кого читать");
                return;
            }
            var hist = await Client.GetHistoryAsync(target, 0, -1, 0, numberOfMessages);
            var some2 = (TLMessagesSlice)hist;
            Messages.Clear();
            foreach (TLMessage m in some2.Messages)
            {
                Messages.Add(m);
            }
            Console.WriteLine("Got the messages");
        }

        /// <summary>
        /// Вывод сообщений от <see cref="Messages"/> на консоль.
        /// </summary>
        public static void ShowMessages()
        {
            if (Messages.Count == 0)
            {
                Console.WriteLine("Нет сообщений для вывода на экран.");
                return;
            }
            string text = "";
            foreach (var m in Messages)
            {
                text = text + "\n" + m.Message;
            }
            // Отправить сообщения UserTo
            Console.WriteLine("Сообщения от " + UserFrom.Username + ":\n" + text);
        }

        public static async Task SendUnreadMessages()
        {
            //await GetUnreadMessages();
            if (Messages.Count == 0)
            {
                Console.WriteLine("There is no unread messages to send.");
                return;
            }
            string text = "";
            foreach (var m in Messages)
            {
                text = text + "\n" + m.Message;
            }
            // Отправить сообщения UserTo
            await Client.SendMessageAsync(new TLInputPeerUser() { UserId = UserTo.Id }, "Непрочитанные сообщения от " + UserFrom.Username + ":\n" + text);
            Console.WriteLine("Unread messages are sended");
        }


        //      if (firstMessage > 0)
        //      {
        //          var readed = New TeleSharp.TL.Messages.TLRequestReadHistory{
        //.Peer = target, 
        //.Dirty = True, 
        //.MessageId = firstMessage, 
        //.MaxId = -1, 
        //.ConfirmReceived = True, 
        //.Sequence = dia.UnreadCount

        //    };
        //          var affectedMessages = await client.SendRequestAsync<TLAffectedMessages>(readed);
        //          Thread.Sleep(5000);
        //      }
        //  }

    }
}
