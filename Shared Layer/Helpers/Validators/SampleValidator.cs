using Shared.Icp.DTOs.Samples;
using Shared.Icp.Constants;

namespace Shared.Icp.Helpers.Validators
{
    /// <summary>
    /// اعتبارسنجی Sample
    /// </summary>
    public static class SampleValidator
    {
        /// <summary>
        /// اعتبارسنجی CreateSampleDto
        /// </summary>
        public static List<string> Validate(CreateSampleDto dto)
        {
            var errors = new List<string>();

            if (dto == null)
            {
                errors.Add(ErrorMessages.Sample.InvalidData);
                return errors;
            }

            // بررسی SampleId
            if (string.IsNullOrWhiteSpace(dto.SampleId))
            {
                errors.Add(ErrorMessages.Sample.SampleIdRequired);
            }

            // بررسی SampleName
            if (string.IsNullOrWhiteSpace(dto.SampleName))
            {
                errors.Add(ErrorMessages.Sample.SampleNameRequired);
            }

            // بررسی ProjectId
            if (dto.ProjectId == Guid.Empty)
            {
                errors.Add("شناسه پروژه الزامی است");
            }

            // بررسی Weight
            if (dto.Weight.HasValue && dto.Weight.Value <= 0)
            {
                errors.Add(ErrorMessages.Sample.WeightInvalid);
            }

            // بررسی Volume
            if (dto.Volume.HasValue && dto.Volume.Value <= 0)
            {
                errors.Add(ErrorMessages.Sample.VolumeInvalid);
            }

            // بررسی DilutionFactor
            if (dto.DilutionFactor.HasValue && dto.DilutionFactor.Value <= 0)
            {
                errors.Add(ErrorMessages.Sample.DilutionFactorInvalid);
            }

            return errors;
        }

        /// <summary>
        /// اعتبارسنجی UpdateSampleDto
        /// </summary>
        public static List<string> Validate(UpdateSampleDto dto)
        {
            var errors = new List<string>();

            if (dto == null)
            {
                errors.Add(ErrorMessages.Sample.InvalidData);
                return errors;
            }

            // بررسی SampleName
            if (string.IsNullOrWhiteSpace(dto.SampleName))
            {
                errors.Add(ErrorMessages.Sample.SampleNameRequired);
            }

            // بررسی Weight
            if (dto.Weight.HasValue && dto.Weight.Value <= 0)
            {
                errors.Add(ErrorMessages.Sample.WeightInvalid);
            }

            // بررسی Volume
            if (dto.Volume.HasValue && dto.Volume.Value <= 0)
            {
                errors.Add(ErrorMessages.Sample.VolumeInvalid);
            }

            // بررسی DilutionFactor
            if (dto.DilutionFactor.HasValue && dto.DilutionFactor.Value <= 0)
            {
                errors.Add(ErrorMessages.Sample.DilutionFactorInvalid);
            }

            return errors;
        }

        /// <summary>
        /// بررسی اینکه آیا Sample قابل حذف است یا نه
        /// </summary>
        public static bool CanDelete(SampleDto sample)
        {
            if (sample == null) return false;

            // می‌توان نمونه‌هایی که وضعیت Pending دارند را حذف کرد
            return sample.Status == "Pending";
        }

        /// <summary>
        /// بررسی اینکه آیا Sample قابل ویرایش است یا نه
        /// </summary>
        public static bool CanEdit(SampleDto sample)
        {
            if (sample == null) return false;

            // نمی‌توان نمونه‌های Approved را ویرایش کرد
            return sample.Status != "Approved";
        }

        /// <summary>
        /// بررسی محدوده مجاز برای Weight
        /// </summary>
        public static bool IsWeightInRange(decimal weight, decimal min = 0.001m, decimal max = 100m)
        {
            return weight >= min && weight <= max;
        }

        /// <summary>
        /// بررسی محدوده مجاز برای Volume
        /// </summary>
        public static bool IsVolumeInRange(decimal volume, decimal min = 0.1m, decimal max = 1000m)
        {
            return volume >= min && volume <= max;
        }

        /// <summary>
        /// بررسی محدوده مجاز برای DilutionFactor
        /// </summary>
        public static bool IsDilutionFactorInRange(decimal df, decimal min = 1m, decimal max = 10000m)
        {
            return df >= min && df <= max;
        }
    }
}