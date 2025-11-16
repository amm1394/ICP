using Core.Icp.Domain.Entities.Elements;
using Core.Icp.Domain.Entities.Projects;
using Core.Icp.Domain.Entities.Samples;
using Core.Icp.Domain.Interfaces.Repositories;
using Core.Icp.Domain.Interfaces.Services;
using Core.Icp.Domain.Models.Files;
using Infrastructure.Icp.Files.Interfaces;
using Infrastructure.Icp.Files.Models;
using Shared.Icp.Exceptions;

namespace Core.Icp.Application.Services.Files
{
    /// <summary>
    /// سرویس سطح Application برای ایمپورت فایل‌های ICP (CSV / Excel)
    /// و ساخت Project / Sample / Measurement در دیتابیس.
    /// </summary>
    public class FileProcessingService : IFileProcessingService
    {
        private readonly ICsvFileProcessor _csvFileProcessor;
        private readonly IExcelFileProcessor _excelFileProcessor;
        private readonly IUnitOfWork _unitOfWork;

        public FileProcessingService(
            ICsvFileProcessor csvFileProcessor,
            IExcelFileProcessor excelFileProcessor,
            IUnitOfWork unitOfWork)
        {
            _csvFileProcessor = csvFileProcessor;
            _excelFileProcessor = excelFileProcessor;
            _unitOfWork = unitOfWork;
        }

        #region Import Methods

        public async Task<ProjectImportResult> ImportCsvAsync(
            string filePath,
            string projectName,
            CancellationToken cancellationToken = default)
        {
            var importResult = await _csvFileProcessor.ImportSamplesAsync(filePath);

            if (!importResult.Success)
            {
                // خطای Validation/Parsing سطح فایل
                throw new FileProcessingException(
                    $"ایمپورت CSV ناموفق بود: {importResult.Message}");
            }

            var project = await CreateProjectWithSamplesAsync(
                projectName,
                importResult.Samples,
                importResult,
                cancellationToken);

            return MapToProjectImportResult(project, importResult);
        }

        public async Task<ProjectImportResult> ImportExcelAsync(
            string filePath,
            string projectName,
            string? sheetName = null,
            CancellationToken cancellationToken = default)
        {
            var importResult = await _excelFileProcessor.ImportSamplesAsync(filePath);

            if (!importResult.Success)
            {
                throw new FileProcessingException(
                    $"ایمپورت Excel ناموفق بود: {importResult.Message}");
            }

            var project = await CreateProjectWithSamplesAsync(
                projectName,
                importResult.Samples,
                importResult,
                cancellationToken);

            return MapToProjectImportResult(project, importResult);
        }

        #endregion

        #region Validation / Extraction

        public async Task<bool> ValidateFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            Infrastructure.Icp.Files.Models.FileImportResult result =
                extension == ".csv"
                    ? await _csvFileProcessor.ImportSamplesAsync(filePath)
                    : await _excelFileProcessor.ImportSamplesAsync(filePath);

            return result.Success;
        }

        public async Task<IEnumerable<Sample>> ExtractSamplesFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            Infrastructure.Icp.Files.Models.FileImportResult result =
                extension == ".csv"
                    ? await _csvFileProcessor.ImportSamplesAsync(filePath)
                    : await _excelFileProcessor.ImportSamplesAsync(filePath);

            if (!result.Success)
            {
                throw new FileProcessingException(
                    $"استخراج داده از فایل ناموفق بود: {result.Message}");
            }

