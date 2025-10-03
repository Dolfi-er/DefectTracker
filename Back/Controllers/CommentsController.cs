using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Back.Data;
using Back.Models.Entities;
using Back.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "CanViewAll")]
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
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            IQueryable<Comment> query = _context.Comments
                .Include(c => c.User)
                .Where(c => !c.IsDeleted);

            // Наблюдатель видит только комментарии к своим дефектам
            if (currentUserRole == "Observer")
            {
                query = query.Where(c => c.Defect.ResponsibleId == currentUserId);
            }

            return await query
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
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var comment = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Defect)
                .FirstOrDefaultAsync(c => c.CommentId == id && !c.IsDeleted);

            if (comment == null)
            {
                return NotFound();
            }

            // Наблюдатель может видеть только комментарии к своим дефектам
            if (currentUserRole == "Observer" && comment.Defect?.ResponsibleId != currentUserId)
            {
                return Forbid();
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
        [Authorize(Policy = "CanManageDefects")]
        public async Task<ActionResult<CommentDTO>> CreateComment(CommentCreateDTO commentCreateDTO)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Проверяем доступ к дефекту
            var defect = await _context.Defects.FindAsync(commentCreateDTO.DefectId);
            if (defect == null)
            {
                return NotFound("Defect not found");
            }

            // Наблюдатель может комментировать только свои дефекты
            if (currentUserRole == "Observer" && defect.ResponsibleId != currentUserId)
            {
                return Forbid();
            }

            var comment = new Comment
            {
                DefectId = commentCreateDTO.DefectId,
                UserId = currentUserId, // Автор - текущий пользователь
                CommentText = commentCreateDTO.CommentText,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Загружаем данные пользователя для ответа
            await _context.Entry(comment).Reference(c => c.User).LoadAsync();

            return CreatedAtAction(nameof(GetComment), new { id = comment.CommentId }, new CommentDTO
            {
                CommentId = comment.CommentId,
                DefectId = comment.DefectId,
                UserId = comment.UserId,
                CommentText = comment.CommentText,
                CreatedDate = comment.CreatedDate,
                IsDeleted = comment.IsDeleted,
                UserFio = comment.User!.Fio
            });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "CanManageDefects")]
        public async Task<IActionResult> UpdateComment(int id, CommentUpdateDTO commentUpdateDTO)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var comment = await _context.Comments
                .Include(c => c.Defect)
                .FirstOrDefaultAsync(c => c.CommentId == id);

            if (comment == null)
            {
                return NotFound();
            }

            // Проверяем права на редактирование
            if (currentUserRole == "Observer" && comment.Defect?.ResponsibleId != currentUserId)
            {
                return Forbid();
            }

            // Инженер может редактировать только свои комментарии
            if (currentUserRole == "Engineer" && comment.UserId != currentUserId)
            {
                return Forbid();
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
        [Authorize(Policy = "CanManageDefects")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var comment = await _context.Comments
                .Include(c => c.Defect)
                .FirstOrDefaultAsync(c => c.CommentId == id);

            if (comment == null)
            {
                return NotFound();
            }

            // Проверяем права на удаление
            if (currentUserRole == "Observer" && comment.Defect?.ResponsibleId != currentUserId)
            {
                return Forbid();
            }

            // Инженер может удалять только свои комментарии
            if (currentUserRole == "Engineer" && comment.UserId != currentUserId)
            {
                return Forbid();
            }

            comment.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("defect/{defectId}")]
        public async Task<ActionResult<IEnumerable<CommentDTO>>> GetCommentsByDefect(int defectId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Проверяем доступ к дефекту
            var defect = await _context.Defects.FindAsync(defectId);
            if (defect == null)
            {
                return NotFound("Defect not found");
            }

            // Наблюдатель может видеть только комментарии к своим дефектам
            if (currentUserRole == "Observer" && defect.ResponsibleId != currentUserId)
            {
                return Forbid();
            }

            return await _context.Comments
                .Include(c => c.User)
                .Where(c => c.DefectId == defectId && !c.IsDeleted)
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

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.CommentId == id);
        }
    }
}