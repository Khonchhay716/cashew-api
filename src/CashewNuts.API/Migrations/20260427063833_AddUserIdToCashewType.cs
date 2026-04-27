using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CashewNuts.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToCashewType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CashewTypes");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "CashewTypes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sales_SaleDate",
                table: "Sales",
                column: "SaleDate");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_PurchaseDate",
                table: "Purchases",
                column: "PurchaseDate");

            migrationBuilder.CreateIndex(
                name: "IX_CashewTypes_UserId",
                table: "CashewTypes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CashewTypes_Users_UserId",
                table: "CashewTypes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CashewTypes_Users_UserId",
                table: "CashewTypes");

            migrationBuilder.DropIndex(
                name: "IX_Sales_SaleDate",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_PurchaseDate",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_CashewTypes_UserId",
                table: "CashewTypes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CashewTypes");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CashewTypes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
