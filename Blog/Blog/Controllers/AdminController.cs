using Blog.Data;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Controllers
{
    [Authorize(Roles="Admin")]
    public class AdminController : Controller
    {
        private readonly BlogDbContext _context;
        private readonly UserManager<BlogUser> _userManager;

        public AdminController(BlogDbContext context, UserManager<BlogUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {            
            return View();            
        }

        [HttpGet]
        public IActionResult GetBlogPosts()
        {
            try
            {
                var posts = _context.BlogPosts                    
                    .Include(c => c.Author)
                    .Include(c => c.Category)
                    .Select(c => new
                    {
                        c.Title,
                        AuthorName = c.Author != null ? $"{c.Author.FirstName} {c.Author.LastName}" : "Desconocido",
                        c.CreatedAt,
                        c.Id,
                        Category = c.Category.Name 
                    })
                    .ToList();

                return Json(posts);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to fetch blog posts." });
            }
        }

        [HttpGet]
        public IActionResult GetComments()
        {
            try
            {
                var comments = _context.Comments
                    .Include(c => c.Author)
                    .Select(c => new
                    {
                        c.Text,
                        AuthorName = c.Author != null ? $"{c.Author.FirstName} {c.Author.LastName}" : "Desconocido",
                        Email = c.Author != null ? c.Author.Email : "",
                        c.CreatedAt,
                        c.Id                        
                    })
                    .ToList();

                return Json(comments);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to fetch comments." });
            }
        }

        [HttpGet]
        public IActionResult GetCategories()
        {
            try
            {
                var comments = _context.Categories                    
                    .Select(c => new
                    {
                        c.Name,                        
                        c.Id,
                    })
                    .ToList();

                return Json(comments);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to fetch categories." });
            }
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            try
            {
                var filteredUsers = _context.UserRoles
                    .Where(c => c.RoleId == "1")
                    .Select(c => c.UserId)
                    .ToList();

                var users = _context.Users                    
                    .Where(c => !filteredUsers.Contains(c.Id))
                    .Select(c => new
                    {
                        Name= $"{c.FirstName} {c.LastName}",
                        c.Email,
                        c.Id                        
                    })
                    .ToList();

                return Json(users);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to fetch users." });
            }
        }

        public async Task<IActionResult> DeleteUser(string? id)
        {
            if (id == null || _context.BlogPosts == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(b => b.Posts)
                .Include(b => b.Comments)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("DeleteUserConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(string id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'BlogDbContext.Users'  is null.");
            }
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
