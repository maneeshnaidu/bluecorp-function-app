using System;
using System.IO;
using System.Threading.Tasks;
using bluecorp_function_app.Interfaces;
using Microsoft.Extensions.Logging;
using Renci.SshNet;

public class SftpService : ISftpService
{
    private readonly string _host;
    private readonly string _username;
    private readonly string _privateKeyPath;
    private readonly string _incomingFolder;
    private readonly string _processedFolder;
    private readonly string _failedFolder;
    private readonly ILogger _logger;

    public SftpService(ILoggerFactory loggerFactory)
    {
        _host = Environment.GetEnvironmentVariable("SftpHost");
        _username = Environment.GetEnvironmentVariable("SftpUsername");
        _privateKeyPath = Environment.GetEnvironmentVariable("SftpPrivateKeyPath");
        _incomingFolder = Environment.GetEnvironmentVariable("SftpRemotePath");
        _processedFolder = Environment.GetEnvironmentVariable("SftpProcessedFolder");
        _failedFolder = Environment.GetEnvironmentVariable("SftpFailedFolder");
        _logger = loggerFactory.CreateLogger<SftpService>();
    }

    public async Task UploadFileAsync(string localFilePath, string remoteFileName)
    {
        using var privateKey = new PrivateKeyFile(_privateKeyPath);
        var connectionInfo = new ConnectionInfo(_host, _username, new PrivateKeyAuthenticationMethod(_username, privateKey));
        using var sftp = new SftpClient(connectionInfo);

        try
        {
            sftp.Connect();
            if (!sftp.IsConnected) throw new Exception("SFTP connection failed.");

            // Check if the destination folder exists, if not, create it
            if (!sftp.Exists(_incomingFolder))
            {
                _logger.LogInformation($"Folder {_incomingFolder} does not exist. Creating it...");
                sftp.CreateDirectory(_incomingFolder);
            }
            else
            {
                _logger.LogInformation($"Folder {_incomingFolder} already exists.");
            }


            string fullPath = _incomingFolder + remoteFileName;

            using var fileStream = File.OpenRead(localFilePath);
            await Task.Run(() => sftp.UploadFile(fileStream, fullPath));

            sftp.Disconnect();
        }
        catch (Exception ex)
        {
            throw new Exception($"SFTP upload failed: {ex.Message}");
        }
    }

    public async Task MoveCsvFilesAsync()
    {
        using var privateKey = new PrivateKeyFile(_privateKeyPath);
        var connectionInfo = new ConnectionInfo(_host, _username, new PrivateKeyAuthenticationMethod(_username, privateKey));
        using var sftp = new SftpClient(connectionInfo);

        try
        {
            sftp.Connect();
            if (!sftp.IsConnected) throw new Exception("SFTP connection failed.");

            // List files in the source folder
            var files = sftp.ListDirectory(_incomingFolder);

            // Filter CSV files
            var csvFiles = files.Where(file => !file.IsDirectory && file.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)).ToList();

            if (!csvFiles.Any())
            {
                _logger.LogInformation("No CSV files found in the source folder.");
            }

            // Move each CSV file from source to destination
            foreach (var file in csvFiles)
            {
                try
                {
                    string sourceFilePath = file.FullName;
                    string destinationFilePath = Path.Combine(_processedFolder, file.Name);

                    _logger.LogInformation($"Moving file {file.Name} from {_incomingFolder} to {_processedFolder}.");

                    // Check if the destination folder exists, if not, create it
                    if (!sftp.Exists(_processedFolder))
                    {
                        _logger.LogInformation($"Folder {_processedFolder} does not exist. Creating it...");
                        sftp.CreateDirectory(_processedFolder);
                    }
                    else
                    {
                        _logger.LogInformation($"Folder {_processedFolder} already exists.");
                    }

                    // Rename (move) the file within the SFTP server
                    await Task.Run(() => sftp.RenameFile(sourceFilePath, destinationFilePath));
                    _logger.LogInformation($"Moved file {file.Name} successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error moving file {file.Name}: {ex.Message}");
                }
            }

            sftp.Disconnect();
            _logger.LogInformation("Disconnected from SFTP server.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while moving CSV files: {ex.Message}");
        }
    }

    // Helper method to check if the folder exists
    private static bool FolderExists(SftpClient client, string folderPath)
    {
        try
        {
            var directoryList = client.ListDirectory(folderPath);
            return directoryList != null && directoryList.GetEnumerator().MoveNext(); // Check if folder exists by listing its content
        }
        catch (Exception)
        {
            return false; // If an error occurs (e.g., directory doesn't exist), return false
        }
    }

}
