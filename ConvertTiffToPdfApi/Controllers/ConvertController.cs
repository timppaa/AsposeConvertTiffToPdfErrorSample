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
        [HttpPost]
        [Route("")]
        [RequestSizeLimit(1073741824)]
        [RequestFormLimits(MultipartBodyLengthLimit = 1073741824)]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Index(ConvertRequest request)
        {
            var response = ConvertToPdf(request);
            return Ok(response);
        }

        private string ConvertToPdf(ConvertRequest request)
        {

            List<string> resultfileList = new List<string>();
            var conformanceLevel = request.PdfAConformanceLevel;


            foreach (var file in request.FileRequestModels)
            {
                switch (file.Type.ToLower())
                {

                    case "tiff":
                    case "tif":
                        resultfileList.Add(ConvertTiffToPdf(file));
                        break;

                    default:
                        throw new Exception("Filformatet: " + file.Type + " stöds inte.");
                }
            }

            var mergedPdf = MergePdf(resultfileList, request.ResultFileName, conformanceLevel);
            resultfileList.Clear();
            request.FileRequestModels.Clear();
            return mergedPdf;
        }

        private string ConvertTiffToPdf(FileRequestModel file)
        {
            try
            {
                /*using*/
                var pdf = new Aspose.Pdf.Document();
                /*using*/
                var pdfStream = new MemoryStream();
                /*using*/
                var ms = new MemoryStream(Convert.FromBase64String(file.FileBase64));
                /*using*/
                var myImage = new Bitmap(ms);

                var dimension = new FrameDimension(myImage.FrameDimensionsList[0]);
                var frameCount = myImage.GetFrameCount(dimension);

                //var currentImageList = new List<MemoryStream>();
                for (int frameIdx = 0; frameIdx <= frameCount - 1; frameIdx++)
                {

                    var page = pdf.Pages.Add();

                    myImage.SelectActiveFrame(dimension, frameIdx);
                    var currentImage = new MemoryStream();
                    //currentImageList.Add(currentImage);
                    myImage.Save(currentImage, ImageFormat.Tiff);
                    var image = new Aspose.Pdf.Image { ImageStream = currentImage };


                    //image = DownScaleImageToA4(myImage.Height, myImage.Width, image);
                    //SetPageInfo(page);

                    //Denna flagga gör så att det ibland smäller oväntat. Nackdelen med att ha den av är att filstorleken blir enormt mycket större än om man har den på.
                    image.IsBlackWhite = true;



                    page.Paragraphs.Add(image);
                }

                //OptimizePdf(pdf);

                pdf.Save(pdfStream, Aspose.Pdf.SaveFormat.Pdf);
                pdfStream.Position = 0;

                //foreach(var stream in currentImageList)
                //{
                //    stream.Dispose();
                //}

                //pdf.FreeMemory();
                return Convert.ToBase64String(pdfStream.ToArray());
            }
            catch (Exception e)
            {

                var test = e;
                throw;
            }
        }

        private string MergePdf(List<string> base64FileStrings, string resultFilename,
            string conformanceLevel = null)
        {
            try
            {
 

                var inputStreams = new MemoryStream[base64FileStrings.Count];
                for (var i = 0; i < base64FileStrings.Count; i++)
                {
                    inputStreams[i] = new MemoryStream(Convert.FromBase64String(base64FileStrings[i]));
                }

                var pdfEditor = new PdfFileEditor();
                var pdfStream = new MemoryStream();
                pdfEditor.Concatenate(inputStreams, pdfStream);
                var pdf = new Aspose.Pdf.Document(pdfStream);
                pdf.Info.Title = resultFilename ?? "TS.AHS.PDF";

                var returnStream = new MemoryStream();
                pdf.Save(returnStream);

                returnStream.Position = 0;
                var base64 = Convert.ToBase64String(returnStream.ToArray());

               
  
                return base64;
            }

            catch (Exception e)
            {

                var test = e;
                throw;
            }
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
