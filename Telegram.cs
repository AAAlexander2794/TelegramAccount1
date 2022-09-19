using System;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;


namespace ConsoleApp2
{
    internal static class Telegram
    {
        static int ApiId { get; }

        static string ApiHash { get; }

        static TelegramClient Client { get; }

        static TLUser UserFrom { get; set; }

        static TLUser UserTo { get; set; }

        static string PhoneNumber { get; }

        static string Code { get; }

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
                //
                Client = new TelegramClient(ApiId, ApiHash);
                ConnectAsync();
                // *** Здесь должна происходить авторизация
                //
            }
            catch
            {
                Console.WriteLine("Ошибка при создании подключения. Проверьте заполнение файла Config.");
            }
        }

        static async void ConnectAsync()
        {
            await Client.ConnectAsync();
        }
       
        static async void AuthAsync()
        {
            var hash = await Client.SendCodeRequestAsync("+79185351854");
            var code = "26513"; // you can change code in debugger
            var t = Console.ReadLine();
            code = t;
            var user = await Client.MakeAuthAsync("+79185351854", hash, code);
        }

        /// <summary>
        /// Задает свойства <see cref="UserFrom"/> и <see cref="UserTo"/>
        /// </summary>
        /// <param name="usernameFrom"></param>
        /// <param name="usernameTo"></param>
        static async void SetUsersAsync(string usernameFrom, string usernameTo)
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

        public static async void MyTask()
        {
            var config = ConfigManager.Config;
            // Присваиваем пользователей, с которыми будем работать
            SetUsersAsync(config.UserNameFrom, config.UserNameTo);
            //
            var dialogs = await Client.GetUserDialogsAsync() as TLDialogsSlice;
            //var users = dialogs.Users.ToList();
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
                //if (peer == null || peer.UserId != target.UserId) continue;
                
                var hist = await Client.GetHistoryAsync(target, 0, -1, 0, dia.UnreadCount);
                var some2 = (TLMessagesSlice)hist;
                string text = "";
                foreach (TLMessage m in some2.Messages)
                {


                    Console.WriteLine("{0} {1} {2}", m.Id, m.Message, m.FromId);
                    text = text + "\n" + m.Message;

                }
                // Отправить сообщения UserTo
                // await Client.SendMessageAsync(new TLInputPeerUser() { UserId = UserTo.Id }, "Я еще не прочитал сообщения: " + text);
            }
            Console.WriteLine("Done");
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
