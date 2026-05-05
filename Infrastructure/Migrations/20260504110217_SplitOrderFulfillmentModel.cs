using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SplitOrderFulfillmentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeliveryType",
                table: "Orders",
                newName: "FulfillmentScope");

            migrationBuilder.AddColumn<int>(
                name: "OutHouseFulfillmentType",
                table: "Orders",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OutHouseFulfillmentType",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "FulfillmentScope",
                table: "Orders",
                newName: "DeliveryType");
        }
    }
}
