using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deliveryApp.Migrations
{
    /// <inheritdoc />
    public partial class DishInCartWithoutOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DishesInCart_Orders_OrderId",
                table: "DishesInCart");

            migrationBuilder.AlterColumn<Guid>(
                name: "OrderId",
                table: "DishesInCart",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "DishesInCart",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_DishesInCart_UserId",
                table: "DishesInCart",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DishesInCart_Orders_OrderId",
                table: "DishesInCart",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DishesInCart_Users_UserId",
                table: "DishesInCart",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DishesInCart_Orders_OrderId",
                table: "DishesInCart");

            migrationBuilder.DropForeignKey(
                name: "FK_DishesInCart_Users_UserId",
                table: "DishesInCart");

            migrationBuilder.DropIndex(
                name: "IX_DishesInCart_UserId",
                table: "DishesInCart");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "DishesInCart");

            migrationBuilder.AlterColumn<Guid>(
                name: "OrderId",
                table: "DishesInCart",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DishesInCart_Orders_OrderId",
                table: "DishesInCart",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
