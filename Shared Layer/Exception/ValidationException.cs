namespace Shared.Icp.Exceptions
{
    /// <summary>
    /// Exception برای خطاهای اعتبارسنجی
    /// </summary>
    public class ValidationException : BaseException
    {
        // تغییر نوع از List<string> به string[] برای سازگاری با API
        public Dictionary<string, string[]> Errors { get; }

        public ValidationException(string message)
            : base(message, "VALIDATION_ERROR")
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(Dictionary<string, string[]> errors)
            : base("یک یا چند خطای اعتبارسنجی رخ داده است", "VALIDATION_ERROR")
        {
            Errors = errors;
        }

        public ValidationException(string field, string error)
            : base($"خطای اعتبارسنجی: {error}", "VALIDATION_ERROR")
        {
            Errors = new Dictionary<string, string[]>
            {
                { field, new[] { error } }
            };
        }

        // Helper برای تبدیل از List به Array
        public ValidationException(Dictionary<string, List<string>> errors)
            : base("یک یا چند خطای اعتبارسنجی رخ داده است", "VALIDATION_ERROR")
        {
            Errors = errors.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToArray()
            );
        }
    }
}