using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [NotMapped]
        public int Age
        {
            get
            {
                DateTime now = DateTime.Today;
                int age = now.Year - Birthday.Year;
                if (now < Birthday.AddYears(age)) age--;
                return age;
            }
        }
    }
}