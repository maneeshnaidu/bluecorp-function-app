using System;
using bluecorp_function_app.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace BlueCorp.DispatchFunction
{
    public class SftpFileMover
    {
        private readonly ILogger _logger;
        private readonly ISftpService _sftpService;
        private readonly string _incomingFolder;
        private readonly string _processedFolder;

        public SftpFileMover(ILoggerFactory loggerFactory, ISftpService sftpService)
        {
            _logger = loggerFactory.CreateLogger<SftpFileMover>();
            _sftpService = sftpService;
            _incomingFolder = Environment.GetEnvironmentVariable("SftpRemotePath");
            _processedFolder = Environment.GetEnvironmentVariable("SftpProcessedFolder");
        }

        [Function("SftpFileMover")]
        public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"SFTP File Mover function executed at: {DateTime.Now}");

            // Upload to SFTP storage
            await _sftpService.MoveCsvFilesAsync();

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }

    }
}