using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiftApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedQuantityIntoDeliveryInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "VoucherDeliveryInfos",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "VoucherDeliveryInfos");
        }
    }
}
