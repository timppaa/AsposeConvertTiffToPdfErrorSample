using Aspose.Pdf.Facades;
using ConvertTiffToPdfApi.Controllers;
using System.Drawing.Imaging;
using System.Drawing;
using Aspose.Pdf;

namespace ConvertTiffToPdfApi
{
    public class PDFHandler : IDisposable
    {
        private readonly Document _pdf;
        public PDFHandler()
        {
            _pdf = new Document();
        }

        public string ConvertTiffToPdf(string file)
        {
            using var pdfStream = new MemoryStream();

            var ms = new MemoryStream(Convert.FromBase64String(file));
            using var myImage = new Bitmap(ms);

            var dimension = new FrameDimension(myImage.FrameDimensionsList[0]);
            var frameCount = myImage.GetFrameCount(dimension);

            for (int frameIdx = 0; frameIdx <= frameCount - 1; frameIdx++)
            {
                var page = _pdf.Pages.Add();
                myImage.SelectActiveFrame(dimension, frameIdx);
                var currentImage = new MemoryStream();
                myImage.Save(currentImage, ImageFormat.Tiff);
                var image = new Aspose.Pdf.Image
                {
                    ImageStream = currentImage,
                    IsBlackWhite = true
                };

                page.Paragraphs.Add(image);
            }

            _pdf.Save(pdfStream, SaveFormat.Pdf);
            pdfStream.Position = 0;

            return Convert.ToBase64String(pdfStream.ToArray());
        }

        public string MergePdf(List<string> base64FileStrings, string resultFilename,
            string conformanceLevel = null)
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
            pdf.Info.Title = resultFilename ?? "PDF";

            var returnStream = new MemoryStream();
            pdf.Save(returnStream);

            returnStream.Position = 0;
            var base64 = Convert.ToBase64String(returnStream.ToArray());

            return base64;
        }

        public void Dispose()
        {
            _pdf?.Dispose();
        }
    }
}
