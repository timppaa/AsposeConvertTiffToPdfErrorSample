using Aspose.Pdf.Facades;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Drawing.Imaging;


namespace ConvertTiffToPdfApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConvertController : Controller
    {
        private readonly IServiceProvider _serviceProvider;

        public ConvertController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [HttpPost]
        [Route("")]
        [RequestSizeLimit(1073741824)]
        [RequestFormLimits(MultipartBodyLengthLimit = 1073741824)]
        [DisableRequestSizeLimit]
        public IActionResult Index(ConvertRequest request)
        {
            var response = ConvertToPdf(request);
            return Ok(response);
        }

        private string ConvertToPdf(ConvertRequest request)
        {
            var resultfileList = new List<string>();
            var conformanceLevel = request.PdfAConformanceLevel;

            foreach (var file in request.FileRequestModels)
            {
                using var pdfHandler = _serviceProvider.GetRequiredService<PDFHandler>();
                switch (file.Type.ToLower())
                {
                    case "tiff":
                    case "tif":
                        resultfileList.Add(pdfHandler.ConvertTiffToPdf(file.FileBase64));
                        break;

                    default:
                        throw new Exception("Filformatet: " + file.Type + " stöds inte.");
                }
            }

            var handler = _serviceProvider.GetRequiredService<PDFHandler>();
            var mergedPdf = handler.MergePdf(resultfileList, request.ResultFileName, conformanceLevel);
            return mergedPdf;
        }
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