            return result.Samples;
        }

        #endregion

        #region Core Project Creation

        /// <summary>
        /// ساخت Project و اتصال Sample/Measurement با نگاشت خودکار Element ها.
        /// </summary>
        private async Task<Project> CreateProjectWithSamplesAsync(
            string projectName,
            IEnumerable<Sample> samples,
            Infrastructure.Icp.Files.Models.FileImportResult importResult,
            CancellationToken cancellationToken)
        {
            var sampleList = samples.ToList();

            // ۱) برای همه‌ی ElementSymbolها، Element متناظر را یا از DB می‌گیریم،
            //    یا اگر نبود، به‌صورت خودکار می‌سازیم.
            var elementMap = await EnsureElementsForSamplesAsync(
                sampleList,
                cancellationToken);

            // ۲) ساخت پروژه جدید
            var project = new Project
            {
                Name = projectName,
                Description = $"ایمپورت از فایل در تاریخ {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
                CreatedAt = DateTime.UtcNow,
                Status = Core.Icp.Domain.Enums.ProjectStatus.Active,
                Samples = new List<Sample>()
            };

            // ۳) نگاشت Samples و Measurements
            foreach (var sample in sampleList)
            {
                // اتصال Sample به Project
                sample.Project = project;
                sample.ProjectId = project.Id;
                project.Samples.Add(sample);

                if (sample.Measurements == null || sample.Measurements.Count == 0)
                    continue;

                foreach (var measurement in sample.Measurements.ToList())
                {
                    var symbol = measurement.ElementSymbol?.Trim();

                    // اگر سمبل خالی یا null بود، این Measurement را نادیده می‌گیریم
                    if (string.IsNullOrWhiteSpace(symbol))
                    {
                        sample.Measurements.Remove(measurement);
                        continue;
                    }

                    if (!elementMap.TryGetValue(symbol, out var element))
                    {
                        // طبق EnsureElementsForSamplesAsync نباید رخ بدهد،
                        // ولی به صورت دفاعی حذفش می‌کنیم.
                        sample.Measurements.Remove(measurement);
                        continue;
                    }

                    measurement.Sample = sample;
                    measurement.SampleId = sample.Id;

                    measurement.Element = element;
                    measurement.ElementId = element.Id;

                    // هم‌راستا با طراحی Domain: ElementSymbol در Measurement نگه می‌داریم
                    measurement.ElementSymbol = element.Symbol;
                }
            }

            // ۴) ذخیره در DB
            await _unitOfWork.Projects.AddAsync(project, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return project;
        }

        /// <summary>
        /// برای تمام ElementSymbolهای استفاده‌شده در Samples:
        /// - عناصر موجود را از دیتابیس می‌خواند
        /// - برای نمادهای مفقود، Element جدید با AtomicNumber منحصربه‌فرد می‌سازد
        /// - در نهایت دیکشنری Symbol → Element را برمی‌گرداند.
        /// </summary>
        private async Task<Dictionary<string, Element>> EnsureElementsForSamplesAsync(
            IEnumerable<Sample> samples,
            CancellationToken cancellationToken)
        {
            var symbolSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var sample in samples)
            {
                if (sample.Measurements == null) continue;

                foreach (var measurement in sample.Measurements)
                {
                    var symbol = measurement.ElementSymbol?.Trim();
                    if (!string.IsNullOrWhiteSpace(symbol))
                        symbolSet.Add(symbol);
                }
            }

            var elementMap = new Dictionary<string, Element>(StringComparer.OrdinalIgnoreCase);

            if (symbolSet.Count == 0)
                return elementMap;

            // ۱) عناصر موجود در DB را می‌خوانیم
            var existingElements = await _unitOfWork.Elements
                .GetBySymbolsAsync(symbolSet, cancellationToken);

            foreach (var element in existingElements)
            {
                if (!string.IsNullOrWhiteSpace(element.Symbol) &&
                    !elementMap.ContainsKey(element.Symbol))
                {
                    elementMap[element.Symbol] = element;
                }
            }

            // ۲) سمبل‌های مفقود را پیدا می‌کنیم
            var missingSymbols = symbolSet
                .Where(s => !elementMap.ContainsKey(s))
                .ToList();

            if (missingSymbols.Count == 0)
                return elementMap;

            // ۳) یک AtomicNumber یکتا برای عناصر جدید در نظر می‌گیریم
            var maxAtomic = await _unitOfWork.Elements.GetMaxAtomicNumberAsync(cancellationToken);
            // اگر همه 0 بودند، از 1 شروع می‌کنیم
            var nextAtomic = maxAtomic <= 0 ? 1 : maxAtomic + 1;

            foreach (var symbol in missingSymbols)
            {
                var newElement = new Element
                {
                    Id = Guid.NewGuid(),
                    Symbol = symbol,
                    Name = symbol,          // فعلاً خودش، بعداً می‌توانی اسم کامل را در UI ویرایش کنی
                    AtomicNumber = nextAtomic++,
                    IsActive = true
                };

                await _unitOfWork.Elements.AddAsync(newElement, cancellationToken);
                elementMap[symbol] = newElement;
            }

            // SaveChanges اینجا نیاز نیست؛ در CreateProjectWithSamplesAsync روی UnitOfWork انجام می‌شود.
            return elementMap;
        }

        #endregion

        #region Mapping Helpers

        /// <summary>
        /// ترکیب خروجی فایل (FileImportResult) و پروژه‌ی ساخته‌شده
        /// و تولید یک ProjectImportResult سطح Application/Domain.
        /// </summary>
        private static ProjectImportResult MapToProjectImportResult(
            Project project,
            Infrastructure.Icp.Files.Models.FileImportResult importResult)
        {
            var totalSamples = project.Samples?.Count ?? 0;

            var result = new ProjectImportResult
            {
                Project = project,
                Success = importResult.Success,
                Message = importResult.Message,
                TotalRecords = importResult.TotalRecords,
                SuccessfulRecords = importResult.SuccessfulRecords,
                FailedRecords = importResult.FailedRecords,
                TotalSamples = totalSamples,
                // فعلاً لیست خطا/هشدار را خالی می‌گذاریم؛
                // در صورت نیاز می‌توان آن‌ها را از ValidationResult برداشت.
                Errors = new List<string>(),
                Warnings = new List<string>()
            };

            return result;
        }

        #endregion

        private ProjectImportResult CreateProjectImportResult(
            Project project,
            int totalRecords,
            int successfulRecords,
            int failedRecords,
            int totalSamples,
            List<string> errors,
            List<string> warnings)
        {
            return new ProjectImportResult
            {
                Project = project,
                TotalRecords = totalRecords,
                SuccessfulRecords = successfulRecords,
                FailedRecords = failedRecords,
                TotalSamples = totalSamples,
                Errors = errors,
                Warnings = warnings
            };
        }
    }
}
