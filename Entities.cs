using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Message
    {
        public int Id { get; }

        public string Text { get; }

        public int? UserId { get; }

        public bool IsSent { get; set; }

        public bool IsShown { get; set; }

        public bool IsUnread { get; set; }

        public Message(int id, string text, int? userId)
        {
            Id = id;
            Text = text;
            UserId = userId;
        }
    }

    class User
    {
        public int Id { get; }

        public string Username { get; }

        public User(int id, string username)
        {
            Id = id;
            Username = username;
        }
    }

    class UserMessages
    {
        public User User { get; }

        public List<Message> Messages { get; set; }

        public UserMessages(int id, string username)
        {
            User = new User(id, username);
            Messages = new List<Message>();
        }
    }
}
