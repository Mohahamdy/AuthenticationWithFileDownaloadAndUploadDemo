using AuthenticationDemo.Models;
using AuthenticationDemo.Utilies.APIResponse;
using AuthenticationDemo.Utilies.File;

namespace AuthenticationDemo.Services;
public interface IFiles
{
    Task<APIResponseModel<string>> UploadFilesAsync(List<IFormFile> files);
    Task<APIResponseModel<List<UploadedFile>>> GetAllFileAsync();
    Task<APIResponseModel<FileDownload>> DownloadFileAsync(string fileName);
}
