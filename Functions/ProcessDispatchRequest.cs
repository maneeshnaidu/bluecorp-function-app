using System.Text.Json;
using bluecorp_function_app.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BlueCorp.DispatchFunction
{
    public class ProcessDispatchRequest
    {
        private readonly IJsonToCsvMapper _mapper;
        private readonly ISftpService _sftpService;
        private readonly ILogger<ProcessDispatchRequest> _logger;
        private readonly string _failedFolder;
        private readonly string _incomingFolder;


        public ProcessDispatchRequest(ILogger<ProcessDispatchRequest> logger,  
            IJsonToCsvMapper mapper, 
            ISftpService sftpService)
        {
            _logger = logger;
            _mapper = mapper;
            _sftpService = sftpService;
            _incomingFolder = Environment.GetEnvironmentVariable("SftpRemotePath") ?? string.Empty;
            _failedFolder = Environment.GetEnvironmentVariable("SftpFailedFolder") ?? string.Empty;
        }

        [Function("ProcessDispatchRequest")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            string fullPath = string.Empty;
            _logger.LogInformation("Processing dispatch request.");

            // Read the request body 
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // Deserialize the JSON into the model 
            var payload = JsonConvert.DeserializeObject<LoadData>(requestBody);

            string tempFilePath = Path.GetTempFileName();
            // Map Json to CSV
            _mapper.MapToCsv(payload, tempFilePath);
            // Generate a new GUID
            string guid = Guid.NewGuid().ToString();

            string remoteFileName = $"dispatch-{guid}.csv";

            // Set maximum payload size (800 KB)
            const int maxPayloadSize = 800 * 1024;

            // Check Content-Length header
            if (req.ContentLength > maxPayloadSize)
            {
                fullPath = _failedFolder + remoteFileName;
                // Upload to SFTP storage
            await _sftpService.UploadFileAsync(tempFilePath, fullPath);
                return new BadRequestObjectResult($"Payload exceeds the maximum allowed size of {maxPayloadSize / 1024} KB.");
            }

            

            // Map Json to CSV
            _mapper.MapToCsv(payload, tempFilePath);
            // Generate a new GUID
            guid = Guid.NewGuid().ToString();

            remoteFileName = $"dispatch-{guid}.csv";

            fullPath = _incomingFolder + remoteFileName; 

            // Upload to SFTP storage
            await _sftpService.UploadFileAsync(tempFilePath, fullPath);

            _logger.LogInformation("Dispatch request processed successfully.");
            return new OkResult();
        }
    }
}
