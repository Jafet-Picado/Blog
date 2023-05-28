using Microsoft.AspNetCore.Identity;

namespace Blog.Models
{
    public class BlogUser : IdentityUser
    {
        public virtual ICollection<Comment>? Comments { get; set; }
        public virtual ICollection<BlogPost>? Posts { get; set; }
    }
}
