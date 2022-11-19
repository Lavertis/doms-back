using DoctorsOffice.DocumentGenerator.DocumentTemplates.Prescription;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using Razor.Templating.Core;

namespace DoctorsOffice.DocumentGenerator.DocumentGenerator;

public class DocumentGenerator : IDocumentGenerator
{
    public async Task<MemoryStream> GeneratePrescriptionAsPdf(PrescriptionTemplateData data)
    {
        const string templatePath = "DocumentTemplates/Prescription/Prescription.cshtml";
        var prescriptionHtmlString = await RazorTemplateEngine.RenderAsync(templatePath, data);
        var pdfStream = await ConvertHtmlToPdfAsync(prescriptionHtmlString);
        return pdfStream;
    }

    private static async Task<MemoryStream> ConvertHtmlToPdfAsync(string html)
    {
        using var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args = new[] {"--no-sandbox"}
        });
        await using var page = await browser.NewPageAsync();
        await page.SetContentAsync(html);
        var pdfStream = await page.PdfStreamAsync(new PdfOptions
        {
            Format = PaperFormat.A6,
            MarginOptions = new MarginOptions
            {
                Bottom = "0.5cm",
                Top = "0.5cm",
                Left = "0.5cm",
                Right = "0.5cm"
            },
            PrintBackground = true
        });
        await browser.CloseAsync();
        var memoryStream = new MemoryStream();
        await pdfStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }
}