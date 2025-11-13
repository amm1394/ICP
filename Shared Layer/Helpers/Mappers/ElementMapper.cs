using Core.Icp.Domain.Entities.Elements;
using Shared.Icp.DTOs.Elements;

namespace Shared.Icp.Helpers.Mappers
{
    /// <summary>
    /// Mapping helpers for converting between domain <see cref="Element"/>/<see cref="Isotope"/> entities
    /// and their corresponding DTOs (<see cref="ElementDto"/>, <see cref="CreateElementDto"/>, <see cref="UpdateElementDto"/>, <see cref="IsotopeDto"/>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This static, stateless mapper centralizes projection logic so controllers/services remain focused on orchestration.
    /// It does not perform validation or persistence; it only translates between domain entities and transport models.
    /// </para>
    /// <para>
    /// Conventions:
    /// - Null inputs throw <see cref="ArgumentNullException"/> to avoid silent failures.
    /// - No business rules are enforced here beyond simple conversions and safe defaults.
    /// - User-facing texts (if any) must remain Persian elsewhere; this code adds no user-visible messages.
    /// </para>
    /// </remarks>
    public static class ElementMapper
    {
        /// <summary>
        /// Projects a domain <see cref="Element"/> entity to a transport-friendly <see cref="ElementDto"/>.
        /// </summary>
        /// <param name="element">The domain element entity to project.</param>
        /// <returns>A populated <see cref="ElementDto"/> reflecting the entity state.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="element"/> is null.</exception>
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
        /// Creates a new domain <see cref="Element"/> entity from a <see cref="CreateElementDto"/>.
        /// </summary>
        /// <param name="dto">The create DTO containing initial element data.</param>
        /// <returns>A new <see cref="Element"/> entity ready for persistence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is null.</exception>
        /// <remarks>
        /// Defaulting rules:
        /// - <c>IsActive</c> defaults to <c>true</c> for newly created elements.
        /// - <c>DisplayOrder</c> defaults to <c>0</c> when not provided.
        /// Other values are copied as-is from the DTO.
        /// </remarks>
        public static Element ToEntity(this CreateElementDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new Element
            {
                Symbol = dto.Symbol,
                Name = dto.Name,
                AtomicNumber = dto.AtomicNumber,
                AtomicMass = dto.AtomicMass,
                IsActive = true,                // default to active
                DisplayOrder = dto.DisplayOrder ?? 0
            };
        }

        /// <summary>
        /// Applies changes from an <see cref="UpdateElementDto"/> to an existing <see cref="Element"/> entity.
        /// </summary>
        /// <param name="element">The target domain entity to modify.</param>
        /// <param name="dto">The update DTO containing fields to apply.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="element"/> or <paramref name="dto"/> is null.
        /// </exception>
        /// <remarks>
        /// Update rules:
        /// - <c>Name</c> is updated only when provided and non-whitespace.
        /// - <c>AtomicMass</c>, <c>IsActive</c>, and <c>DisplayOrder</c> are updated only when provided (non-null).
        /// - No validation is performed here; callers should validate prior to mapping.
        /// </remarks>
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
        /// Projects a sequence of <see cref="Element"/> entities to a list of <see cref="ElementDto"/> objects.
        /// </summary>
        /// <param name="elements">The source sequence of domain elements. When null, an empty list is returned.</param>
        /// <returns>A list of <see cref="ElementDto"/>s, preserving source order when available.</returns>
        public static List<ElementDto> ToDtoList(this IEnumerable<Element> elements)
        {
            return elements?.Select(e => e.ToDto()).ToList() ?? new List<ElementDto>();
        }

        /// <summary>
        /// Projects a domain <see cref="Isotope"/> entity to a transport-friendly <see cref="IsotopeDto"/>.
        /// </summary>
        /// <param name="isotope">The domain isotope entity to project.</param>
        /// <returns>A populated <see cref="IsotopeDto"/> reflecting the entity state.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="isotope"/> is null.</exception>
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