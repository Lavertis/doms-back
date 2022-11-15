using DoctorsOffice.Domain.Entities;
using DoctorsOffice.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DoctorsOffice.Infrastructure.Database.Seeders;

public static class QuickButtonSeeder
{
    public static void SeedQuickButtons(ModelBuilder builder)
    {
        SeedQuickButtonsForDoctor(builder, Guid.Parse(UserSeeder.DoctorUserId));
    }

    private static void SeedQuickButtonsForDoctor(ModelBuilder builder, Guid doctorId)
    {
        var interviewQuickButtons = new List<QuickButton>
        {
            new()
            {
                Id = Guid.Parse("df3f8ee7-a5e2-4c2a-a2f8-e598b560947d"), Type = QuickButtonTypes.Interview,
                Value = "Cough"
            },
            new()
            {
                Id = Guid.Parse("cc53e419-d5c6-4131-b77a-4040e279c83b"), Type = QuickButtonTypes.Interview,
                Value = "Fever"
            },
            new()
            {
                Id = Guid.Parse("b0437003-e8c6-4cd5-858e-bc9cfe12357f"), Type = QuickButtonTypes.Interview,
                Value = "Headache"
            },
            new()
            {
                Id = Guid.Parse("814e13d2-0e02-4380-bc7f-692280ea68e2"), Type = QuickButtonTypes.Interview,
                Value = "Sore throat"
            },
            new()
            {
                Id = Guid.Parse("2c9c5afa-9331-4d23-87da-ac59217ac1fb"), Type = QuickButtonTypes.Interview,
                Value = "Fatigue"
            },
            new()
            {
                Id = Guid.Parse("d548b5e1-6f9b-44ec-bc33-0ed7f3f0e460"), Type = QuickButtonTypes.Interview,
                Value = "Loss of smell"
            },
            new()
            {
                Id = Guid.Parse("30101274-f6d5-4b7e-9d06-b9e816c801b5"), Type = QuickButtonTypes.Interview,
                Value = "Loss of taste"
            },
            new()
            {
                Id = Guid.Parse("aca1f0bc-25f6-463f-aa93-45cb64cc5d7c"), Type = QuickButtonTypes.Interview,
                Value = "Diarrhea"
            },
            new()
            {
                Id = Guid.Parse("521affee-54ee-48a5-ba36-0217e0bbb5d0"), Type = QuickButtonTypes.Interview,
                Value = "Nausea"
            },
            new()
            {
                Id = Guid.Parse("9d7dd8b1-9cd9-4673-8900-6240a27d1847"), Type = QuickButtonTypes.Interview,
                Value = "Vomiting"
            },
            new()
            {
                Id = Guid.Parse("7026dc95-3957-4779-bdc3-59294b8d8faa"), Type = QuickButtonTypes.Interview,
                Value = "Shortness of breath"
            },
            new()
            {
                Id = Guid.Parse("1deba9b9-0eeb-43d7-a95a-768a32a86f26"), Type = QuickButtonTypes.Interview,
                Value = "Chest pain"
            },
            new()
            {
                Id = Guid.Parse("0b6747a5-9eea-4484-a192-6ad927025b84"), Type = QuickButtonTypes.Interview,
                Value = "Muscle pain"
            },
            new()
            {
                Id = Guid.Parse("963dc7c0-fb0e-44b8-bf59-7048d241f807"), Type = QuickButtonTypes.Interview,
                Value = "Confusion"
            }
        };

        var diagnosisQuickButtons = new List<QuickButton>
        {
            new()
            {
                Id = Guid.Parse("2ea7fa99-42c7-4b75-a673-2435a2c50584"), Type = QuickButtonTypes.Diagnosis,
                Value = "COVID-19"
            },
            new()
            {
                Id = Guid.Parse("9dd0b024-e5dd-4169-bd91-391d6fbb615c"), Type = QuickButtonTypes.Diagnosis,
                Value = "Influenza"
            },
            new()
            {
                Id = Guid.Parse("e6d6e719-f349-417a-83e5-f9ef4c41543f"), Type = QuickButtonTypes.Diagnosis,
                Value = "Common cold"
            },
            new()
            {
                Id = Guid.Parse("cf407c2a-cf8a-404a-b07a-91f871e4a8cc"), Type = QuickButtonTypes.Diagnosis,
                Value = "Pneumonia"
            },
            new()
            {
                Id = Guid.Parse("32368078-8d03-4ad2-8bc1-d683a94e82ed"), Type = QuickButtonTypes.Diagnosis,
                Value = "Bronchitis"
            },
            new()
            {
                Id = Guid.Parse("8a9ee513-9ff5-4b40-bb09-59730a829c77"), Type = QuickButtonTypes.Diagnosis,
                Value = "Tuberculosis"
            },
            new()
            {
                Id = Guid.Parse("179dfb23-44f4-49ec-bf10-159ef4ca6954"), Type = QuickButtonTypes.Diagnosis,
                Value = "Chronic obstructive pulmonary disease"
            },
            new()
            {
                Id = Guid.Parse("75c22761-f498-4973-86fb-cf1b13dd729e"), Type = QuickButtonTypes.Diagnosis,
                Value = "Acute bronchitis"
            },
            new()
            {
                Id = Guid.Parse("a86c35ee-f3a0-4c18-8494-3e9c201dfcf0"), Type = QuickButtonTypes.Diagnosis,
                Value = "Acute sinusitis"
            },
            new()
            {
                Id = Guid.Parse("6f72df4d-d279-4fc4-bd22-67d3ba33cadc"), Type = QuickButtonTypes.Diagnosis,
                Value = "Acute pharyngitis"
            },
            new()
            {
                Id = Guid.Parse("48ff8e51-0164-4194-90c5-9e0dd87b864d"), Type = QuickButtonTypes.Diagnosis,
                Value = "Acute tonsillitis"
            },
            new()
            {
                Id = Guid.Parse("e53c4fab-e86a-4c03-9ad4-431846ff8467"), Type = QuickButtonTypes.Diagnosis,
                Value = "Acute otitis media"
            },
            new()
            {
                Id = Guid.Parse("60898d97-32e0-4841-bec0-6ffd3f15cb9c"), Type = QuickButtonTypes.Diagnosis,
                Value = "Acute bronchiti"
            }
        };

        var recommendationsQuickButtons = new List<QuickButton>
        {
            new()
            {
                Id = Guid.Parse("41589b18-4df9-4f02-a101-af827964b3e3"), Type = QuickButtonTypes.Recommendations,
                Value = "Antibiotics"
            },
            new()
            {
                Id = Guid.Parse("5aa836ee-00c1-4f7b-8c5e-b74d7162fe3c"), Type = QuickButtonTypes.Recommendations,
                Value = "Antiviral drugs"
            },
            new()
            {
                Id = Guid.Parse("9bb77950-3379-4c77-9213-b43950976939"), Type = QuickButtonTypes.Recommendations,
                Value = "Antifungal drugs"
            },
            new()
            {
                Id = Guid.Parse("650c86c0-c74e-4101-9777-4598873e1050"), Type = QuickButtonTypes.Recommendations,
                Value = "Antiparasitic drugs"
            },
            new()
            {
                Id = Guid.Parse("1e310e93-070b-4202-8dc3-f514fce5a3b9"), Type = QuickButtonTypes.Recommendations,
                Value = "Antihistamines"
            },
            new()
            {
                Id = Guid.Parse("3652b57a-07e4-4915-b22a-581e76f58448"), Type = QuickButtonTypes.Recommendations,
                Value = "Corticosteroids"
            },
            new()
            {
                Id = Guid.Parse("57581436-4e1b-4856-bc53-d52c24c2fd65"), Type = QuickButtonTypes.Recommendations,
                Value = "Bronchodilators"
            },
            new()
            {
                Id = Guid.Parse("9390cf8d-f52a-4527-9f17-d785d281637e"), Type = QuickButtonTypes.Recommendations,
                Value = "Decongestants"
            },
            new()
            {
                Id = Guid.Parse("7a46084f-288c-4bbe-8da1-5ea97ca48ea6"), Type = QuickButtonTypes.Recommendations,
                Value = "Expectorants"
            },
            new()
            {
                Id = Guid.Parse("2027a099-57ce-4fbe-9d38-1c7c337a8e50"), Type = QuickButtonTypes.Recommendations,
                Value = "Mucolytics"
            },
            new()
            {
                Id = Guid.Parse("7ff91ada-1ec7-46e4-b2d6-435e5cbbf38d"), Type = QuickButtonTypes.Recommendations,
                Value = "Antitussives"
            },
            new()
            {
                Id = Guid.Parse("a3c3d331-21c9-48eb-aace-ddffcd3d9331"), Type = QuickButtonTypes.Recommendations,
                Value = "Antiemetics"
            },
            new()
            {
                Id = Guid.Parse("4a87b3bd-5877-4388-9239-69f4e83bc322"), Type = QuickButtonTypes.Recommendations,
                Value = "Antipyretics"
            },
            new()
            {
                Id = Guid.Parse("e3a88a80-5c6d-4060-a8d7-7af07db8e3e4"), Type = QuickButtonTypes.Recommendations,
                Value = "Analgesics"
            },
            new()
            {
                Id = Guid.Parse("7582bbb7-9ecb-47d5-a742-891b514d60de"), Type = QuickButtonTypes.Recommendations,
                Value = "Antacids"
            },
            new()
            {
                Id = Guid.Parse("c10ae801-7042-485f-a528-539503c568b7"), Type = QuickButtonTypes.Recommendations,
                Value = "Antispasmodics"
            },
            new()
            {
                Id = Guid.Parse("5da4cdd1-9923-4cc9-aa08-eaf0e4003e0b"), Type = QuickButtonTypes.Recommendations,
                Value = "Antidiarrheals"
            },
            new()
            {
                Id = Guid.Parse("7d80a7b1-599d-457f-b8a3-7f963f0fed98"), Type = QuickButtonTypes.Recommendations,
                Value = "Rest"
            },
            new()
            {
                Id = Guid.Parse("53e76e9d-6093-41f3-8de9-8d6c33434f71"), Type = QuickButtonTypes.Recommendations,
                Value = "Fluids"
            },
            new()
            {
                Id = Guid.Parse("8ea7023f-f79b-4dc7-965a-890cdd68ba9c"), Type = QuickButtonTypes.Recommendations,
                Value = "Vitamin C"
            },
            new()
            {
                Id = Guid.Parse("a2c32b19-cd8f-495c-a11e-1c34eec818fe"), Type = QuickButtonTypes.Recommendations,
                Value = "Vitamin D"
            },
            new()
            {
                Id = Guid.Parse("47a50b1f-e874-4964-831b-d677941e9ecf"), Type = QuickButtonTypes.Recommendations,
                Value = "Vitamin E"
            },
            new()
            {
                Id = Guid.Parse("e015b806-ea24-4741-853c-2628d1393ad7"), Type = QuickButtonTypes.Recommendations,
                Value = "Vitamin B6"
            },
            new()
            {
                Id = Guid.Parse("0eeb68fc-ccdf-4102-b6da-2f9dd939bc3e"), Type = QuickButtonTypes.Recommendations,
                Value = "Vitamin B12"
            },
            new()
            {
                Id = Guid.Parse("dcb02f21-0ed8-4c97-8f2f-2b58a42c6de3"), Type = QuickButtonTypes.Recommendations,
                Value = "Vitamin K"
            },
            new()
            {
                Id = Guid.Parse("074dacb8-0264-4a3b-8627-1581cf14ec3a"), Type = QuickButtonTypes.Recommendations,
                Value = "Vitamin A"
            }
        };

        var doctorQuickButtons = interviewQuickButtons
            .Concat(diagnosisQuickButtons)
            .Concat(recommendationsQuickButtons)
            .ToList();

        foreach (var quickButton in doctorQuickButtons)
        {
            quickButton.DoctorId = doctorId;
            quickButton.CreatedAt = quickButton.UpdatedAt = DatabaseSeeder.TimeStamp;
        }

        builder.Entity<QuickButton>().HasData(doctorQuickButtons);
    }
}