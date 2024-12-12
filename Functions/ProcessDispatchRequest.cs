using System.Text.Json;
using bluecorp_function_app.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace BlueCorp.DispatchFunction
{
    public class ProcessDispatchRequest
    {
        private readonly IJsonToCsvMapper _mapper;
        private readonly ISftpService _sftpService;
        private readonly ILogger<ProcessDispatchRequest> _logger;
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly IControlNumberValidationService _validationService;
        private readonly string _failedFolder;
        private readonly string _incomingFolder;


        public ProcessDispatchRequest(ILogger<ProcessDispatchRequest> logger,
            IJsonToCsvMapper mapper,
            ISftpService sftpService,
            IConnectionMultiplexer redisConnection,
            IControlNumberValidationService validationService)
        {
            _logger = logger;
            _mapper = mapper;
            _sftpService = sftpService;
            _redisConnection = redisConnection;
            _validationService = validationService;
            _incomingFolder = Environment.GetEnvironmentVariable("SftpRemotePath") ?? string.Empty;
            _failedFolder = Environment.GetEnvironmentVariable("SftpFailedFolder") ?? string.Empty;
        }

        [Function("ProcessDispatchRequest")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            string tempFilePath = Path.GetTempFileName();
            _logger.LogInformation("Processing dispatch request.");
            var redisDb = _redisConnection.GetDatabase();

            // Read the request body 
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // Deserialize the JSON into the model 
            var payload = JsonConvert.DeserializeObject<LoadData>(requestBody);

            // Extract the control number from the payload (assuming it's an integer)
            if (!int.TryParse(payload?.ControlNumber.ToString(), out int controlNumber))
            {
                return new BadRequestObjectResult("Control number is missing or not a valid integer.");
            }

            // Validate the control number using the service
            bool isValid = await _validationService.IsControlNumberIncrementedAsync(controlNumber);

            if (!isValid)
            {
                // Map Json to CSV
                _mapper.MapToCsv(payload, tempFilePath);
                // Upload to SFTP storage
                await _sftpService.UploadFileAsync(tempFilePath, FileUploadHelper(_failedFolder));
                // Store ControlNumber
                await _validationService.StoreControlNumberAsync(payload.ControlNumber);
            }

            // Set maximum payload size (800 KB)
            const int maxPayloadSize = 800 * 1024;

            // Check Content-Length header
            if (req.ContentLength > maxPayloadSize)
            {
                return new BadRequestObjectResult($"Payload exceeds the maximum allowed size of {maxPayloadSize / 1024} KB.");
            }

            // Map Json to CSV
            _mapper.MapToCsv(payload, tempFilePath);
            // Generate a new GUID
            string guid = Guid.NewGuid().ToString();
            string remoteFileName = $"dispatch-{guid}.csv";

            // Upload to SFTP storage
            await _sftpService.UploadFileAsync(tempFilePath, remoteFileName);

            _logger.LogInformation("Dispatch request processed successfully.");
            return new OkResult();
        }

        public static string FileUploadHelper(string? destinationPath)
        {
            // Generate a new GUID
            string guid = Guid.NewGuid().ToString();

            string remoteFileName = $"dispatch-{guid}.csv";

            if (!string.IsNullOrEmpty(destinationPath))
            {
                string fullPath = destinationPath + remoteFileName;
                return fullPath;
            }

            return remoteFileName;

        }

    }
}
