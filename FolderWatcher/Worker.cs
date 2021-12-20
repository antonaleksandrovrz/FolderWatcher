using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace FolderWatcher
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(@"C:\TestFolder");
        List<byte[]> hashedFiles = new List<byte[]>();

        public Worker(ILogger<Worker> logger)
        {
            fileSystemWatcher.EnableRaisingEvents = true;
            fileSystemWatcher.IncludeSubdirectories = true;

            fileSystemWatcher.Changed += file_Changed;
            fileSystemWatcher.Created += file_Created;
            fileSystemWatcher.Deleted += file_Deleted;

            _logger = logger;

            _logger.LogInformation(@"Reading all the files from C:\TestFolder");
            ShowAllHashes();
            _logger.LogInformation("Done");
        }

        private void ShowAllHashes()
        {
            foreach (var item in Directory.GetFiles(@"C:\TestFolder"))
            {
                using (var md5 = MD5.Create())
                {
                    try
                    {
                        byte[] hash = Get_Hash(item);

                        if (hashedFiles.Any(x => x.SequenceEqual(hash)))
                        {
                            _logger.LogWarning("Dublicated hash: " + BitConverter.ToString(hash).Replace("-", ""));
                        }

                        else
                        {
                            _logger.LogInformation("File hash: " + BitConverter.ToString(hash).Replace("-", ""));
                        }

                        hashedFiles.Add(hash);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation(ex.Message);
                    }

                }
            }
        }
        private void file_Deleted(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation("File: {0} was deleted.",e.Name);
        }

        private void file_Created(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation("File: {0} was created.",e.Name);
            CreateMD5(e.FullPath);
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

        void CreateMD5(string path)
        {
            using (var md5 = MD5.Create())
            {
                try
                {
                    byte[] hash = Get_Hash(path);
                    _logger.LogInformation("With hash: " + BitConverter.ToString(hash).Replace("-", ""));

                    if (!hashedFiles.Any(x => x.SequenceEqual(hash)))
                    {
                        hashedFiles.Add(hash);
                    }

                    else
                    {
                        _logger.LogWarning("Dublicated File! It can be virus!");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex.Message);
                }

                finally
                {
                    md5.Dispose();
                }
            }
        }

        byte[] Get_Hash(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    try
                    {
                        return md5.ComputeHash(stream);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                        throw;
                    }

                    finally
                    {
                        stream.Close();
                    }
                }
            }
        }
    }
}
