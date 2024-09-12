using AuthenticationDemo.Models;
using AuthenticationDemo.Utilies.APIResponse;
using AuthenticationDemo.Utilies.File;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;

namespace AuthenticationDemo.Services;

public class Files(IWebHostEnvironment _environment,AuthDbContext _context,IOptions<FileConstrains> options) : IFiles
{
    FileConstrains fileConstrains = options.Value;

    //Get all files
    public async Task<APIResponseModel<List<UploadedFile>>> GetAllFileAsync()
    {
       return APIResponseFactory<List<UploadedFile>>.Success(await _context.UploadedFiles.ToListAsync(),"All files",HttpStatusCode.OK);
    }

    //Upload files
    public async Task<APIResponseModel<string>> UploadFilesAsync(List<IFormFile> files)
    {
        if(!files.Any())
            return APIResponseFactory<string>.Failure("You should add files!", "Not found files", HttpStatusCode.NotFound);
        
       List<string> errors = await ChecksOnFiles(files);

        if(errors.Any())
            return APIResponseFactory<string>.Failure(string.Join(", ",errors), "Failed to upload", HttpStatusCode.BadRequest);

        List<UploadedFile> uploadedFiles = [];

        foreach (var file in files)
        {
            var fakeFile = Path.GetRandomFileName();

            UploadedFile uploadedFile = new()
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                StoredFileName = fakeFile
            };

            uploadedFiles.Add(uploadedFile);

            var path = Path.Combine(_environment.WebRootPath, "uploads",fakeFile);
            using FileStream fileStream = new(path, FileMode.Create);

            await file.CopyToAsync(fileStream);
        }

        await _context.UploadedFiles.AddRangeAsync(uploadedFiles);
        await _context.SaveChangesAsync();

        return 
            APIResponseFactory<string>.Success("Uploaded files successfuly","Success",HttpStatusCode.OK);
    }

    //Download file
    public async Task<APIResponseModel<FileDownload>> DownloadFileAsync(string fileName)
    {
        var file = await _context.UploadedFiles.FirstOrDefaultAsync(x => x.StoredFileName == fileName);

        if(file is null)
            return APIResponseFactory<FileDownload>.Failure(null, "File Not found", HttpStatusCode.NotFound);

        var path = Path.Combine(_environment.WebRootPath, "uploads", file.StoredFileName);

        var memoryStream = new MemoryStream();

        using FileStream fileStream = new(path, FileMode.Open);

        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;  

        FileDownload fileDownload = new()
        {
            MemoryStream = memoryStream,
            FileContentType = file.ContentType,
            FileName = file.FileName,
        };

        return APIResponseFactory<FileDownload>.Success(fileDownload, "Download successfuly", HttpStatusCode.OK); 
    }

    //Check for file existing, formationg and size
    private async Task<List<string>> ChecksOnFiles(List<IFormFile> files)
    {
        List<string> errors = [];

        var uploadedFilesDb = await _context.UploadedFiles.ToListAsync();

        foreach (var file in files)
        {
            if (uploadedFilesDb.Exists(f => f.FileName == file.FileName))
            {
                errors.Add($"File {file.FileName} is already existed");
                continue;
            }

            if (!fileConstrains.Extensions!.Contains(file.ContentType))
            {
                errors.Add($"File {file.FileName} should be with extentions (pdf|png|jpeg)!");
                continue;
            }

            if (file.Length >= fileConstrains.Size)
            {
                errors.Add($"File {file.FileName} should be less than 3mb!");
                continue;
            }
        }

        return errors;
    }
}
