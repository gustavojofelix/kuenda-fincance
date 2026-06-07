using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KuendaFinance.Operations.Application.Interfaces;

public interface IStorageService
{
    Task<string> UploadFileAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default);
}
