using System;
using System.ComponentModel.DataAnnotations;

namespace SpeedDatingBot.Models
{
    public class User
    {
        [Key] 
        public ulong Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsGirl { get; set; }
        public DateTime Birthday { get; set; }
    }
}