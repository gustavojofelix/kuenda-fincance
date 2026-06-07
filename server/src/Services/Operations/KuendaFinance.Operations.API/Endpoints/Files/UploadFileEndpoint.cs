using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using KuendaFinance.Operations.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace KuendaFinance.Operations.API.Endpoints.Files;

public class UploadFileRequest
{
    public IFormFile File { get; set; } = null!;
}

public class UploadFileResponse
{
    public string Url { get; set; } = string.Empty;
}

public class UploadFileEndpoint : Endpoint<UploadFileRequest, UploadFileResponse>
{
    private readonly IStorageService _storageService;

    public UploadFileEndpoint(IStorageService storageService)
    {
        _storageService = storageService;
    }

    public override void Configure()
    {
        Post("/api/files/upload");
        AllowFileUploads();
        Claims("tenantId"); // Protect via tenant JWT context
    }

    public override async Task HandleAsync(UploadFileRequest req, CancellationToken ct)
    {
        if (req.File == null || req.File.Length == 0)
        {
            AddError("No file provided.");
            ThrowIfAnyErrors();
            return;
        }

        using var stream = req.File.OpenReadStream();
        var url = await _storageService.UploadFileAsync(stream, req.File.FileName, req.File.ContentType, ct);

        HttpContext.Response.StatusCode = 200;
        await HttpContext.Response.WriteAsJsonAsync(new UploadFileResponse { Url = url }, ct);
    }
}
