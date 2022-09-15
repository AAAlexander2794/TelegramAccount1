// See https://aka.ms/new-console-template for more information
using System;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;

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


Console.WriteLine("======================================");
