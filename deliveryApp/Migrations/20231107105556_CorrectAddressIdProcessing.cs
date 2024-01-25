using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deliveryApp.Migrations
{
    /// <inheritdoc />
    public partial class CorrectAddressIdProcessing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AddresId",
                table: "Orders");

            migrationBuilder.AddColumn<Guid>(
                name: "AddressGuid",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AddresGuid",
                table: "Orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressGuid",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AddresGuid",
                table: "Orders");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddresId",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
