using System;
using System.IO;
using System.Threading.Tasks;
using Renci.SshNet;

public class SftpService
{
    private readonly string _host;
    private readonly string _username;
    private readonly string _privateKeyPath;

    public SftpService(string host, string username, string privateKeyPath)
    {
        _host = host;
        _username = username;
        _privateKeyPath = privateKeyPath;
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

            using var fileStream = File.OpenRead(localFilePath);
            await Task.Run(() => sftp.UploadFile(fileStream, remoteFileName));

            sftp.Disconnect();
        }
        catch (Exception ex)
        {
            throw new Exception($"SFTP upload failed: {ex.Message}");
        }
    }
}
