using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class SplitOrderFulfillmentModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FulfillmentScope",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "OutHouseFulfillmentType",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE Orders
                SET FulfillmentScope = 1,
                    OutHouseFulfillmentType = DeliveryType
            ");

            migrationBuilder.DropColumn(
                name: "DeliveryType",
                table: "Orders");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliveryType",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE Orders
                SET DeliveryType = ISNULL(OutHouseFulfillmentType, 0)
            ");

            migrationBuilder.DropColumn(
                name: "FulfillmentScope",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OutHouseFulfillmentType",
                table: "Orders");
        }
    }
}
