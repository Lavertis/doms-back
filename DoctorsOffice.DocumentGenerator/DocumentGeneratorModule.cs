using DoctorsOffice.DocumentGenerator.DocumentGenerator;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorsOffice.DocumentGenerator;

public static class DocumentGeneratorModule
{
    public static void AddDocumentGenerator(this IServiceCollection services)
    {
        services.AddSingleton<IDocumentGenerator, DocumentGenerator.DocumentGenerator>();
    }
}