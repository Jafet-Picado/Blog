using System.Drawing;
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
        public byte[]? Image { get; set; }


        public byte[]? ConvertImageToByte(string url)
        {
            try
            {
                using (FileStream fs = new FileStream(url, FileMode.Open, FileAccess.Read))
                using (MemoryStream ms = new MemoryStream())
                {
                    fs.CopyTo(ms);
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("IOException source: {0}", ex.Source);
                return null;
            }
        }

        public string ConvertByteToImage(byte[] img)
        {
            return "data:image/bmp;base64," + Convert.ToBase64String(img);
        }
    }
}
