using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TifToPdf.Models;

namespace TifToPdf.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [RequestSizeLimit(1073741824)]
        [RequestFormLimits(MultipartBodyLengthLimit = 1073741824)]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> ConvertToPdf(ICollection<IFormFile> Files)
        {
        
                var fileRequestModelList = new List<FileRequestModel>();

                foreach (var file in Files)
                {
                    if (file != null)
                    {
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            fileRequestModelList.Add(new FileRequestModel
                            {
                                FileName = file.FileName,
                                FileBase64 = Convert.ToBase64String(ms.ToArray()),
                                Type = Path.GetExtension(file.FileName).Replace(".", string.Empty),
                                Encoding = "utf-8",
                            });
                        }
                    }
                }
            var request = new ConvertRequest();
            request.ResultFileName = "Test";
            request.FileRequestModels = fileRequestModelList;
            request.PdfAConformanceLevel = string.Empty;
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:7112/");

            var response = await httpClient.PostAsJsonAsync("api/Convert", request);


             return File(Convert.FromBase64String(await response.Content.ReadAsStringAsync()), "application/pdf", $"{Guid.NewGuid()}.pdf");

        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class FileModel
    {
        public List<IFormFile> Files { get; set; }
    }

    public class ConvertRequest
    {
        public string ResultFileName { get; set; }
        public List<FileRequestModel> FileRequestModels { get; set; }
        public string PdfAConformanceLevel { get; set; }
    }

    public class FileRequestModel
    {
        public string FileName { get; set; }
        public string FileBase64 { get; set; }
        public string Type { get; set; }
        public string Encoding { get; set; }
    }

}