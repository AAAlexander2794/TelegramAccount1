using System;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using TeleSharp.TL;
using TeleSharp.TL.Contacts;
using TeleSharp.TL.Messages;
using TLSharp.Core;


namespace ConsoleApp2
{
    internal static class Telegram
    {
        /// <summary>
        /// Клиент телеграма, через который происходят все действия
        /// </summary>
        static TelegramClient Client { get; set; }

        /// <summary>
        /// Для авторизации нужно
        /// </summary>
        static string AuthHash { get; set; }

        /// <summary>
        /// Список контактов клиента
        /// </summary>
        static TLContacts? Contacts { get; set; } 

        /// <summary>
        /// Диалоги клиента
        /// </summary>
        static TLDialogsSlice? Dialogs { get; set; }

        static List<int> sendMessageIds { get; }

        static Telegram()
        {
            sendMessageIds = new List<int>();
        }

        /// <summary>
        /// Создание сессии в Телеграме
        /// </summary>
        /// <param name="apiId"></param>
        /// <param name="apiHash"></param>
        public static async Task CreateSession(int apiId, string apiHash)
        {
            // Записываем адрес сессии, чтобы можно было чекнуть авторизацию
            var store = new FileSessionStore();
            Client = new TelegramClient(apiId, apiHash, store, "session");
            // Первичное подключение
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
        public static async Task AuthRequestAsync(string phoneNumber)
        {
            AuthHash = await Client.SendCodeRequestAsync(phoneNumber);
        }

        /// <summary>
        /// Авторизация по коду с телефона, требует сначала выполнения запроса на получение кода <see cref="AuthRequestAsync"/>
        /// </summary>
        /// <param name="code">Код с телефона</param>
        public static async Task AuthAsync(string phoneNumber, string code)
        {
            await Client.MakeAuthAsync(phoneNumber, AuthHash, code);
        }

        /// <summary>
        /// Получает список контактов из Телеграма
        /// </summary>
        public static async Task RecieveContacts()
        {
            Contacts = await Client.GetContactsAsync();
        }

        /// <summary>
        /// Возвращает Id пользователя по Username из <see cref="Contacts"/>
        /// </summary>
        /// <param name="username"></param>
        public static User? GetUser(string username)
        {
            if (Contacts == null) return null;
            //
            var q = Contacts.Users
                .Where(x => x.GetType() == typeof(TLUser))
                .Cast<TLUser>().First(x => x.Username == username);
            //
            var user = new User(q.Id, q.FirstName, q.LastName, q.Phone, q.Username);
            return user;
        }

        /// <summary>
        /// Возвращает Username пользователя по Id из <see cref="Contacts"/>
        /// </summary>
        /// <param name="userId"></param>
        public static User? GetUser(int userId)
        {
            if (Contacts == null) return null;
            var q = Contacts.Users
                .Where(x => x.GetType() == typeof(TLUser))
                .Cast<TLUser>().First(x => x.Id == userId);
            //
            var user = new User(q.Id, q.FirstName, q.LastName, q.Phone, q.Username);
            return user;
        }

        /// <summary>
        /// Отправляет сообщение
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="text"></param>
        static async void SendMessageAsync(int userId, string text)
        {
            await Client.SendMessageAsync(new TLInputPeerUser() { UserId = userId }, text);
        }

        /// <summary>
        /// Получает все диалоги клиента
        /// </summary>
        /// <returns></returns>
        static async Task RecieveDialogsAsync()
        {
            Dialogs = await Client.GetUserDialogsAsync() as TLDialogsSlice;
        }

        /// <summary>
        /// Получает непрочитанные сообщения пользователя из <see cref="Dialogs"/>.
        /// </summary>
        /// <remarks>
        /// Если вы отправили пользователю сообщение после его нерпочитанных сообщений, то оно будет включено в список.
        /// </remarks>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static async Task<List<Message>> GetUnreadMessagesAsync(int userId)
        {
            await RecieveDialogsAsync();
            if (Dialogs == null) return new List<Message>();
            // Количество непрочитанных сообщений в диалоге
            var dia = Dialogs.Dialogs
                .FirstOrDefault(x => x.UnreadCount > 0 && x.Peer is TLPeerUser user && user.UserId == userId);
            int unreadCount;
            if (dia == null)
            {
                unreadCount = 0;
            }
            else
            {
                unreadCount = dia.UnreadCount;
            };
            return await GetMessagesAsync(userId, unreadCount);
        }

        /// <summary>
        /// Возвращает сообщения из чата с пользователем (свои тоже) 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static async Task<List<Message>> GetMessagesAsync(int userId, int count)
        {
            // Создаем список сообщений
            var messages = new List<Message>();
            if (count > 0)
            {
                // Создаем пира по Id пользователя
                var target = new TLInputPeerUser { UserId = userId };
                // Получаем историю сообщений
                var hist = (TLMessagesSlice)await Client.GetHistoryAsync(target, 0, -1, 0, count);
                //
                foreach (TLMessage m in hist.Messages)
                {
                    messages.Add(new Message(m.Id, m.Message, m.FromId));
                }
            }
            return messages;
        }

        /// <summary>
        /// Возвращает контейнер с непрочитанными сообщениями от разных пользователей из <see cref="Dialogs"/>
        /// </summary>
        /// <returns></returns>
        public static async Task<List<UserMessages>> GetUnreadUserMessagesAsync()
        {
            await RecieveDialogsAsync();
            var result = new List<UserMessages>();
            if (Dialogs == null) return result;

            // В каждом диалоге с непрочитанными сообщениями
            foreach (var dia in Dialogs.Dialogs.Where(x => x.UnreadCount > 0))
            {
                var peer = dia.Peer as TLPeerUser;
                if (peer == null) continue;
                var user = GetUser(peer.UserId);
                if (user == null) continue;
                var userMessages = new UserMessages(user);
                // Получаем непрочитанные сообщения этого диалога
                userMessages.Messages = await GetMessagesAsync(peer.UserId, dia.UnreadCount);
                //
                result.Add(userMessages);
            }
            return result;
        }

        /// <summary>
        /// Вывод сообщений на консоль.
        /// </summary>
        public static void ShowUserMessages(List<UserMessages> userMessages)
        {
            string text = "";
            foreach (var um in userMessages)
            {
                if (um.Messages.Count == 0) continue;
                text = text + um.User.FirstName + " " + um.User.LastName + ":\n";
                foreach (var m in um.Messages)
                {
                    text = text + m.Text + "\n";
                }
            }
            if (text != "")
            {
                Console.WriteLine("Сообщения:\n" + text);
            }
            else
            {
                Console.WriteLine("Нет новых сообщений");
            }
            
        }

        public static async Task SendUserMessages(int userId, List<UserMessages> userMessages, string? header = "")
        {
            string text = "";
            foreach (var um in userMessages)
            {
                if (um.Messages.Count == 0) continue;
                var subtext = "";
                foreach (var m in um.Messages)
                {
                    if (sendMessageIds.Contains(m.Id)) continue;
                    subtext = subtext + m.Text + "\n";
                    sendMessageIds.Add(m.Id);
                }
                if (subtext != "")
                {
                    text = text + um.User.FirstName + " " + um.User.LastName + ":\n";
                    text = text + subtext;
                }
            }
            if (text != "")
            {
                // Отправить сообщения UserTo
                await Client.SendMessageAsync(new TLInputPeerUser() { UserId = userId }, header + "\n" + text);
                Console.WriteLine("Unread messages are sended");
            }
            else
            {
                Console.WriteLine("No messages to send.");
            }
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
