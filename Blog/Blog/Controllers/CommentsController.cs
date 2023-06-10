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
using Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore;

namespace Blog.Controllers
{
    public class CommentsController : Controller
    {
        private readonly BlogDbContext _context;
        private readonly UserManager<BlogUser> _userManager;
        public CommentsController(BlogDbContext context, UserManager<BlogUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Comments/Create        
        public IActionResult Create()
        {
            ViewData["AuthorId"] = new SelectList(_context.Set<BlogUser>(), "Id", "Id");
            ViewData["BlogPostId"] = new SelectList(_context.BlogPosts, "Id", "Id");
            return View();
        }

        // POST: Comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]        
        public async Task<IActionResult> Create(string text, int blogPostId)
        {            
            try
            {
                Comment comment = new Comment();
                if (ModelState.IsValid)
                {
                    comment.Text = text;
                    comment.BlogPostId = blogPostId;
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

        // GET: Comments/Edit/5
        [Authorize(Roles ="Admin, Author")]
        public async Task<IActionResult> Edit(int? id)
        {            
            if (id == null || _context.Comments == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .Include(c => c.BlogPost)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null)
            {
                return NotFound();
            }
            ViewData["AuthorId"] = new SelectList(_context.Set<BlogUser>(), "Id", "Id", comment.AuthorId);
            ViewData["BlogPostId"] = new SelectList(_context.BlogPosts, "Id", "Id", comment.BlogPostId);
            if (User.IsInRole("Admin") || _userManager.GetUserId(User) == comment.AuthorId)
            {
                return View(comment);
            }
            return NotFound();
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Text,CreatedAt,BlogPostId,AuthorId")] Comment comment)
        {
            if (id != comment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var originalComment = _context.Comments.Find(comment.Id);
                if (User.IsInRole("Admin") || _userManager.GetUserId(User) == originalComment.AuthorId)
                {
                    try
                    {
                        originalComment.Text = comment.Text;
                        _context.Update(originalComment);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!CommentExists(comment.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return Redirect("/BlogPosts/Details/"+comment.BlogPostId);
                }
            }
            ViewData["AuthorId"] = new SelectList(_context.Set<BlogUser>(), "Id", "Id", comment.AuthorId);
            ViewData["BlogPostId"] = new SelectList(_context.BlogPosts, "Id", "Id", comment.BlogPostId);                        
            
            return NotFound();            
        }

        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Comments == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .Include(c => c.Author)
                .Include(c => c.BlogPost)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }
            if (User.IsInRole("Admin") || _userManager.GetUserId(User) == comment.AuthorId)
            {
                return View(comment);
            }
            return NotFound();
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Comments == null)
            {
                return Problem("Entity set 'BlogDbContext.Comments'  is null.");
            }
            var comment = await _context.Comments.FindAsync(id);
            var blogPostId = comment.BlogPostId;
            if (User.IsInRole("Admin") || _userManager.GetUserId(User) == comment.AuthorId)
            {
                if (comment != null)
                {
                    _context.Comments.Remove(comment);
                }

                await _context.SaveChangesAsync();
                return Redirect("/BlogPosts/Details/" + blogPostId);
            }
            return NotFound();
        }

        private bool CommentExists(int id)
        {
          return (_context.Comments?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpGet]
        public IActionResult GetComments(int blogPostId)
        {
            try
            {
                var comments = _context.Comments
                    .Where(c => c.BlogPostId == blogPostId)
                    .Include(c => c.Author)
                    .Select(c => new
                    {
                        AuthorId = c.AuthorId,
                        AuthorName = c.Author != null ? $"{c.Author.FirstName} {c.Author.LastName}" : "Desconocido",
                        c.CreatedAt,
                        Email = c.Author != null ? c.Author.Email : "",
                        c.Text,
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
    }
}
