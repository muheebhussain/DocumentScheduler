namespace Logging
{
    public class StorageOptions
    {
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
        public string DefaultLogFileName { get; set; }
    }
}

using System;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Logging
{
    public class LoggerFactory
    {
        private readonly StorageOptions _storageOptions;

        public LoggerFactory(IOptions<StorageOptions> storageOptions)
        {
            _storageOptions = storageOptions.Value;
        }

        public ILogger CreateLogger(string fileName)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.AzureBlobStorage(
                    connectionString: _storageOptions.ConnectionString,
                    formatter: new CompactJsonFormatter(),
                    storageContainerName: _storageOptions.ContainerName,
                    storageFileName: fileName)
                .CreateLogger();
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using FileService;
using FileParserService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BankAccountFileProcessor
{
    public class Worker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        public Worker(ILogger logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var fileService = scope.ServiceProvider.GetRequiredService<FileService>();
                var fileParserService = scope.ServiceProvider.GetRequiredService<FileParserService>();

                string filePath = "<path_to_your_bank_account_file>";
                await fileService.ProcessFileAsync(filePath, fileParserService);
            }
        }
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using Logging;
using Serilog;

namespace FileService
{
    public class FileService
    {
        private readonly ILogger _logger;

        public FileService(LoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("FileServiceLog");
        }

        public async Task ProcessFileAsync(string filePath, FileParserService fileParserService)
        {
            if (!File.Exists(filePath))
            {
                _logger.Error("File not found: {FilePath}", filePath);
                return;
            }

            _logger.Information("Processing file: {FilePath}", filePath);

            using (var fileStream = File.OpenRead(filePath))
            {
                await fileParserService.ParseAsync(fileStream);
            }
        }
    }
}

using System;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.MSSqlServer;

namespace Logging
{
    public class LoggerFactory
    {
        private readonly StorageOptions _storageOptions;

        public LoggerFactory(IOptions<StorageOptions> storageOptions)
        {
            _storageOptions = storageOptions.Value;
        }

        public ILogger CreateLogger(string fileName)
        {
            var columnOptions = new ColumnOptions();
            columnOptions.Store.Remove(StandardColumn.Properties);
            columnOptions.Store.Add(StandardColumn.LogEvent);

            return new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.AzureBlobStorage(
                    connectionString: _storageOptions.ConnectionString,
                    formatter: new CompactJsonFormatter(),
                    storageContainerName: _storageOptions.ContainerName,
                    storageFileName: fileName)
                .WriteTo.MSSqlServer(
                    connectionString: _storageOptions.SqlConnectionString,
                    tableName: _storageOptions.SqlTableName,
                    columnOptions: columnOptions,
                    restrictedToMinimumLevel: LogEventLevel.Information)
                .CreateLogger();
        }
    }
}

namespace Logging
{
    public class StorageOptions
    {
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
        public string DefaultLogFileName { get; set; }
        public string SqlConnectionString { get; set; }
        public string SqlTableName { get; set; }
    }
}



