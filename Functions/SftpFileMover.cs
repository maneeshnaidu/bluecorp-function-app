using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace BlueCorp.DispatchFunction
{
    public class SftpFileMover
    {
        private readonly ILogger _logger;

        public SftpFileMover(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SftpFileMover>();
        }

        [Function("SftpFileMover")]
        public void Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"SFTP File Mover function executed at: {DateTime.Now}");

            try
            {
                // SFTP Configuration
                string _host = Environment.GetEnvironmentVariable("SftpHost");
                int _port = int.Parse(Environment.GetEnvironmentVariable("SftpPort") ?? "22");
                string _username = Environment.GetEnvironmentVariable("SftpUsername");
                string _privateKeyPath = Environment.GetEnvironmentVariable("SftpPrivateKeyPath");
                string _incomingFolder = Environment.GetEnvironmentVariable("SftpRemotePath");
                string _processedFolder = Environment.GetEnvironmentVariable("SftpProcessedFolder");
                string _failedFolder = Environment.GetEnvironmentVariable("SftpFailedFolder");

                // Connect to SFTP
                using var privateKey = new PrivateKeyFile(_privateKeyPath);
                var connectionInfo = new ConnectionInfo(_host, _username, new PrivateKeyAuthenticationMethod(_username, privateKey));
                using var sftp = new SftpClient(connectionInfo);

                // Get the list of CSV files from the incoming folder
                var files = sftp.ListDirectory(_incomingFolder).Where(f => f.IsRegularFile && f.Name.EndsWith(".csv")).ToList();

                foreach (var file in files)
                {
                    try
                    {
                        string sourcePath = $"{_incomingFolder}/{file.Name}";
                        string targetPathProcessed = $"{_processedFolder}/{file.Name}";
                        string targetPathFailed = $"{_failedFolder}/{file.Name}";

                        // Download the file to a local memory stream (optional, for inspection or modification)
                        using var memoryStream = new MemoryStream();
                        sftp.DownloadFile(sourcePath, memoryStream);
                        memoryStream.Position = 0; // Reset stream position for further use

                        // Simulate processing of the CSV file (replace with actual logic)
                        bool isProcessedSuccessfully = ProcessCsvFile(memoryStream);

                        if (isProcessedSuccessfully)
                        {
                            // Move file to processed folder
                            sftp.RenameFile(sourcePath, targetPathProcessed);
                            _logger.LogInformation($"File {file.Name} moved to processed folder.");
                        }
                        else
                        {
                            // Move file to failed folder
                            sftp.RenameFile(sourcePath, targetPathFailed);
                            _logger.LogError($"Processing failed for file {file.Name}. Moved to failed folder.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing file {file.Name}: {ex.Message}");
                        // Move file to failed folder in case of error
                        string targetPathFailed = $"{_failedFolder}/{file.Name}";
                        sftp.RenameFile(file.FullName, targetPathFailed);
                        _logger.LogError($"File {file.Name} moved to failed folder due to an error.");
                    }
                }

                sftp.Disconnect();
                _logger.LogInformation("Disconnected from SFTP server.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during SFTP file processing: {ex.Message}");
            }

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }


        private bool ProcessCsvFile(MemoryStream memoryStream)
        {
            try
            {
                // Placeholder for actual CSV processing logic
                // Simulate processing logic (return true if processed successfully)
                _logger.LogInformation("Processing CSV file...");
                return true; // Simulate success, replace with actual processing logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing CSV file: {ex.Message}");
                return false; // Return false if processing fails
            }
        }
    }
}