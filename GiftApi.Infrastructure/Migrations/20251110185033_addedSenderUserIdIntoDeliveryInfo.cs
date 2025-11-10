using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiftApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addedSenderUserIdIntoDeliveryInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SenderId",
                table: "VoucherDeliveryInfos",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenderId",
                table: "VoucherDeliveryInfos");
        }
    }
}
