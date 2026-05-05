using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkingSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkingSchedules",
                columns: table => new
                {
                    Day = table.Column<int>(type: "int", nullable: false),
                    OpenTime = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    CloseTime = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkingSchedules", x => x.Day);
                });

            migrationBuilder.InsertData(
                table: "WorkingSchedules",
                columns: new[] { "Day", "CloseTime", "OpenTime" },
                values: new object[,]
                {
                    { 0, "20:00", "09:00" },
                    { 1, "22:00", "08:00" },
                    { 2, "22:00", "08:00" },
                    { 3, "22:00", "08:00" },
                    { 4, "22:00", "08:00" },
                    { 5, "22:00", "08:00" },
                    { 6, "21:00", "09:00" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkingSchedules");
        }
    }
}
