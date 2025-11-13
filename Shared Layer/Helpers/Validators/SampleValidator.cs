using Shared.Icp.DTOs.Samples;
using Shared.Icp.Constants;

namespace Shared.Icp.Helpers.Validators
{
    /// <summary>
    /// Provides validation helpers for <see cref="CreateSampleDto"/> and <see cref="UpdateSampleDto"/>,
    /// along with simple domain checks for editing/deletion and numeric ranges.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All validation methods are non-throwing and return either a boolean result or a list of error messages.
    /// The error messages are intended for end-user presentation and therefore remain in Persian.
    /// </para>
    /// <para>
    /// This type contains only stateless helper methods and does not mutate input DTOs.
    /// </para>
    /// </remarks>
    public static class SampleValidator
    {
        /// <summary>
        /// Validates a <see cref="CreateSampleDto"/> instance and returns a list of validation errors, if any.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Validation rules include, but are not limited to:
        /// - <c>SampleId</c> is required (non-empty/whitespace).
        /// - <c>SampleName</c> is required (non-empty/whitespace).
        /// - <c>ProjectId</c> must not be empty (<see cref="Guid.Empty"/>).
        /// - If provided, <c>Weight</c> must be greater than 0.
        /// - If provided, <c>Volume</c> must be greater than 0.
        /// - If provided, <c>DilutionFactor</c> must be greater than 0.
        /// </para>
        /// <para>
        /// Error messages are returned in Persian and should be shown to the end user as-is.
        /// The method does not throw exceptions and does not alter the input DTO.
        /// </para>
        /// </remarks>
        /// <param name="dto">The <see cref="CreateSampleDto"/> to validate. When null, a single generic error is returned.</param>
        /// <returns>
        /// A list of Persian validation error messages. An empty list indicates the input passed all checks.
        /// </returns>
        public static List<string> Validate(CreateSampleDto dto)
        {
            var errors = new List<string>();

            if (dto == null)
            {
                errors.Add(ErrorMessages.Sample.InvalidData);
                return errors;
            }

            // Required: SampleId
            if (string.IsNullOrWhiteSpace(dto.SampleId))
            {
                errors.Add(ErrorMessages.Sample.SampleIdRequired);
            }

            // Required: SampleName
            if (string.IsNullOrWhiteSpace(dto.SampleName))
            {
                errors.Add(ErrorMessages.Sample.SampleNameRequired);
            }

            // Required: ProjectId must not be empty
            if (dto.ProjectId == Guid.Empty)
            {
                errors.Add("شناسه پروژه الزامی است");
            }

            // Optional numeric: Weight > 0 when specified
            if (dto.Weight.HasValue && dto.Weight.Value <= 0)
            {
                errors.Add(ErrorMessages.Sample.WeightInvalid);
            }

            // Optional numeric: Volume > 0 when specified
            if (dto.Volume.HasValue && dto.Volume.Value <= 0)
            {
                errors.Add(ErrorMessages.Sample.VolumeInvalid);
            }

            // Optional numeric: DilutionFactor > 0 when specified
            if (dto.DilutionFactor.HasValue && dto.DilutionFactor.Value <= 0)
            {
                errors.Add(ErrorMessages.Sample.DilutionFactorInvalid);
            }

            return errors;
        }

