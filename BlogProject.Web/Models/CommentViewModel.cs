namespace BlogProject.Web.Models
{
    public class CommentViewModel
    {
        public string Content { get; set; }
        public int BlogId { get; set; }
        public int? ParentCommentId { get; set; }
    }

}
