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

// Блок первичного подключения
try
{
    await Telegram.ConnectAsync();
    Console.WriteLine("Connected.");
}
catch
{
    Console.WriteLine("Ошибка при создании подключения. Проверьте заполнение файла Config.xml");
    Thread.Sleep(5000);
    Environment.Exit(0);
}
// Блок авторизации
var isAuth = Telegram.Client.IsUserAuthorized();
if (!isAuth)
{
    await Telegram.AuthRequestAsync();
    Console.WriteLine("Пользователь не авторизован, отправлен код на телефон. Введите код..." + "\nq - выйти.");
    string? code = null;
    while (code is null)
    {
        code = Console.ReadLine();
        if (code == "q") Environment.Exit(0);
    }
    try
    {
        await Telegram.AuthAsync(code);
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
// Получить ссылки на пользователей, указанных в конфигурации
await Telegram.SetUsersAsync();
//
while (true)
{
    // Получить непрочитанные сообщения от указанного юзера
    await Telegram.GetUnreadMessages();
    // Отправить указанному юзеру непрочитанные сообщения указанного юзера
    await Telegram.SendUnreadMessages();
    // Вывести непрочитанные сообщения в консоль
    Telegram.ShowMessages();
    //
    Console.WriteLine("---------------------------------");
    // Задержка, чтобы не спамить
    Thread.Sleep(5000);
}


Console.WriteLine("======================================");
