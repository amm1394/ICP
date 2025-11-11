using Core.Icp.Domain.Entities.Elements;
using Shared.Icp.DTOs.Elements;

namespace Shared.Icp.Helpers.Mappers
{
    /// <summary>
    /// Mapper برای تبدیل Element ↔ DTO
    /// </summary>
    public static class ElementMapper
    {
        /// <summary>
        /// تبدیل Entity به DTO
        /// </summary>
        public static ElementDto ToDto(this Element element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));

            return new ElementDto
            {
                Id = element.Id,
                Symbol = element.Symbol,
                Name = element.Name,
                AtomicNumber = element.AtomicNumber,
                AtomicMass = element.AtomicMass,
                IsActive = element.IsActive,
                DisplayOrder = element.DisplayOrder,
                CreatedAt = element.CreatedAt,
                UpdatedAt = element.UpdatedAt
            };
        }

        /// <summary>
        /// تبدیل DTO به Entity (برای Create)
        /// </summary>
        public static Element ToEntity(this CreateElementDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new Element
            {
                Symbol = dto.Symbol,
                Name = dto.Name,
                AtomicNumber = dto.AtomicNumber,
                AtomicMass = dto.AtomicMass,
                IsActive = true,
                DisplayOrder = dto.DisplayOrder ?? 0
            };
        }

        /// <summary>
        /// به‌روزرسانی Entity از DTO
        /// </summary>
        public static void UpdateFromDto(this Element element, UpdateElementDto dto)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (!string.IsNullOrWhiteSpace(dto.Name))
                element.Name = dto.Name;

            if (dto.AtomicMass.HasValue)
                element.AtomicMass = dto.AtomicMass.Value;

            if (dto.IsActive.HasValue)
                element.IsActive = dto.IsActive.Value;

            if (dto.DisplayOrder.HasValue)
                element.DisplayOrder = dto.DisplayOrder.Value;
        }

        /// <summary>
        /// تبدیل لیست Entity به لیست DTO
        /// </summary>
        public static List<ElementDto> ToDtoList(this IEnumerable<Element> elements)
        {
            return elements?.Select(e => e.ToDto()).ToList() ?? new List<ElementDto>();
        }

        /// <summary>
        /// تبدیل Isotope به DTO
        /// </summary>
        public static IsotopeDto ToDto(this Isotope isotope)
        {
            if (isotope == null) throw new ArgumentNullException(nameof(isotope));

            return new IsotopeDto
            {
                Id = isotope.Id,
                ElementId = isotope.ElementId,
                MassNumber = isotope.MassNumber,
                Abundance = isotope.Abundance,
                IsStable = isotope.IsStable,
                CreatedAt = isotope.CreatedAt,
                UpdatedAt = isotope.UpdatedAt
            };
        }
    }
}