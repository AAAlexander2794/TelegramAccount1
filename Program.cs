// See https://aka.ms/new-console-template for more information
using System;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TeleSharp.TL.Updates;
using TLSchema.Messages;
using TLSharp.Core;
using static System.Net.Mime.MediaTypeNames;
using TLDialogsSlice = TeleSharp.TL.Messages.TLDialogsSlice;
using TLInputPeerChat = TeleSharp.TL.TLInputPeerChat;
using TLMessages = TeleSharp.TL.Messages.TLMessages;
using TLMessagesSlice = TeleSharp.TL.Messages.TLMessagesSlice;

Console.WriteLine("======================================");

var apiId = 11533761;
var apiHash = "96d93a118d65971b80259ccc5b1c70fb";

var client = new TelegramClient(apiId, apiHash);
await client.ConnectAsync();

//var hash = await client.SendCodeRequestAsync("+79185351854");
////Environment.Exit(0);
//var code = "26513"; // you can change code in debugger
////var t = Console.ReadLine();
////code = t;
//var user = await client.MakeAuthAsync("+79185351854", hash, code);

//find recipient in contacts
var result = await client.GetContactsAsync();
var user1 = result.Users
    .Where(x => x.GetType() == typeof(TLUser))
    .Cast<TLUser>()
    .Where(x => x.Username == "AnRostov");
//if (user1.ToList().Count != 0)
//{
//    foreach (var u in user1)
//    {
//        //send message
//        await client.SendMessageAsync(new TLInputPeerUser() { UserId = u.Id }, "test message");
//    }
//}

var dialogs = await client.GetUserDialogsAsync() as TLDialogsSlice;
var users = dialogs.Users.ToList();

var target = new TLInputPeerUser { UserId = user1.First().Id };

foreach (var dia in dialogs.Dialogs.Where(x => x.UnreadCount > 0))
{
    TLPeerUser peer = dia.Peer as TLPeerUser;
    if (peer == null || peer.UserId != target.UserId) continue;
    if (target == null) break;
    var hist = await client.GetHistoryAsync(target, 0, -1, 0, dia.UnreadCount);
    var some2 = (TLMessagesSlice)hist;
    string text = "";
    foreach (TLMessage m in some2.Messages)
    {

        
            Console.WriteLine("{0} {1} {2}", m.Id, m.Message, m.FromId);
        text = text + "\n" + m.Message;
        
    }
    await client.SendMessageAsync(new TLInputPeerUser() { UserId = target.UserId }, "Я еще не прочитал сообщения: " + text);
}
//foreach (TLUser user in result.Users)
//{
//    if (user.Username == user1.First().Username)
//    {
//        Console.WriteLine(user.Username);
//        var target = new TLInputPeerUser { UserId = user.Id};
//        var hist = await client.GetHistoryAsync(target, 0, -1, 0);
//        var some2 = (TLMessagesSlice)hist;
        
//        foreach (TLMessage m in some2.Messages)
//        {
            
//            if (dialogs.Dialogs.Where(x => x.Peer is TL && == target.UserId))
//            Console.WriteLine("{0} {1} {2}", m.Id, m.Message, m.FromId);
//        }
//    }
//}
foreach (var dia in dialogs.Dialogs.ToList())
{
    
    //if (dia.UnreadCount < 1) continue;

    //if (dia.Peer is TLPeerUser)
    //{
    //    var peer = dia.Peer as TLPeerUser;

    //OfType(TLUser).First(x => x.id == peer.UserId);
    //      var target = new TLInputPeerUser{UserId = chat.Id, AccessHash = chat.AccessHash};
    //      var hist = await client.GetHistoryAsync(target, 0, -1, dia.UnreadCount);
    //      int firstMessage = 0;

    //      if (hist is TLMessagesSlice)
    //      {
    //          int index = 0;
    //          foreach (var m in ((TLMessageSlice)hist).Messages.GetList)
    //          {
    //              TLMessage msg = m as TLMessage;

    //              if (index == 0) firstMessage = msg.Id;

    //              Console.WriteLine("{0} {1} {2}", msg.Id, msg.Message, msg.FromId);
    //          }
    //      }
    //      else if (hist is TLMessages)
    //      {
    //          int index = 0;
    //          foreach (var m in ((TLMessage)hist).Messages.GetList)
    //          {
    //              TLMessage msg = m as TLMessage;

    //              if (index == 0) firstMessage = msg.Id;

    //              Console.WriteLine("{0} {1} {2}", msg.Id, msg.Message, msg.FromId);
    //          }
    //      }

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

//Console.WriteLine(text);

//await client.SendMessageAsync(new TLInputPeerUser() { UserId = user1.First().Id }, text);

//while (true)
//{
//    break;
//        var dialogs = await client.GetUserDialogsAsync() as TLDialogsSlice;
//    if (dialogs == null)
//    {
//        Console.WriteLine("dialogs are empty");
//        break;
//    }
    

//    foreach (var dia in dialogs.Dialogs.Where(x => x.Peer is TLPeerChannel && x.UnreadCount > 0))
//    {
//        var peer = dia.Peer as TLPeerChannel;
//        var chat = dialogs.Chats.ToList().OfType<TLChannel>().First(x => x.Id == peer.ChannelId);
//        var target = new TLInputPeerChannel() { ChannelId = chat.Id, AccessHash = (long)chat.AccessHash };
//        var hist = await client.GetHistoryAsync(target, 0, -1, dia.UnreadCount) as TLChannelMessages;
//        Console.WriteLine("=====================================================================");
//        Console.WriteLine("THIS IS:" + chat.Title + " WITH " + dia.UnreadCount + " UNREAD MESSAGES NOW READED");
//        foreach (var m in hist.Messages.ToList().OfType<TLMessage>())
//        {
//            Console.WriteLine(m.Message);
//            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~");
//        }

//        ////marking them as read
//        //var ch = new TLInputChannel() { ChannelId = target.ChannelId, AccessHash = target.AccessHash };
//        //var markAsRead = new TeleSharp.TL.Channels.TLRequestReadHistory()
//        //{
//        //    Channel = ch,
//        //    MaxId = -1,
//        //    Dirty = true,
//        //    MessageId = (hist.Messages.ToList().First() as TLMessage).Id,
//        //    ConfirmReceived = true,
//        //    Sequence = dia.UnreadCount
//        //};
//        //Console.Write(await client.SendRequestAsync<bool>(markAsRead));
//    }

//    //string text = Console.ReadLine();
//    //if (string.IsNullOrEmpty(text)) break;
//    ////get available contacts
//    //var result = await client.GetContactsAsync();

//    ////find recipient in contacts
//    //var user1 = result.Users
//    //    .Where(x => x.GetType() == typeof(TLUser))
//    //    .Cast<TLUser>()
//    //    .Where(x => x.Username == "AnRostov");
//    //if (user1.ToList().Count != 0)
//    //{
//    //    foreach (var u in user1)
//    //    {
//    //        //send message
//    //        await client.SendMessageAsync(new TLInputPeerUser() { UserId = u.Id }, text.ToString());
//    //    }
//    //}

//}




Console.WriteLine("======================================");
