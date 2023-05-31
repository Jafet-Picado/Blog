using Microsoft.AspNetCore.Identity;

namespace Blog.Models
{
    public class BlogUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }
        public virtual ICollection<BlogPost>? Posts { get; set; }
    }
}
