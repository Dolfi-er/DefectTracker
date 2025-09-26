using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Back.Data;
using Back.Models.Entities;
using Back.DTOs;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CommentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentDTO>>> GetComments()
        {
            return await _context.Comments
                .Include(c => c.User)
                .Where(c => !c.IsDeleted)
                .Select(c => new CommentDTO
                {
                    CommentId = c.CommentId,
                    DefectId = c.DefectId,
                    UserId = c.UserId,
                    CommentText = c.CommentText,
                    CreatedDate = c.CreatedDate,
                    IsDeleted = c.IsDeleted,
                    UserFio = c.User!.Fio
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CommentDTO>> GetComment(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CommentId == id && !c.IsDeleted);

            if (comment == null)
            {
                return NotFound();
            }

            return new CommentDTO
            {
                CommentId = comment.CommentId,
                DefectId = comment.DefectId,
                UserId = comment.UserId,
                CommentText = comment.CommentText,
                CreatedDate = comment.CreatedDate,
                IsDeleted = comment.IsDeleted,
                UserFio = comment.User!.Fio
            };
        }

        [HttpPost]
        public async Task<ActionResult<CommentDTO>> CreateComment(CommentCreateDTO commentCreateDTO)
        {
            var comment = new Comment
            {
                DefectId = commentCreateDTO.DefectId,
                UserId = commentCreateDTO.UserId,
                CommentText = commentCreateDTO.CommentText,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetComment), new { id = comment.CommentId }, new CommentDTO
            {
                CommentId = comment.CommentId,
                DefectId = comment.DefectId,
                UserId = comment.UserId,
                CommentText = comment.CommentText,
                CreatedDate = comment.CreatedDate,
                IsDeleted = comment.IsDeleted
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, CommentUpdateDTO commentUpdateDTO)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            comment.CommentText = commentUpdateDTO.CommentText;
            comment.IsDeleted = commentUpdateDTO.IsDeleted;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            comment.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.CommentId == id);
        }
    }
}