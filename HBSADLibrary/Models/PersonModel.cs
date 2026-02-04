using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace HBSADLibrary.Models
{
    public class PersonModel
    {
        [Name("Index")]
        public int Index { get; set; }

        [Name("User Id")]
        public string UserId { get; set; }

        [Name("User Name")]
        public string UserName { get; set; }

        [Name("sAMAccountName")]
        public string sAMAccountName { get; set; }

        [Name("Display Name")]
        public string DisplayName { get; set; }

        [Name("Initials")]
        public string Initials { get; set; }

        [Name("First Name")]
        public string FirstName { get; set; }

        [Name("Last Name")]
        public string LastName { get; set; }

        [Name("Gender")]
        public string Sex { get; set; }

        [Name("Email")]
        public string Email { get; set; }

        [Name("Phone")]
        public string Phone { get; set; }

        [Name("Date of birth")]
        public DateTime DateOfBirth { get; set; }

        [Name("Job Title")]
        public string JobTitle { get; set; }
    }
}
