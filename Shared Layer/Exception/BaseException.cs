namespace Shared.Icp.Exceptions  // ← از BaseException به Exceptions تغییر داد
{
    /// <summary>
    /// کلاس پایه برای تمام Exception های سفارشی سیستم
    /// </summary>
    public abstract class BaseException : Exception
    {
        /// <summary>
        /// کد خطا برای شناسایی نوع خطا
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// زمان وقوع خطا
        /// </summary>
        public DateTime Timestamp { get; }

        protected BaseException(string message, string code)
            : base(message)
        {
            Code = code;
            Timestamp = DateTime.UtcNow;
        }

        protected BaseException(string message, string code, Exception innerException)
            : base(message, innerException)
        {
            Code = code;
            Timestamp = DateTime.UtcNow;
        }
    }
}