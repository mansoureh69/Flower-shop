using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SweetFlowerShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderItemSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "CartItems");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "OrderItems",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitPrice_Currency",
                table: "OrderItems",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SnapshotPrice_Amount",
                table: "CartItems",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SnapshotPrice_Currency",
                table: "CartItems",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductId",
                table: "CartItems",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_ProductId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "UnitPrice_Currency",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "SnapshotPrice_Amount",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "SnapshotPrice_Currency",
                table: "CartItems");

            migrationBuilder.AddColumn<int>(
                name: "Price",
                table: "CartItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
