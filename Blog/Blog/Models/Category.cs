﻿namespace Blog.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<BlogPost>? BlogPosts { get; set; }
    }
}
