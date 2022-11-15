using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoctorsOffice.Infrastructure.Migrations
{
    public partial class SeededDoctorHasQuickButtons : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "QuickButtons",
                columns: new[] { "Id", "CreatedAt", "DoctorId", "Type", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { new Guid("074dacb8-0264-4a3b-8627-1581cf14ec3a"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Vitamin A" },
                    { new Guid("0b6747a5-9eea-4484-a192-6ad927025b84"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Muscle pain" },
                    { new Guid("0eeb68fc-ccdf-4102-b6da-2f9dd939bc3e"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Vitamin B12" },
                    { new Guid("179dfb23-44f4-49ec-bf10-159ef4ca6954"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Chronic obstructive pulmonary disease" },
                    { new Guid("1deba9b9-0eeb-43d7-a95a-768a32a86f26"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Chest pain" },
                    { new Guid("1e310e93-070b-4202-8dc3-f514fce5a3b9"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Antihistamines" },
                    { new Guid("2027a099-57ce-4fbe-9d38-1c7c337a8e50"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Mucolytics" },
                    { new Guid("2c9c5afa-9331-4d23-87da-ac59217ac1fb"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Fatigue" },
                    { new Guid("2ea7fa99-42c7-4b75-a673-2435a2c50584"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "COVID-19" },
                    { new Guid("30101274-f6d5-4b7e-9d06-b9e816c801b5"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Loss of taste" },
                    { new Guid("32368078-8d03-4ad2-8bc1-d683a94e82ed"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Bronchitis" },
                    { new Guid("3652b57a-07e4-4915-b22a-581e76f58448"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Corticosteroids" },
                    { new Guid("41589b18-4df9-4f02-a101-af827964b3e3"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Antibiotics" },
                    { new Guid("47a50b1f-e874-4964-831b-d677941e9ecf"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Vitamin E" },
                    { new Guid("48ff8e51-0164-4194-90c5-9e0dd87b864d"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Acute tonsillitis" },
                    { new Guid("4a87b3bd-5877-4388-9239-69f4e83bc322"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Antipyretics" },
                    { new Guid("521affee-54ee-48a5-ba36-0217e0bbb5d0"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Nausea" },
                    { new Guid("53e76e9d-6093-41f3-8de9-8d6c33434f71"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Fluids" },
                    { new Guid("57581436-4e1b-4856-bc53-d52c24c2fd65"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Bronchodilators" },
                    { new Guid("5aa836ee-00c1-4f7b-8c5e-b74d7162fe3c"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Antiviral drugs" },
                    { new Guid("5da4cdd1-9923-4cc9-aa08-eaf0e4003e0b"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Antidiarrheals" },
                    { new Guid("60898d97-32e0-4841-bec0-6ffd3f15cb9c"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Acute bronchiti" },
                    { new Guid("650c86c0-c74e-4101-9777-4598873e1050"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Antiparasitic drugs" },
                    { new Guid("6f72df4d-d279-4fc4-bd22-67d3ba33cadc"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Acute pharyngitis" },
                    { new Guid("7026dc95-3957-4779-bdc3-59294b8d8faa"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Shortness of breath" },
                    { new Guid("7582bbb7-9ecb-47d5-a742-891b514d60de"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Antacids" },
                    { new Guid("75c22761-f498-4973-86fb-cf1b13dd729e"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Acute bronchitis" },
                    { new Guid("7a46084f-288c-4bbe-8da1-5ea97ca48ea6"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Expectorants" },
                    { new Guid("7d80a7b1-599d-457f-b8a3-7f963f0fed98"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Rest" },
                    { new Guid("7ff91ada-1ec7-46e4-b2d6-435e5cbbf38d"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Antitussives" },
                    { new Guid("814e13d2-0e02-4380-bc7f-692280ea68e2"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Sore throat" },
                    { new Guid("8a9ee513-9ff5-4b40-bb09-59730a829c77"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Tuberculosis" },
                    { new Guid("8ea7023f-f79b-4dc7-965a-890cdd68ba9c"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Vitamin C" },
                    { new Guid("9390cf8d-f52a-4527-9f17-d785d281637e"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Decongestants" },
                    { new Guid("963dc7c0-fb0e-44b8-bf59-7048d241f807"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Confusion" },
                    { new Guid("9bb77950-3379-4c77-9213-b43950976939"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Antifungal drugs" },
                    { new Guid("9d7dd8b1-9cd9-4673-8900-6240a27d1847"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Vomiting" },
                    { new Guid("9dd0b024-e5dd-4169-bd91-391d6fbb615c"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Influenza" },
                    { new Guid("a2c32b19-cd8f-495c-a11e-1c34eec818fe"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Vitamin D" },
                    { new Guid("a3c3d331-21c9-48eb-aace-ddffcd3d9331"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Antiemetics" },
                    { new Guid("a86c35ee-f3a0-4c18-8494-3e9c201dfcf0"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Acute sinusitis" },
                    { new Guid("aca1f0bc-25f6-463f-aa93-45cb64cc5d7c"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Diarrhea" },
                    { new Guid("b0437003-e8c6-4cd5-858e-bc9cfe12357f"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Headache" },
                    { new Guid("c10ae801-7042-485f-a528-539503c568b7"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Antispasmodics" },
                    { new Guid("cc53e419-d5c6-4131-b77a-4040e279c83b"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Fever" },
                    { new Guid("cf407c2a-cf8a-404a-b07a-91f871e4a8cc"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Pneumonia" },
                    { new Guid("d548b5e1-6f9b-44ec-bc33-0ed7f3f0e460"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Loss of smell" },
                    { new Guid("dcb02f21-0ed8-4c97-8f2f-2b58a42c6de3"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Vitamin K" },
                    { new Guid("df3f8ee7-a5e2-4c2a-a2f8-e598b560947d"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Interview", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Cough" },
                    { new Guid("e015b806-ea24-4741-853c-2628d1393ad7"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Vitamin B6" },
                    { new Guid("e3a88a80-5c6d-4060-a8d7-7af07db8e3e4"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Recommendations", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Analgesics" },
                    { new Guid("e53c4fab-e86a-4c03-9ad4-431846ff8467"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Acute otitis media" },
                    { new Guid("e6d6e719-f349-417a-83e5-f9ef4c41543f"), new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c8934fff-2f5a-4198-893f-26023d8f4107"), "Diagnosis", new DateTime(2022, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Common cold" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("074dacb8-0264-4a3b-8627-1581cf14ec3a"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("0b6747a5-9eea-4484-a192-6ad927025b84"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("0eeb68fc-ccdf-4102-b6da-2f9dd939bc3e"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("179dfb23-44f4-49ec-bf10-159ef4ca6954"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("1deba9b9-0eeb-43d7-a95a-768a32a86f26"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("1e310e93-070b-4202-8dc3-f514fce5a3b9"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("2027a099-57ce-4fbe-9d38-1c7c337a8e50"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("2c9c5afa-9331-4d23-87da-ac59217ac1fb"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("2ea7fa99-42c7-4b75-a673-2435a2c50584"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("30101274-f6d5-4b7e-9d06-b9e816c801b5"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("32368078-8d03-4ad2-8bc1-d683a94e82ed"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("3652b57a-07e4-4915-b22a-581e76f58448"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("41589b18-4df9-4f02-a101-af827964b3e3"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("47a50b1f-e874-4964-831b-d677941e9ecf"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("48ff8e51-0164-4194-90c5-9e0dd87b864d"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("4a87b3bd-5877-4388-9239-69f4e83bc322"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("521affee-54ee-48a5-ba36-0217e0bbb5d0"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("53e76e9d-6093-41f3-8de9-8d6c33434f71"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("57581436-4e1b-4856-bc53-d52c24c2fd65"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("5aa836ee-00c1-4f7b-8c5e-b74d7162fe3c"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("5da4cdd1-9923-4cc9-aa08-eaf0e4003e0b"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("60898d97-32e0-4841-bec0-6ffd3f15cb9c"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("650c86c0-c74e-4101-9777-4598873e1050"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("6f72df4d-d279-4fc4-bd22-67d3ba33cadc"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("7026dc95-3957-4779-bdc3-59294b8d8faa"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("7582bbb7-9ecb-47d5-a742-891b514d60de"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("75c22761-f498-4973-86fb-cf1b13dd729e"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("7a46084f-288c-4bbe-8da1-5ea97ca48ea6"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("7d80a7b1-599d-457f-b8a3-7f963f0fed98"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("7ff91ada-1ec7-46e4-b2d6-435e5cbbf38d"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("814e13d2-0e02-4380-bc7f-692280ea68e2"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("8a9ee513-9ff5-4b40-bb09-59730a829c77"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("8ea7023f-f79b-4dc7-965a-890cdd68ba9c"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("9390cf8d-f52a-4527-9f17-d785d281637e"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("963dc7c0-fb0e-44b8-bf59-7048d241f807"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("9bb77950-3379-4c77-9213-b43950976939"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("9d7dd8b1-9cd9-4673-8900-6240a27d1847"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("9dd0b024-e5dd-4169-bd91-391d6fbb615c"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("a2c32b19-cd8f-495c-a11e-1c34eec818fe"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("a3c3d331-21c9-48eb-aace-ddffcd3d9331"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("a86c35ee-f3a0-4c18-8494-3e9c201dfcf0"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("aca1f0bc-25f6-463f-aa93-45cb64cc5d7c"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("b0437003-e8c6-4cd5-858e-bc9cfe12357f"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("c10ae801-7042-485f-a528-539503c568b7"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("cc53e419-d5c6-4131-b77a-4040e279c83b"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("cf407c2a-cf8a-404a-b07a-91f871e4a8cc"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("d548b5e1-6f9b-44ec-bc33-0ed7f3f0e460"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("dcb02f21-0ed8-4c97-8f2f-2b58a42c6de3"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("df3f8ee7-a5e2-4c2a-a2f8-e598b560947d"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("e015b806-ea24-4741-853c-2628d1393ad7"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("e3a88a80-5c6d-4060-a8d7-7af07db8e3e4"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("e53c4fab-e86a-4c03-9ad4-431846ff8467"));

            migrationBuilder.DeleteData(
                table: "QuickButtons",
                keyColumn: "Id",
                keyValue: new Guid("e6d6e719-f349-417a-83e5-f9ef4c41543f"));
        }
    }
}
