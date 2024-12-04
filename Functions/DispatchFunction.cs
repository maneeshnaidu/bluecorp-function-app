using System;
using System.IO;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging;
using System.Text.Json;


public class DispatchFunction
{
    private readonly JsonToCsvMapper _mapper;
    private readonly SftpService _sftpService;

    public DispatchFunction(JsonToCsvMapper mapper, SftpService sftpService)
    {
        _mapper = mapper;
        _sftpService = sftpService;
    }

    [Function("ProcessDispatchRequest")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("Processing dispatch request.");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        // Deserialize Json payload
        var loadData = JsonSerializer.Deserialize<LoadData>(requestBody);

        string tempFilePath = Path.GetTempFileName();

        // Map Json to CSV
        _mapper.MapToCsv(loadData, tempFilePath);

        string remoteFileName = $"dispatch-{loadData.SalesOrder}.csv";

        // Upload to SFTP storage
        await _sftpService.UploadFileAsync(tempFilePath, remoteFileName);

        log.LogInformation("Dispatch request processed successfully.");
        return new OkResult();
    }
}
