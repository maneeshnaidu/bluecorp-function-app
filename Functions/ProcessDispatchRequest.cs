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

        public ProcessDispatchRequest(ILogger<ProcessDispatchRequest> logger, IJsonToCsvMapper mapper, ISftpService sftpService)
        {
            _logger = logger;
            _mapper = mapper;
            _sftpService = sftpService;
        }

        [Function("ProcessDispatchRequest")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Processing dispatch request.");

            // Read the request body 
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // Deserialize the JSON into the model 
            var payload = JsonConvert.DeserializeObject<LoadData>(requestBody);

            string tempFilePath = Path.GetTempFileName();

            // Map Json to CSV
            _mapper.MapToCsv(payload, tempFilePath);

            string remoteFileName = $"dispatch-{payload.SalesOrder}.csv";

            // Upload to Blob Storage
            // var blobClient = new BlobContainerClient(Environment.GetEnvironmentVariable("BlobStorageConnectionString"), "incoming");
            // var blob = blobClient.GetBlobClient(remoteFileName);

            // await blob.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(csvContent)), overwrite: true);

            // return req.CreateResponse(HttpStatusCode.OK);

            // Upload to SFTP storage
            await _sftpService.UploadFileAsync(tempFilePath, remoteFileName);

            _logger.LogInformation("Dispatch request processed successfully.");
            return new OkResult();
        }
    }
}
