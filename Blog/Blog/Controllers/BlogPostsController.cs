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
        [Authorize(Roles ="Author")]
        public IActionResult Create()
        {
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id");
            return View();
        }

        // POST: BlogPosts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,CreatedAt,UpdatedAt,AuthorId,CategoryId")] BlogPost blogPost, IFormFile? imageFile)
        {

            if (ModelState.IsValid)
            {
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
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", blogPost.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", blogPost.CategoryId);
            return View(blogPost);
        }

        // POST: BlogPosts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,CreatedAt,UpdatedAt,AuthorId,CategoryId,Image")] BlogPost blogPost)
        {
            if (id != blogPost.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(blogPost);
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
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", blogPost.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", blogPost.CategoryId);
            return View(blogPost);
        }

        // GET: BlogPosts/Delete/5
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

            return View(blogPost);
        }

        // POST: BlogPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BlogPosts == null)
            {
                return Problem("Entity set 'BlogDbContext.BlogPosts'  is null.");
            }
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost != null)
            {
                _context.BlogPosts.Remove(blogPost);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment([FromBody] Comment comment)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    comment.AuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    comment.CreatedAt = DateTime.Now;
                    _context.Add(comment);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true });
                }

                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to create comment." });
            }
        }

        public IActionResult PostsByAuthor(string id)
        {
            var posts =  _context.BlogPosts
                .Include(c => c.Author)
                .Where(p => p.AuthorId == id)
                .ToList();

            return View(posts);
        }
    }
}
