using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services
{
    /// <summary>
    /// نتیجهٔ پردازش که از سرویس پردازش بازگردانده می‌شود.
    /// شامل فیلدهای Data و Messages که Controllerها انتظار دارند.
    /// </summary>
    public sealed record ProcessingResult(
        bool Succeeded,
        object? Data = null,
        IEnumerable<string>? Messages = null,
        string? Error = null
    )
    {
        public static ProcessingResult Success(object? data = null, IEnumerable<string>? messages = null)
            => new ProcessingResult(true, data, messages, null);

        public static ProcessingResult Failure(string error, object? data = null, IEnumerable<string>? messages = null)
            => new ProcessingResult(false, data, messages, error);
    }

    /// <summary>
    /// سرویس پردازش پروژه — interface در لایهٔ Application قرار می‌گیرد.
    /// اضافه شده: EnqueueProcessProjectAsync برای استفادهٔ Controllerها که پردازش را enqueue می‌کنند.
    /// </summary>
    public interface IProcessingService
    {
        /// <summary>
        /// پردازش هم‌زمان پروژه (sync).
        /// </summary>
        Task<ProcessingResult> ProcessProjectAsync(Guid projectId, bool overwriteLatestState = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// صف‌بندی پردازش پروژه (enqueue) — می‌تواند jobId یا اطلاعات دیگر را در Data برگرداند.
        /// پیاده‌سازی فعلی می‌تواند به صورت sync عمل کند یا یک job تولید کند.
        /// </summary>
        Task<ProcessingResult> EnqueueProcessProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
    }
}