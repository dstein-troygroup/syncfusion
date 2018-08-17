using System;
using System.IO;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Syncfusion.Licensing;
using Syncfusion.OfficeChartToImageConverter;

namespace Syncfusion.AzureFunctions
{
    public static class ConvertPresentationToPdf
    {
        private static string _functionAppDirectory;

        [FunctionName("ConvertPresentationToPdf")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log, ExecutionContext executionContext)
        {
            log.Info("C# HTTP trigger function ConvertPresentationToPdf processed a request.");

            SyncfusionLicenseProvider.RegisterLicense(Environment.GetEnvironmentVariable("SyncfusionLicenseKey"));

            _functionAppDirectory = executionContext?.FunctionAppDirectory;

            var docStream = new MemoryStream();

            using (var reader = new StreamReader($"{_functionAppDirectory}/Files/PowerPoint.pptx"))
            {
                reader.BaseStream.CopyTo(docStream);
                docStream.Position = 0;
            }

            var htmlStream = ConvertFromPresentation(docStream);

            var res = new HttpResponseMessage(HttpStatusCode.OK) {Content = new ByteArrayContent(htmlStream.ToArray())};
            res.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

            return res;
        }

        private static MemoryStream ConvertFromPresentation(Stream docStream)
        {
            // Opens a PowerPoint Presentation
            var presentation = Presentation.Presentation.Open(docStream);

            // Creates an instance of ChartToImageConverter and assigns it to ChartToImageConverter property of Presentation
            presentation.ChartToImageConverter = new ChartToImageConverter();

            // Converts the PowerPoint Presentation into PDF document
            var pdfDocument = PresentationToPdfConverter.PresentationToPdfConverter.Convert(presentation);

            // Saves the PDF document
            var outputStream = new MemoryStream();
            pdfDocument.Save(outputStream);

            // Closes the PDF document
            pdfDocument.Close(true);

            // Closes the Presentation
            presentation.Close();

            return outputStream;
        }
    }
}
