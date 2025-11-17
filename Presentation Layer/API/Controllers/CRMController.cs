using Core.Icp.Application.Interfaces;
using Core.Icp.Application.Models.CRM;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers // اگر namespace پروژه‌ات متفاوت است، همین را با namespace فعلی Controllerها یکی کن
{
    [ApiController]
    [Route("api/[controller]")]
    public class CRMController : ControllerBase
    {
        private readonly ICRMService _crmService;

        public CRMController(ICRMService crmService)
        {
            _crmService = crmService;
        }

        /// <summary>
        /// دریافت لیست CRM ها
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyCollection<CRMDto>>> GetAll(
            [FromQuery] bool onlyActive = true,
            CancellationToken cancellationToken = default)
        {
            var result = await _crmService.GetCrmsAsync(onlyActive, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// دریافت یک CRM با Id
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CRMDto>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var crm = await _crmService.GetByIdAsync(id, cancellationToken);
            if (crm == null)
                return NotFound();

            return Ok(crm);
        }

        /// <summary>
        /// جستجوی CRM ها بر اساس کد یا ماتریس
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IReadOnlyCollection<CRMDto>>> Search(
            [FromQuery] string? crmId = null,
            [FromQuery] string? matrix = null,
            [FromQuery] bool onlyActive = true,
            CancellationToken cancellationToken = default)
        {
            var result = await _crmService.SearchAsync(crmId, matrix, onlyActive, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// ایجاد CRM جدید
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CRMDto>> Create(
            [FromBody] CreateCRMDto dto,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _crmService.CreateAsync(dto, cancellationToken);

            // برگردوندن 201 همراه با مسیر دسترسی به رکورد جدید
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// ویرایش CRM موجود
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<CRMDto>> Update(
            Guid id,
            [FromBody] UpdateCRMDto dto,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.Id == Guid.Empty)
                dto.Id = id;

            if (dto.Id != id)
                return BadRequest("Id in route and body do not match.");

            var updated = await _crmService.UpdateAsync(dto, cancellationToken);
            return Ok(updated);
        }

        /// <summary>
        /// حذف CRM (Soft Delete از طریق Service/Repository)
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            await _crmService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
