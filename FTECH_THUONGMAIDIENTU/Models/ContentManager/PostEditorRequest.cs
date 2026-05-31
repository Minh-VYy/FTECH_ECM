namespace FTECH_THUONGMAIDIENTU.Models.ContentManager
{
    public class PostEditorRequest
    {
        public int? PostID { get; set; }

        public string Title { get; set; }

        public string Slug { get; set; }

        public string Content { get; set; }

        public string ThumbnailURL { get; set; }

        public int CategoryID { get; set; }

        public string Status { get; set; }

        public string RejectionReason { get; set; }
    }
}