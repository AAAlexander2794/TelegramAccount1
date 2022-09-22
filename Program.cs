// See https://aka.ms/new-console-template for more information
using ConsoleApp2;
using System;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;

Console.WriteLine("======================================");
Console.WriteLine("Все настройки хранятся в файле Config.xml");
var config = ConfigManager.Config;
// Блок первичного подключения
try
{
    await Telegram.CreateSession(config.ApiId, config.ApiHash);
    Console.WriteLine("Connected.");
}
catch
{
    Console.WriteLine("Ошибка при создании подключения. Проверьте заполнение файла Config.xml");
    Thread.Sleep(5000);
    Environment.Exit(0);
}
// Блок авторизации
if (!Telegram.IsUserAuth())
{
    await Telegram.AuthRequestAsync(config.PhoneNumber);
    Console.WriteLine("Пользователь не авторизован, отправлен код на телефон. Введите код..." + "\nq - выйти.");
    string? code = null;
    while (code is null)
    {
        code = Console.ReadLine();
        if (code == "q") Environment.Exit(0);
    }
    try
    {
        await Telegram.AuthAsync(config.PhoneNumber, code);
        Console.WriteLine("Теперь пользователь авторизован.");
    }
    catch
    {
        Console.WriteLine("При авторизации что-то пошло не так. Прощайте.");
        Thread.Sleep(3000);
        Environment.Exit(0);
    }
}
else
{
    Console.WriteLine("Authorized.");
}
// Загружаем список контактов
await Telegram.RecieveContacts();
// 
User userFrom = Telegram.GetUser(config.UsernameFrom);
User userTo = Telegram.GetUser(config.UsernameTo);
//
while (true)
{
    List<UserMessages> result;
    if (config.IsReadAll)
    {
        // Получаем все непрочитанные сообщения от пользователей
        result = await Telegram.GetUnreadUserMessagesAsync();
    }
    else 
    {
        // Создаем контейнер пользователя
        result = new List<UserMessages>();
        var some = new UserMessages(userFrom);
        // Получаем непрочитанные сообщения от указанного пользователя
        some.Messages = await Telegram.GetUnreadMessagesAsync(userFrom.Id);
        result.Add(some);
    }
  
    // Отправить указанному юзеру непрочитанные сообщения указанного юзера
    await Telegram.SendUserMessages(userTo.Id, result);
    // Вывести непрочитанные сообщения в консоль
    Telegram.ShowUserMessages(result);
    //
    Console.WriteLine("---------------------------------");
    // Задержка, чтобы не спамить
    Thread.Sleep(5000);
}


Console.WriteLine("======================================");
