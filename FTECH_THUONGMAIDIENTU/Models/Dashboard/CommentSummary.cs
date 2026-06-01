using System;

namespace FTECH_THUONGMAIDIENTU.Models.Dashboard
{
    public class CommentSummary
    {
        public int CommentID { get; set; }

        public int? PostID { get; set; }

        public string PostSlug { get; set; }

        public string PostTitle { get; set; }

        public string MemberName { get; set; }

        public string MemberAvatarURL { get; set; }

        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}