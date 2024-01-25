using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deliveryApp.Migrations
{
    /// <inheritdoc />
    public partial class CorrectDishInCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DishesInCart_Orders_OrderEntityId",
                table: "DishesInCart");

            migrationBuilder.DropForeignKey(
                name: "FK_DishesInCart_Users_UserId",
                table: "DishesInCart");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_HouseEntity_AddressId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "HierarchyEntity");

            migrationBuilder.DropTable(
                name: "AddressElementEntity");

            migrationBuilder.DropTable(
                name: "HouseEntity");

            migrationBuilder.DropIndex(
                name: "IX_Orders_AddressId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_DishesInCart_OrderEntityId",
                table: "DishesInCart");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderEntityId",
                table: "DishesInCart");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "DishesInCart",
                newName: "OrderId");

            migrationBuilder.RenameColumn(
                name: "Count",
                table: "DishesInCart",
                newName: "Price");

            migrationBuilder.RenameIndex(
                name: "IX_DishesInCart_UserId",
                table: "DishesInCart",
                newName: "IX_DishesInCart_OrderId");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Amount",
                table: "DishesInCart",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_DishesInCart_Orders_OrderId",
                table: "DishesInCart",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DishesInCart_Orders_OrderId",
                table: "DishesInCart");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "DishesInCart");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "DishesInCart",
                newName: "Count");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "DishesInCart",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_DishesInCart_OrderId",
                table: "DishesInCart",
                newName: "IX_DishesInCart_UserId");

            migrationBuilder.AddColumn<Guid>(
                name: "AddressId",
                table: "Orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrderEntityId",
                table: "DishesInCart",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AddressElementEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Level = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ObjectGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectId = table.Column<int>(type: "integer", nullable: false),
                    TypeName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressElementEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HouseEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Addnum1 = table.Column<int>(type: "integer", nullable: false),
                    Addnum2 = table.Column<int>(type: "integer", nullable: false),
                    Addtype1 = table.Column<int>(type: "integer", nullable: false),
                    Addtype2 = table.Column<int>(type: "integer", nullable: false),
                    Housenum = table.Column<int>(type: "integer", nullable: false),
                    Isactive = table.Column<bool>(type: "boolean", nullable: false),
                    ObjectId = table.Column<int>(type: "integer", nullable: false),
                    Objectguid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HouseEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HierarchyEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AddressElementId = table.Column<Guid>(type: "uuid", nullable: true),
                    HouseId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ObjectId = table.Column<int>(type: "integer", nullable: false),
                    ParentObjId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HierarchyEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HierarchyEntity_AddressElementEntity_AddressElementId",
                        column: x => x.AddressElementId,
                        principalTable: "AddressElementEntity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HierarchyEntity_HouseEntity_HouseId",
                        column: x => x.HouseId,
                        principalTable: "HouseEntity",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_AddressId",
                table: "Orders",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_DishesInCart_OrderEntityId",
                table: "DishesInCart",
                column: "OrderEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_HierarchyEntity_AddressElementId",
                table: "HierarchyEntity",
                column: "AddressElementId");

            migrationBuilder.CreateIndex(
                name: "IX_HierarchyEntity_HouseId",
                table: "HierarchyEntity",
                column: "HouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_DishesInCart_Orders_OrderEntityId",
                table: "DishesInCart",
                column: "OrderEntityId",
                principalTable: "Orders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DishesInCart_Users_UserId",
                table: "DishesInCart",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_HouseEntity_AddressId",
                table: "Orders",
                column: "AddressId",
                principalTable: "HouseEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
