namespace Blog.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string? Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public int BlogPostId { get; set; }
        public virtual BlogPost BlogPost { get; set; }
        public int? AuthorId { get; set; }
        public BlogUser? Author { get; set; }
    }
}
