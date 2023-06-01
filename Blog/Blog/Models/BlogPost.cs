namespace Blog.Models
{
    public class BlogPost
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }
        public string? AuthorId { get; set; }
        public BlogUser? Author { get; set; }
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
    }

}
