using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Back.Data;
using Back.Models.Entities;
using Back.DTOs;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            return await _context.DefectAttachments
                .Include(da => da.UploadedBy)
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
            var defectAttachment = await _context.DefectAttachments
                .Include(da => da.UploadedBy)
                .FirstOrDefaultAsync(da => da.AttachmentId == id);

            if (defectAttachment == null)
            {
                return NotFound();
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
        public async Task<ActionResult<DefectAttachmentDTO>> CreateDefectAttachment(DefectAttachmentCreateDTO defectAttachmentCreateDTO)
        {
            var defectAttachment = new DefectAttachment
            {
                DefectId = defectAttachmentCreateDTO.DefectId,
                FileName = defectAttachmentCreateDTO.FileName,
                FilePath = defectAttachmentCreateDTO.FilePath,
                FileSize = defectAttachmentCreateDTO.FileSize,
                UploadDate = DateTime.UtcNow,
                UploadedById = defectAttachmentCreateDTO.UploadedById
            };

            _context.DefectAttachments.Add(defectAttachment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDefectAttachment), new { id = defectAttachment.AttachmentId }, new DefectAttachmentDTO
            {
                AttachmentId = defectAttachment.AttachmentId,
                DefectId = defectAttachment.DefectId,
                FileName = defectAttachment.FileName,
                FilePath = defectAttachment.FilePath,
                FileSize = defectAttachment.FileSize,
                UploadDate = defectAttachment.UploadDate,
                UploadedById = defectAttachment.UploadedById
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDefectAttachment(int id, DefectAttachmentUpdateDTO defectAttachmentUpdateDTO)
        {
            var defectAttachment = await _context.DefectAttachments.FindAsync(id);
            if (defectAttachment == null)
            {
                return NotFound();
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
        public async Task<IActionResult> DeleteDefectAttachment(int id)
        {
            var defectAttachment = await _context.DefectAttachments.FindAsync(id);
            if (defectAttachment == null)
            {
                return NotFound();
            }

            _context.DefectAttachments.Remove(defectAttachment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DefectAttachmentExists(int id)
        {
            return _context.DefectAttachments.Any(e => e.AttachmentId == id);
        }
    }
}