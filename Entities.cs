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

        public string FirstName { get; }

        public string LastName { get; }

        public string Phone { get; }

        public User(int id, string firstName = "No_FirstName", string lastName = "No_LastName", 
            string phone = "No_Phone", string username = "No_Username")
        {
            Id = id;
            Username = username;
            FirstName = firstName;
            LastName = lastName;
            Phone = phone;
        }
    }

    class UserMessages
    {
        public User User { get; }

        public List<Message> Messages { get; set; }

        public UserMessages(int id, string firstName = "No_FirstName", string lastName = "No_LastName", 
            string phone = "No_Phone", string username = "No_Username")
        {
            User = new User(id, firstName, lastName, phone, username);
            Messages = new List<Message>();
        }

        public UserMessages(User user)
        {
            User = user;
            Messages = new List<Message>();
        }
    }
}
