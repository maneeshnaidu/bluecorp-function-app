using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bluecorp_function_app.Interfaces
{
    public interface ISftpService
    {
        Task UploadFileAsync(string localFilePath, string remoteFileName);
    }
}