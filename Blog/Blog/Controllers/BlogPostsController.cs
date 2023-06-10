using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Blog.Data;
using Blog.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blog.Controllers
{
    public class BlogPostsController : Controller
    {
        private readonly BlogDbContext _context;
        private readonly UserManager<BlogUser> _userManager;

        public BlogPostsController(BlogDbContext context, UserManager<BlogUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: BlogPosts
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 5; // Número de publicaciones por página

            var model = await _context.BlogPosts
                .Include(b => b.Author)
                .Include(c => c.Comments)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.BlogPosts.CountAsync();
            var pageCount = (int)Math.Ceiling((double)totalCount / pageSize);

            ViewBag.PageCount = pageCount;
            ViewBag.CurrentPage = page;            
            return View(model);
        }

        // GET: BlogPosts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.BlogPosts == null)
            {
                return NotFound();
            }

            var blogPost = await _context.BlogPosts
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
        }

        // GET: BlogPosts/Create
        [Authorize(Roles ="Admin, Author")]
        public IActionResult Create()
        {
            List<Category> categories = _context.Categories.ToList();            
            ViewBag.Categories = categories;
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id");
            return View();
        }

        // POST: BlogPosts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles ="Admin, Author")]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,CreatedAt,UpdatedAt,AuthorId,CategoryId")] BlogPost blogPost, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {                              
                blogPost.CreatedAt = DateTime.Now;

                if (imageFile != null && imageFile.Length > 0)
                {
                    byte[] imageBytes = null;

                    using (var memoryStream = new MemoryStream())
                    {
                        imageFile.CopyTo(memoryStream);
                        imageBytes = memoryStream.ToArray();
                    }

                    blogPost.Image = imageBytes;
                }
                else
                {
                    string defaultImagePath = Path.Combine("Data", "Image", "Default.jpg");
                    byte[] defaultImageBytes = null;

                    using (var fileStream = new FileStream(defaultImagePath, FileMode.Open, FileAccess.Read))
                    using (var memoryStream = new MemoryStream())
                    {
                        fileStream.CopyTo(memoryStream);
                        defaultImageBytes = memoryStream.ToArray();
                    }

                    blogPost.Image = defaultImageBytes;
                }
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                blogPost.AuthorId = string.IsNullOrEmpty(userId) ? null : userId;
                _context.Add(blogPost);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", blogPost.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", blogPost.CategoryId);
            return View(blogPost);
        }

        // GET: BlogPosts/Edit/5        
        [Authorize(Roles = "Admin, Author")]
        public async Task<IActionResult> Edit(int? id)
        {            
            if (id == null || _context.BlogPosts == null)
            {
                return NotFound();
            }

            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null)
            {
                return NotFound();
            }
            List<Category> categories = _context.Categories.ToList();
            ViewBag.Categories = categories;
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", blogPost.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", blogPost.CategoryId);
            if (User.IsInRole("Admin") || _userManager.GetUserId(User) == blogPost.AuthorId)
            {
                return View(blogPost);
            }
            return NotFound();
        }

        // POST: BlogPosts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Author")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,CreatedAt,UpdatedAt,AuthorId,CategoryId")] BlogPost blogPost, IFormFile? imageFile)
        {
            if (id != blogPost.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (User.IsInRole("Admin") || _userManager.GetUserId(User) == blogPost.AuthorId)
                {
                    try
                    {
                        var originalBlogPost = _context.BlogPosts.Find(blogPost.Id);
                        if (imageFile != null && imageFile.Length > 0)
                        {
                            byte[]? imageBytes = null;

                            using (var memoryStream = new MemoryStream())
                            {
                                imageFile.CopyTo(memoryStream);
                                imageBytes = memoryStream.ToArray();
                            }

                            originalBlogPost.Image = imageBytes;
                        }
                        originalBlogPost.Title = blogPost.Title;
                        originalBlogPost.Content = blogPost.Content;
                        originalBlogPost.UpdatedAt = DateTime.Now;
                        originalBlogPost.CategoryId = blogPost.CategoryId;
                        _context.Update(originalBlogPost);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!BlogPostExists(blogPost.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", blogPost.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", blogPost.CategoryId);
            
            return NotFound();                        
        }

        // GET: BlogPosts/Delete/5
        [Authorize(Roles = "Admin, Author")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BlogPosts == null)
            {
                return NotFound();
            }

            var blogPost = await _context.BlogPosts
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blogPost == null)
            {
                return NotFound();
            }
            if (User.IsInRole("Admin") || _userManager.GetUserId(User) == blogPost.AuthorId)
            {
                return View(blogPost);
            }
            return NotFound();
        }

        // POST: BlogPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Author")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BlogPosts == null)
            {
                return Problem("Entity set 'BlogDbContext.BlogPosts'  is null.");
            }
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (User.IsInRole("Admin") || _userManager.GetUserId(User) == blogPost.AuthorId)
            {
                if (blogPost != null)
                {
                    _context.BlogPosts.Remove(blogPost);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }

        private bool BlogPostExists(int id)
        {
          return (_context.BlogPosts?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public async Task<IActionResult> GetComments(int? id)
        {
            var comments = await _context.Comments
                .Where(c => c.BlogPostId == id)
                .ToListAsync();

            // Render the comments as an HTML string            
            return Json(comments);
        }

        public IActionResult PostsByAuthor(string id, int page=1)
        {            

            int pageSize = 5; // Número de publicaciones por página

            var model = _context.BlogPosts
                .Include(b => b.Author)
                .Include(c => c.Comments)
                .Where(p => p.AuthorId == id)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)                
                .ToList();

            var totalCount = _context.BlogPosts.Where(p => p.AuthorId == id).Count();
            var pageCount = (int)Math.Ceiling((double)totalCount / pageSize);

            ViewBag.PageCount = pageCount;
            ViewBag.CurrentPage = page;
            return View(model);
        }

        public IActionResult PostsByCategory(int id, int page = 1)
        {

            int pageSize = 5; // Número de publicaciones por página

            var model = _context.BlogPosts
                .Include(c => c.Category)
                .Include(c => c.Author)
                .Include(c => c.Comments)
                .Where(p => p.CategoryId == id)              
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)                
                .ToList();

            var totalCount = _context.BlogPosts.Where(p => p.CategoryId == id).Count();
            var pageCount = (int)Math.Ceiling((double)totalCount / pageSize);

            ViewBag.PageCount = pageCount;
            ViewBag.CurrentPage = page;
            return View(model);
        }

        public IActionResult GetAllAuthors()
        {
            var authors = _context.Users
                .Where(c => c.Posts != null && c.Posts.Count > 0)
                .Select(c => new
                {
                    Name = $"{c.FirstName} {c.LastName}",
                    c.Id
                })
                .ToList();

            return Json(authors);
        }
    }
}
