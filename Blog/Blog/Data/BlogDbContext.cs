using Blog.Models;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data
{
    public class BlogDbContext : DbContext
    {
        public DbSet<BlogPost>? BlogPosts { get; set; }
        public DbSet<Comment>? Comments { get; set; }

        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure table names if needed
            modelBuilder.Entity<BlogPost>().ToTable("BlogPost");
            modelBuilder.Entity<Comment>().ToTable("Comment");

            base.OnModelCreating(modelBuilder);
        }
    }
}
