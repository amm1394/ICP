namespace Shared.Icp.Constants
{
    /// <summary>
    /// پیام‌های خطای فارسی
    /// </summary>
    public static class ErrorMessages
    {
        /// <summary>
        /// پیام‌های خطای Sample
        /// </summary>
        public static class Sample
        {
            public const string NotFound = "نمونه مورد نظر یافت نشد";
            public const string InvalidData = "اطلاعات نمونه نامعتبر است";
            public const string SampleIdRequired = "شناسه نمونه الزامی است";
            public const string SampleNameRequired = "نام نمونه الزامی است";
            public const string WeightInvalid = "وزن نمونه باید بزرگتر از صفر باشد";
            public const string VolumeInvalid = "حجم نمونه باید بزرگتر از صفر باشد";
            public const string DilutionFactorInvalid = "فاکتور رقیق‌سازی باید بزرگتر از صفر باشد";
            public const string DuplicateSampleId = "شناسه نمونه تکراری است";
            public const string CannotDelete = "امکان حذف این نمونه وجود ندارد";
            public const string CannotEdit = "امکان ویرایش این نمونه وجود ندارد";
        }

        /// <summary>
        /// پیام‌های خطای Element
        /// </summary>
        public static class Element
        {
            public const string NotFound = "عنصر مورد نظر یافت نشد";
            public const string InvalidData = "اطلاعات عنصر نامعتبر است";
            public const string SymbolRequired = "نماد عنصر الزامی است";
            public const string NameRequired = "نام عنصر الزامی است";
            public const string DuplicateSymbol = "نماد عنصر تکراری است";
            public const string DuplicateAtomicNumber = "عدد اتمی تکراری است";
        }

        /// <summary>
        /// پیام‌های خطای CRM
        /// </summary>
        public static class CRM
        {
            public const string NotFound = "ماده مرجع مورد نظر یافت نشد";
            public const string InvalidData = "اطلاعات ماده مرجع نامعتبر است";
            public const string CRMIdRequired = "شناسه ماده مرجع الزامی است";
            public const string NameRequired = "نام ماده مرجع الزامی است";
            public const string DuplicateCRMId = "شناسه ماده مرجع تکراری است";
            public const string ValueRequired = "مقدار گواهی شده الزامی است";
            public const string ValueInvalid = "مقدار گواهی شده باید بزرگتر از صفر باشد";
        }

        /// <summary>
        /// پیام‌های خطای Project
        /// </summary>
        public static class Project
        {
            public const string NotFound = "پروژه مورد نظر یافت نشد";
            public const string InvalidData = "اطلاعات پروژه نامعتبر است";
            public const string NameRequired = "نام پروژه الزامی است";
            public const string DuplicateName = "نام پروژه تکراری است";
            public const string HasSamples = "این پروژه دارای نمونه است و قابل حذف نیست";
        }

        /// <summary>
        /// پیام‌های خطای Measurement
        /// </summary>
        public static class Measurement
        {
            public const string NotFound = "اندازه‌گیری مورد نظر یافت نشد";
            public const string InvalidData = "اطلاعات اندازه‌گیری نامعتبر است";
            public const string SampleRequired = "نمونه الزامی است";
            public const string ElementRequired = "عنصر الزامی است";
            public const string IntensityInvalid = "شدت باید بزرگتر از صفر باشد";
            public const string ConcentrationInvalid = "غلظت نمی‌تواند منفی باشد";
        }

        /// <summary>
        /// پیام‌های خطای QualityCheck
        /// </summary>
        public static class QualityCheck
        {
            public const string NotFound = "کنترل کیفیت مورد نظر یافت نشد";
            public const string InvalidData = "اطلاعات کنترل کیفیت نامعتبر است";
            public const string CheckFailed = "کنترل کیفیت ناموفق بود";
        }

        /// <summary>
        /// پیام‌های خطای عمومی
        /// </summary>
        public static class General
        {
            public const string UnexpectedError = "خطای غیرمنتظره رخ داده است";
            public const string DatabaseError = "خطا در ارتباط با پایگاه داده";
            public const string ValidationError = "خطا در اعتبارسنجی داده‌ها";
            public const string Unauthorized = "شما مجوز دسترسی به این بخش را ندارید";
            public const string NotFound = "موردی یافت نشد";
            public const string InvalidId = "شناسه نامعتبر است";
            public const string RequiredField = "این فیلد الزامی است";
            public const string InvalidFormat = "فرمت داده نامعتبر است";
        }

        /// <summary>
        /// پیام‌های خطای فایل
        /// </summary>
        public static class File
        {
            public const string NotFound = "فایل مورد نظر یافت نشد";
            public const string InvalidFormat = "فرمت فایل نامعتبر است";
            public const string EmptyFile = "فایل خالی است";
            public const string TooLarge = "حجم فایل بیش از حد مجاز است";
            public const string ReadError = "خطا در خواندن فایل";
            public const string WriteError = "خطا در نوشتن فایل";
            public const string UnsupportedFormat = "فرمت فایل پشتیبانی نمی‌شود";
        }

        /// <summary>
        /// پیام‌های خطای Calibration
        /// </summary>
        public static class Calibration
        {
            public const string NotFound = "کالیبراسیون مورد نظر یافت نشد";
            public const string InvalidData = "اطلاعات کالیبراسیون نامعتبر است";
            public const string InsufficientPoints = "تعداد نقاط کالیبراسیون کافی نیست";
            public const string InvalidRSquared = "ضریب همبستگی نامعتبر است";
            public const string CurveNotFound = "منحنی کالیبراسیون یافت نشد";
        }
    }
}