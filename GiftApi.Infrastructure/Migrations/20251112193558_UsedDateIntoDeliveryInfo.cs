using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiftApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UsedDateIntoDeliveryInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UsedDate",
                table: "VoucherDeliveryInfos",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsedDate",
                table: "VoucherDeliveryInfos");
        }
    }
}
