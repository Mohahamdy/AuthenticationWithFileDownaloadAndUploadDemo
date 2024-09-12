using AuthenticationDemo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AuthenticationDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class FileController(IFiles _fileService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _fileService.GetAllFileAsync());
        }

        [HttpPost]
        public async Task<IActionResult> UploadFilesAsync([FromForm]List<IFormFile> files)
        {
            var result = await _fileService.UploadFilesAsync(files);

            if(result.StatusCode == HttpStatusCode.BadRequest)
                return BadRequest(result);

            if(result.StatusCode == HttpStatusCode.NotFound)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost]
        [Route("DownloadFile")]
        public async Task<IActionResult> DownloadFileAsync([FromBody] string fileName)
        {
            var result = await _fileService.DownloadFileAsync(fileName);

            if (result.StatusCode == HttpStatusCode.BadRequest)
                return BadRequest(result);

            if (result.StatusCode == HttpStatusCode.NotFound)
                return NotFound(result);

            return File(result.Data.MemoryStream, result.Data.FileContentType, result.Data.FileName);
        }
    }
}
