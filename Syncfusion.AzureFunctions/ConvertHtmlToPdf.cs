using System;
using System.IO;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Syncfusion.HtmlConverter;
using Syncfusion.Licensing;

namespace Syncfusion.AzureFunctions
{
    public static class ConvertHtmlToPdf
    {
        private static string _functionAppDirectory;

        [FunctionName("ConvertHtmlToPdf")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log, ExecutionContext executionContext)
        {
            log.Info("C# HTTP trigger function ConvertHtmlToPdf processed a request.");

            SyncfusionLicenseProvider.RegisterLicense(Environment.GetEnvironmentVariable("SyncfusionLicenseKey"));

            _functionAppDirectory = executionContext?.FunctionAppDirectory;

            const string htmlString = "<HTML><BODY><H1>Welcome to Syncfusion.!</H1><P>Simple HTML content</P></BODY></HTML>";

            var htmlStream = ConvertFromHtml(htmlString);

            var res = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(htmlStream.ToArray()) };
            res.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

            return res;
        }

        private static MemoryStream ConvertFromHtml(string htmlString)
        {
            var htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.WebKit);
            var settings = new WebKitConverterSettings
            {
                WebKitPath = $"{_functionAppDirectory}/QtBinaries"
            };

            htmlConverter.ConverterSettings = settings;

            var outputStream = new MemoryStream();
            var document = htmlConverter.Convert(htmlString, "");
            document.Save(outputStream);
            document.Close(true);

            return outputStream;
        }
    }
}