        /// <summary>
        /// Validates an <see cref="UpdateSampleDto"/> instance and returns a list of validation errors, if any.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Validation rules include, but are not limited to:
        /// - <c>SampleName</c> is required (non-empty/whitespace) even for updates.
        /// - If provided, <c>Weight</c> must be greater than 0.
        /// - If provided, <c>Volume</c> must be greater than 0.
        /// - If provided, <c>DilutionFactor</c> must be greater than 0.
        /// </para>
        /// <para>
        /// Error messages are returned in Persian for end-user display. The method is non-throwing and does not
        /// mutate the input DTO.
        /// </para>
        /// </remarks>
        /// <param name="dto">The <see cref="UpdateSampleDto"/> to validate. When null, a single generic error is returned.</param>
        /// <returns>
        /// A list of Persian validation error messages. An empty list indicates the input passed all checks.
        /// </returns>
        public static List<string> Validate(UpdateSampleDto dto)
        {
            var errors = new List<string>();

            if (dto == null)
            {
                errors.Add(ErrorMessages.Sample.InvalidData);
                return errors;
            }

            // Required: SampleName for updates
            if (string.IsNullOrWhiteSpace(dto.SampleName))
            {
                errors.Add(ErrorMessages.Sample.SampleNameRequired);
            }

            // Optional numeric: Weight > 0 when specified
            if (dto.Weight.HasValue && dto.Weight.Value <= 0)
            {
                errors.Add(ErrorMessages.Sample.WeightInvalid);
            }

            // Optional numeric: Volume > 0 when specified
            if (dto.Volume.HasValue && dto.Volume.Value <= 0)
            {
                errors.Add(ErrorMessages.Sample.VolumeInvalid);
            }

            // Optional numeric: DilutionFactor > 0 when specified
            if (dto.DilutionFactor.HasValue && dto.DilutionFactor.Value <= 0)
            {
                errors.Add(ErrorMessages.Sample.DilutionFactorInvalid);
            }

            return errors;
        }

        /// <summary>
        /// Determines whether a sample can be deleted based on its status.
        /// </summary>
        /// <remarks>
        /// Domain rule: only samples with status "Pending" are deletable. The check is null-safe.
        /// </remarks>
        /// <param name="sample">The sample to evaluate. When null, returns <c>false</c>.</param>
        /// <returns>
        /// <c>true</c> if the sample can be deleted; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanDelete(SampleDto sample)
        {
            if (sample == null) return false;

            // Deletable only when still Pending
            return sample.Status == "Pending";
        }

        /// <summary>
        /// Determines whether a sample can be edited based on its status.
        /// </summary>
        /// <remarks>
        /// Domain rule: samples with status "Approved" cannot be edited. The check is null-safe.
        /// </remarks>
        /// <param name="sample">The sample to evaluate. When null, returns <c>false</c>.</param>
        /// <returns>
        /// <c>true</c> if the sample can be edited; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanEdit(SampleDto sample)
        {
            if (sample == null) return false;

            // Non-editable only when Approved
            return sample.Status != "Approved";
        }

        /// <summary>
        /// Checks whether the provided weight is within the allowed range.
        /// </summary>
        /// <remarks>
        /// Default bounds are 0.001 ≤ weight ≤ 100 (grams). Values are domain-specific and may vary.
        /// </remarks>
        /// <param name="weight">Sample weight (grams) to check.</param>
        /// <param name="min">Inclusive minimum allowed value. Default is 0.001.</param>
        /// <param name="max">Inclusive maximum allowed value. Default is 100.</param>
        /// <returns>
        /// <c>true</c> if the value lies within [min, max]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsWeightInRange(decimal weight, decimal min = 0.001m, decimal max = 100m)
        {
            return weight >= min && weight <= max;
        }

        /// <summary>
        /// Checks whether the provided volume is within the allowed range.
        /// </summary>
        /// <remarks>
        /// Default bounds are 0.1 ≤ volume ≤ 1000 (milliliters). Values are domain-specific and may vary.
        /// </remarks>
        /// <param name="volume">Sample volume (mL) to check.</param>
        /// <param name="min">Inclusive minimum allowed value. Default is 0.1.</param>
        /// <param name="max">Inclusive maximum allowed value. Default is 1000.</param>
        /// <returns>
        /// <c>true</c> if the value lies within [min, max]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsVolumeInRange(decimal volume, decimal min = 0.1m, decimal max = 1000m)
        {
            return volume >= min && volume <= max;
        }

        /// <summary>
        /// Checks whether the provided dilution factor is within the allowed range.
        /// </summary>
        /// <remarks>
        /// Default bounds are 1 ≤ df ≤ 10000 (unitless). Values are domain-specific and may vary.
        /// </remarks>
        /// <param name="df">Dilution factor (unitless) to check.</param>
        /// <param name="min">Inclusive minimum allowed value. Default is 1.</param>
        /// <param name="max">Inclusive maximum allowed value. Default is 10000.</param>
        /// <returns>
        /// <c>true</c> if the value lies within [min, max]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDilutionFactorInRange(decimal df, decimal min = 1m, decimal max = 10000m)
        {
            return df >= min && df <= max;
        }
    }
}