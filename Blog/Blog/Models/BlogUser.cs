using Microsoft.AspNetCore.Identity;

namespace Blog.Models
{
    public class BlogUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }
        public virtual ICollection<BlogPost>? Posts { get; set; }
    }
}
