using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace HBSADLibrary.Models
{
    public class ADModel
    {
        [Name("EmployeeID")]
        public string EmployeeID { get; set; }

        [Name("UID")]
        public string UID { get; set; }

        [Name("Mail")]
        public string Mail { get; set; }

        [Name("SurName")]
        public string SurName { get; set; }

        [Name("GivenName")]
        public string GivenName { get; set; }

        [Name("PreferredName")]
        public string PreferredName { get; set; }

        [Name("Nickname")]
        public string Nickname { get; set; }

        [Name("Accredidation")]
        public string Accredidation { get; set; }

        [Name("jobTitle")]
        public string JobTitle { get; set; }

        [Name("Manager")]
        public string Manager { get; set; }

        [Name("businessPhones")]
        public string? businessPhones { get; set; }

        [Name("Mobile")]
        public string Mobile { get; set; }

        [Name("Department")]
        public string Department { get; set; }

        [Name("officeLocation")]
        public string officeLocation { get; set; }

        [Name("Expiration_Date")]
        public DateTime? ExpDate { get; set; }     
    }
}
