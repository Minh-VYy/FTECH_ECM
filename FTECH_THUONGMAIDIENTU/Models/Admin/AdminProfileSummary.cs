using System;

namespace FTECH_THUONGMAIDIENTU.Models.Admin
{
    public class AdminProfileSummary
    {
        public int AdminID { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string RoleName { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}