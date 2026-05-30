using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemMagazynu.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryItems_Produces_ProductId",
                table: "DeliveryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Produces_Categories_CategoryId",
                table: "Produces");

            migrationBuilder.DropForeignKey(
                name: "FK_StockAlerts_Produces_ProductId",
                table: "StockAlerts");

            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseMovements_Produces_ProductId",
                table: "WarehouseMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseStocks_Produces_ProductId",
                table: "WarehouseStocks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Produces",
                table: "Produces");

            migrationBuilder.RenameTable(
                name: "Produces",
                newName: "Products");

            migrationBuilder.RenameIndex(
                name: "IX_Produces_CategoryId",
                table: "Products",
                newName: "IX_Products_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Produces_CatalogNumber",
                table: "Products",
                newName: "IX_Products_CatalogNumber");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryItems_Products_ProductId",
                table: "DeliveryItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockAlerts_Products_ProductId",
                table: "StockAlerts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseMovements_Products_ProductId",
                table: "WarehouseMovements",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseStocks_Products_ProductId",
                table: "WarehouseStocks",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryItems_Products_ProductId",
                table: "DeliveryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_StockAlerts_Products_ProductId",
                table: "StockAlerts");

            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseMovements_Products_ProductId",
                table: "WarehouseMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseStocks_Products_ProductId",
                table: "WarehouseStocks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Produces");

            migrationBuilder.RenameIndex(
                name: "IX_Products_CategoryId",
                table: "Produces",
                newName: "IX_Produces_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_CatalogNumber",
                table: "Produces",
                newName: "IX_Produces_CatalogNumber");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Produces",
                table: "Produces",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryItems_Produces_ProductId",
                table: "DeliveryItems",
                column: "ProductId",
                principalTable: "Produces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Produces_Categories_CategoryId",
                table: "Produces",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockAlerts_Produces_ProductId",
                table: "StockAlerts",
                column: "ProductId",
                principalTable: "Produces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseMovements_Produces_ProductId",
                table: "WarehouseMovements",
                column: "ProductId",
                principalTable: "Produces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseStocks_Produces_ProductId",
                table: "WarehouseStocks",
                column: "ProductId",
                principalTable: "Produces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
