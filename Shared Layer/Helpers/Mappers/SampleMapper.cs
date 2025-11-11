using Core.Icp.Domain.Entities.Samples;
using Core.Icp.Domain.Enums;
using Shared.Icp.DTOs.Samples;

namespace Shared.Icp.Helpers.Mappers
{
    /// <summary>
    /// Mapper برای تبدیل Sample ↔ DTO
    /// </summary>
    public static class SampleMapper
    {
        /// <summary>
        /// تبدیل Entity به DTO
        /// </summary>
        public static SampleDto ToDto(this Sample sample)
        {
            if (sample == null) throw new ArgumentNullException(nameof(sample));

            return new SampleDto
            {
                Id = sample.Id,
                SampleId = sample.SampleId,
                SampleName = sample.SampleName,
                RunDate = sample.RunDate,
                Status = sample.Status.ToString(),
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
        /// تبدیل DTO به Entity (برای Create)
        /// </summary>
        public static Sample ToEntity(this CreateSampleDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new Sample
            {
                SampleId = dto.SampleId,
                SampleName = dto.SampleName,
                RunDate = dto.RunDate ?? DateTime.UtcNow,
                Status = SampleStatus.Pending,
                Weight = dto.Weight ?? 0,
                Volume = dto.Volume ?? 0,
                DilutionFactor = dto.DilutionFactor ?? 1,
                Notes = dto.Notes,
                ProjectId = dto.ProjectId
            };
        }

        /// <summary>
        /// به‌روزرسانی Entity از DTO
        /// </summary>
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

            sample.Notes = dto.Notes;

            if (!string.IsNullOrWhiteSpace(dto.Status) &&
                Enum.TryParse<SampleStatus>(dto.Status, out var status))
            {
                sample.Status = status;
            }
        }

        /// <summary>
        /// تبدیل لیست Entity به لیست DTO
        /// </summary>
        public static List<SampleDto> ToDtoList(this IEnumerable<Sample> samples)
        {
            return samples?.Select(s => s.ToDto()).ToList() ?? new List<SampleDto>();
        }
    }
}