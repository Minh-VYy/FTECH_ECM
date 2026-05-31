using System;

namespace FTECH_THUONGMAIDIENTU.Models.UserAccount
{
    public class MemberProfileSummary
    {
        public int MemberID { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string AvatarURL { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public int ClickCount { get; set; }

        public int CommentCount { get; set; }

        public int RatingCount { get; set; }
    }
}