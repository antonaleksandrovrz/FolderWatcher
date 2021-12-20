using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FolderWatcher
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(@"C:\Users\Anton\Desktop\TestFolder");

        public Worker(ILogger<Worker> logger)
        {
            fileSystemWatcher.EnableRaisingEvents = true;
            fileSystemWatcher.IncludeSubdirectories = true;

            fileSystemWatcher.Changed += file_Changed;
            fileSystemWatcher.Created += file_Created;
            fileSystemWatcher.Deleted += file_Deleted;

            _logger = logger;
        }

        private void file_Deleted(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation("File: {0} was deleted.",e.Name);
        }

        private void file_Created(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation("File: {0} was created.",e.Name);
        }

        private void file_Changed(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation("File: {0} was changed.",e.Name);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
