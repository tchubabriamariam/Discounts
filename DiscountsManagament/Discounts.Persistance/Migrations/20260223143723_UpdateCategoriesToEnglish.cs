using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Discounts.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCategoriesToEnglish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Food and Drinks", "Restaurants" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Travel and Hotels", "Tourism" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Salons and Spa", "Beauty" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Gyms and Training", "Sports" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Cinema, Theater, Events", "Entertainment" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Clinics and Pharmacy", "Healthcare" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Courses and Trainings", "Education" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Car Services", "Automotive" });

            migrationBuilder.UpdateData(
                table: "Offers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CategoryId", "Description", "ImageUrl", "Title" },
                values: new object[] { 2, "Take a leap through the wormhole. Includes a gravity-defying stay at our orbital lounge. Tars and Case robots included for navigation.", "https://www.oberlin.edu/sites/default/files/styles/width_760/public/content/news/image/wormhole.1_0.png?itok=ARTDejwP", "Miller’s Planet Escape: 1 Hour Here, 7 Years Savings" });

            migrationBuilder.UpdateData(
                table: "Offers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CategoryId", "Description", "ImageUrl", "Title" },
                values: new object[] { 7, "Master the non-linear orthography of the 'Abbott and Costello' dialect. Guaranteed to change your perception of time.", "https://cdn.prod.website-files.com/639281d335ff5f86b30762e7/66fc20b9f3b199dfac4d4fec_66dc6b9f567f3167369a70a4_AD_4nXfU8j7VSRdch-FEQ5oA6pNRZ3xETFikaUUFCmcSD9osvn8X-lk5i5CZvrKTzE0zjglWsBRV77M3n8Thwp6cuhCHI9RyuyZaTwnvYL_y283XwUx0wAI_mzKBNTrMj86nmYwuXLh9zYHjcIO9K9PyFzRS4Ms.png", "Heptapod Linguistics: Learn to See the Future" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "b1c2d3e4-0001-0000-0000-000000000001",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4c697ba6-73f9-465a-9206-192986d205f6", "AQAAAAIAAYagAAAAEOsQ/XsxASskmviqdfL6qohyhATv5OEVQg80emSSEqbkZ4eJGn0PF0lrTncrFvICMA==", "9505237d-19f5-496d-8b09-f795ebf454ef" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "b1c2d3e4-0002-0000-0000-000000000001",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "327d8143-263e-47de-8e2a-644b499344d4", "AQAAAAIAAYagAAAAEBUSkqK0/Ol2Rx8QLWxVBr+zVEwK6yG2RfX/0dzA+cAJVzHqLi5DvtcXYsS4EtnlmQ==", "8e3fcb49-c6b2-4e53-9157-8d98f308894b" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "b1c2d3e4-0003-0000-0000-000000000001",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "bf6a8d53-bfd2-4c15-8767-dd9446f751a9", "AQAAAAIAAYagAAAAEEuxK/yg3vLfhhnp2P87J9kKKO+B1cA1m54jS+Zjl0ePzqTYyhUjtpcw8UDxB/QKyQ==", "d970cd1c-b505-49fd-b129-a670d3a028d0" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name" },
                values: new object[] { "საკვები და სასმელი", "რესტორნები" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name" },
                values: new object[] { "მოგზაურობა და სასტუმრო", "ტურიზმი" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name" },
                values: new object[] { "სალონები და სპა", "სილამაზე" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Name" },
                values: new object[] { "სპორტული კლუბები და ტრენინგები", "სპორტი" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Name" },
                values: new object[] { "კინო, თეატრი, ღონისძიებები", "გართობა" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "Name" },
                values: new object[] { "კლინიკები და ფარმაცია", "ჯანდაცვა" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Description", "Name" },
                values: new object[] { "კურსები და ტრენინგები", "განათლება" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Description", "Name" },
                values: new object[] { "ავტო სერვისები", "ავტომობილი" });

            migrationBuilder.UpdateData(
                table: "Offers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CategoryId", "Description", "ImageUrl", "Title" },
                values: new object[] { 1, "ორი ადამიანისთვის სრული სადილი: წვნიანი, მთავარი კერძი და დესერტი.", null, "2-სეტიანი სადილი 50% ფასდაკლებით" });

            migrationBuilder.UpdateData(
                table: "Offers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CategoryId", "Description", "ImageUrl", "Title" },
                values: new object[] { 3, "სრული სხეულის მასაჟი და სახის მოვლა 2 საათის განმავლობაში.", null, "სპა-პაკეტი 30% ფასდაკლებით" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "b1c2d3e4-0001-0000-0000-000000000001",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c2e29149-a598-4e8e-aeb4-0bd22d70e48e", "AQAAAAIAAYagAAAAEJio6aiQ9Q6cNDETLcwFdrl74pp2dzjQrKCgD96tRX2F1DbY5K8iHJnlVdqezYTbpg==", "a17b2630-64b5-4dc8-94cc-9c2d6bf3b8cb" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "b1c2d3e4-0002-0000-0000-000000000001",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "609a43a5-03e0-426c-8cc8-dc7fd8a1345f", "AQAAAAIAAYagAAAAEEsxEUSpeisVGe5fIS1oAFPTs1qtbRwmy5Y3s1M8XH5oTzeFryknKDyECv2KmpaW0g==", "fb652bc6-c236-4f57-af3e-17eedd06e5cc" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "b1c2d3e4-0003-0000-0000-000000000001",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "df68bfe3-352e-449b-a8f5-ab55d64903d1", "AQAAAAIAAYagAAAAEB0qsewkp/pKvjpPcc4nsJ73Sl/YPs4rhZjlLET/6xmBBwJjkFbq7Fq9/hw1ZqHiTA==", "33277f5b-883c-4798-b4fe-367a632036fd" });
        }
    }
}
