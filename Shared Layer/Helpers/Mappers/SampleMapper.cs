using Core.Icp.Domain.Entities.Samples;
using Core.Icp.Domain.Enums;
using Shared.Icp.DTOs.Samples;

namespace Shared.Icp.Helpers.Mappers
{
    /// <summary>
    /// Mapping helpers for converting between domain <see cref="Sample"/> entities and DTOs
    /// (<see cref="SampleDto"/>, <see cref="CreateSampleDto"/>, <see cref="UpdateSampleDto"/>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This static mapper centralizes translation logic to keep controllers and services lean. It provides
    /// extensions to: project domain entities into transport-friendly DTOs, materialize entities from create
    /// DTOs, and apply partial updates from update DTOs.
    /// </para>
    /// <para>
    /// Notes:
    /// - All methods are null-safe at the entry point and will throw <see cref="ArgumentNullException"/>
    ///   if a required input is null.
    /// - No business rules are enforced here beyond simple conversions and defaults; validation should be
    ///   performed earlier in the pipeline.
    /// </para>
    /// </remarks>
    public static class SampleMapper
    {
        /// <summary>
        /// Projects a domain <see cref="Sample"/> entity to a transport-friendly <see cref="SampleDto"/>.
        /// </summary>
        /// <param name="sample">The domain sample entity to project.</param>
        /// <returns>A populated <see cref="SampleDto"/> reflecting the entity state.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sample"/> is null.</exception>
        public static SampleDto ToDto(this Sample sample)
        {
            if (sample == null) throw new ArgumentNullException(nameof(sample));

            return new SampleDto
            {
                Id = sample.Id,
                SampleId = sample.SampleId,
                SampleName = sample.SampleName,
                RunDate = sample.RunDate,
                Status = sample.Status.ToString(), // expose enum as string for DTO consumers
                Weight = sample.Weight,
                Volume = sample.Volume,
                DilutionFactor = sample.DilutionFactor,
                Notes = sample.Notes,
                ProjectId = sample.ProjectId,
                CreatedAt = sample.CreatedAt,
                UpdatedAt = sample.UpdatedAt
            };
        }

        /// <summary>
        /// Creates a new domain <see cref="Sample"/> entity from a <see cref="CreateSampleDto"/>.
        /// </summary>
        /// <param name="dto">The create DTO containing initial sample data.</param>
        /// <returns>A new <see cref="Sample"/> entity ready for persistence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is null.</exception>
        /// <remarks>
        /// Defaulting rules:
        /// - <c>RunDate</c> defaults to <see cref="DateTime.UtcNow"/> when not provided.
        /// - <c>Status</c> is initialized to <see cref="SampleStatus.Pending"/>.
        /// - <c>Weight</c> and <c>Volume</c> default to 0 when null.
        /// - <c>DilutionFactor</c> defaults to 1 when null.
        /// </remarks>
        public static Sample ToEntity(this CreateSampleDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new Sample
            {
                SampleId = dto.SampleId,
                SampleName = dto.SampleName,
                RunDate = dto.RunDate ?? DateTime.UtcNow, // default to now if not supplied
                Status = SampleStatus.Pending,
                Weight = dto.Weight ?? 0,
                Volume = dto.Volume ?? 0,
                DilutionFactor = dto.DilutionFactor ?? 1,
                Notes = dto.Notes,
                ProjectId = dto.ProjectId
            };
        }

        /// <summary>
        /// Applies changes from an <see cref="UpdateSampleDto"/> to an existing <see cref="Sample"/> entity.
        /// </summary>
        /// <param name="sample">The target domain entity to modify.</param>
        /// <param name="dto">The update DTO containing fields to apply.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="sample"/> or <paramref name="dto"/> is null.
        /// </exception>
        /// <remarks>
        /// Update rules:
        /// - <c>SampleName</c> is required by upstream validation and is applied directly.
        /// - <c>RunDate</c>, <c>Weight</c>, <c>Volume</c>, and <c>DilutionFactor</c> are updated only when provided (non-null).
        /// - <c>Notes</c> is set from the DTO (can be null to clear notes).
        /// - <c>Status</c> is updated only when a non-empty string is supplied and it parses to a valid <see cref="SampleStatus"/>; otherwise ignored.
        /// </remarks>
        public static void UpdateFromDto(this Sample sample, UpdateSampleDto dto)
        {
            if (sample == null) throw new ArgumentNullException(nameof(sample));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            sample.SampleName = dto.SampleName;

            if (dto.RunDate.HasValue)
                sample.RunDate = dto.RunDate.Value;

            if (dto.Weight.HasValue)
                sample.Weight = dto.Weight.Value;

            if (dto.Volume.HasValue)
                sample.Volume = dto.Volume.Value;

            if (dto.DilutionFactor.HasValue)
                sample.DilutionFactor = dto.DilutionFactor.Value;

            sample.Notes = dto.Notes; // allow clearing by null

            // Update status only if provided and parsable; invalid values are ignored
            if (!string.IsNullOrWhiteSpace(dto.Status) &&
                Enum.TryParse<SampleStatus>(dto.Status, out var status))
            {
                sample.Status = status;
            }
        }

        /// <summary>
        /// Projects a sequence of <see cref="Sample"/> entities to a list of <see cref="SampleDto"/> objects.
        /// </summary>
        /// <param name="samples">The source sequence of domain samples. When null, an empty list is returned.</param>
        /// <returns>A list of <see cref="SampleDto"/>s, preserving source order when available.</returns>
        public static List<SampleDto> ToDtoList(this IEnumerable<Sample> samples)
        {
            return samples?.Select(s => s.ToDto()).ToList() ?? new List<SampleDto>();
        }
    }
}