# 📦 Isatis ICP - Application Layer

Business logic interfaces and DTOs.

## 📋 Overview

این لایه شامل:
- **Interfaces**: قراردادهای سرویس‌ها
- **DTOs**: Data Transfer Objects برای Request/Response

## 📁 Structure

```
Application/
├── DTOs/
│   ├── AuthDtos.cs              # Login, Register, User DTOs
│   ├── CorrectionDtos.cs        # Weight/Volume correction
│   ├── DriftDTOs.cs             # Drift correction
│   ├── ImportDtos.cs            # Import related
│   ├── PivotRequest.cs          # Pivot operations
│   ├── OptimizedSampleDto.cs    # Optimization results
│   ├── CrmDtos.cs               # CRM data
│   ├── RmCheckDtos. cs           # RM verification
│   └── ReportDtos.cs            # Report generation
│
└── Interface/
    ├── IAuthService.cs          # Authentication
    ├── IImportService.cs        # Data import
    ├── IProcessingService.cs    # Data processing
    ├── IPivotService.cs         # Pivot operations
    ├── ICorrectionService.cs    # Corrections
    ├── IDriftCorrectionService.cs # Drift
    ├── IOptimizationService.cs  # Optimization
    ├── ICrmService. cs           # CRM management
    ├── IRmCheckService. cs       # RM checks
    ├── IReportService.cs        # Reports
    └── IChangeLogService.cs     # Audit logging
```

## 🎯 Design Principles

- **Clean Architecture**: جداسازی concerns
- **Dependency Inversion**: وابستگی به abstractions نه implementations
- **Single Responsibility**: هر interface یک مسئولیت