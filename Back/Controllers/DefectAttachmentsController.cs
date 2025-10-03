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
    public class DefectAttachmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DefectAttachmentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DefectAttachmentDTO>>> GetDefectAttachments()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            IQueryable<DefectAttachment> query = _context.DefectAttachments
                .Include(da => da.UploadedBy)
                .Include(da => da.Defect);

            // Наблюдатель видит только вложения своих дефектов
            if (currentUserRole == "Observer")
            {
                query = query.Where(da => da.Defect.ResponsibleId == currentUserId);
            }

            return await query
                .Select(da => new DefectAttachmentDTO
                {
                    AttachmentId = da.AttachmentId,
                    DefectId = da.DefectId,
                    FileName = da.FileName,
                    FilePath = da.FilePath,
                    FileSize = da.FileSize,
                    UploadDate = da.UploadDate,
                    UploadedById = da.UploadedById,
                    UploadedByFio = da.UploadedBy!.Fio
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DefectAttachmentDTO>> GetDefectAttachment(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var defectAttachment = await _context.DefectAttachments
                .Include(da => da.UploadedBy)
                .Include(da => da.Defect)
                .FirstOrDefaultAsync(da => da.AttachmentId == id);

            if (defectAttachment == null)
            {
                return NotFound();
            }

            // Наблюдатель может видеть только вложения своих дефектов
            if (currentUserRole == "Observer" && defectAttachment.Defect?.ResponsibleId != currentUserId)
            {
                return Forbid();
            }

            return new DefectAttachmentDTO
            {
                AttachmentId = defectAttachment.AttachmentId,
                DefectId = defectAttachment.DefectId,
                FileName = defectAttachment.FileName,
                FilePath = defectAttachment.FilePath,
                FileSize = defectAttachment.FileSize,
                UploadDate = defectAttachment.UploadDate,
                UploadedById = defectAttachment.UploadedById,
                UploadedByFio = defectAttachment.UploadedBy!.Fio
            };
        }

        [HttpPost]
        [Authorize(Policy = "CanManageDefects")]
        public async Task<ActionResult<DefectAttachmentDTO>> CreateDefectAttachment(DefectAttachmentCreateDTO defectAttachmentCreateDTO)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Проверяем доступ к дефекту
            var defect = await _context.Defects.FindAsync(defectAttachmentCreateDTO.DefectId);
            if (defect == null)
            {
                return NotFound("Defect not found");
            }

            // Наблюдатель может добавлять вложения только к своим дефектам
            if (currentUserRole == "Observer" && defect.ResponsibleId != currentUserId)
            {
                return Forbid();
            }

            var defectAttachment = new DefectAttachment
            {
                DefectId = defectAttachmentCreateDTO.DefectId,
                FileName = defectAttachmentCreateDTO.FileName,
                FilePath = defectAttachmentCreateDTO.FilePath,
                FileSize = defectAttachmentCreateDTO.FileSize,
                UploadDate = DateTime.UtcNow,
                UploadedById = currentUserId // Текущий пользователь как загрузивший
            };

            _context.DefectAttachments.Add(defectAttachment);
            await _context.SaveChangesAsync();

            // Загружаем данные пользователя для ответа
            await _context.Entry(defectAttachment).Reference(da => da.UploadedBy).LoadAsync();

            return CreatedAtAction(nameof(GetDefectAttachment), new { id = defectAttachment.AttachmentId }, new DefectAttachmentDTO
            {
                AttachmentId = defectAttachment.AttachmentId,
                DefectId = defectAttachment.DefectId,
                FileName = defectAttachment.FileName,
                FilePath = defectAttachment.FilePath,
                FileSize = defectAttachment.FileSize,
                UploadDate = defectAttachment.UploadDate,
                UploadedById = defectAttachment.UploadedById,
                UploadedByFio = defectAttachment.UploadedBy!.Fio
            });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "CanManageDefects")]
        public async Task<IActionResult> UpdateDefectAttachment(int id, DefectAttachmentUpdateDTO defectAttachmentUpdateDTO)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var defectAttachment = await _context.DefectAttachments
                .Include(da => da.Defect)
                .FirstOrDefaultAsync(da => da.AttachmentId == id);

            if (defectAttachment == null)
            {
                return NotFound();
            }

            // Проверяем права на редактирование
            if (currentUserRole == "Observer" && defectAttachment.Defect?.ResponsibleId != currentUserId)
            {
                return Forbid();
            }

            // Инженер может редактировать только свои вложения
            if (currentUserRole == "Engineer" && defectAttachment.UploadedById != currentUserId)
            {
                return Forbid();
            }

            defectAttachment.FileName = defectAttachmentUpdateDTO.FileName;
            defectAttachment.FilePath = defectAttachmentUpdateDTO.FilePath;
            defectAttachment.FileSize = defectAttachmentUpdateDTO.FileSize;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DefectAttachmentExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "CanManageDefects")]
        public async Task<IActionResult> DeleteDefectAttachment(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var defectAttachment = await _context.DefectAttachments
                .Include(da => da.Defect)
                .FirstOrDefaultAsync(da => da.AttachmentId == id);

            if (defectAttachment == null)
            {
                return NotFound();
            }

            // Проверяем права на удаление
            if (currentUserRole == "Observer" && defectAttachment.Defect?.ResponsibleId != currentUserId)
            {
                return Forbid();
            }

            // Инженер может удалять только свои вложения
            if (currentUserRole == "Engineer" && defectAttachment.UploadedById != currentUserId)
            {
                return Forbid();
            }

            _context.DefectAttachments.Remove(defectAttachment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("defect/{defectId}")]
        public async Task<ActionResult<IEnumerable<DefectAttachmentDTO>>> GetDefectAttachmentsByDefect(int defectId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Проверяем доступ к дефекту
            var defect = await _context.Defects.FindAsync(defectId);
            if (defect == null)
            {
                return NotFound("Defect not found");
            }

            // Наблюдатель может видеть только вложения своих дефектов
            if (currentUserRole == "Observer" && defect.ResponsibleId != currentUserId)
            {
                return Forbid();
            }

            return await _context.DefectAttachments
                .Include(da => da.UploadedBy)
                .Where(da => da.DefectId == defectId)
                .Select(da => new DefectAttachmentDTO
                {
                    AttachmentId = da.AttachmentId,
                    DefectId = da.DefectId,
                    FileName = da.FileName,
                    FilePath = da.FilePath,
                    FileSize = da.FileSize,
                    UploadDate = da.UploadDate,
                    UploadedById = da.UploadedById,
                    UploadedByFio = da.UploadedBy!.Fio
                })
                .ToListAsync();
        }

        private bool DefectAttachmentExists(int id)
        {
            return _context.DefectAttachments.Any(e => e.AttachmentId == id);
        }
    }
}