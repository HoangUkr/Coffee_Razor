using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixItemImagesRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemImages_Items_ItemId1",
                table: "ItemImages");

            migrationBuilder.DropIndex(
                name: "IX_ItemImages_ItemId1",
                table: "ItemImages");

            migrationBuilder.DropColumn(
                name: "ItemId1",
                table: "ItemImages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ItemId1",
                table: "ItemImages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemImages_ItemId1",
                table: "ItemImages",
                column: "ItemId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemImages_Items_ItemId1",
                table: "ItemImages",
                column: "ItemId1",
                principalTable: "Items",
                principalColumn: "Id");
        }
    }
}
