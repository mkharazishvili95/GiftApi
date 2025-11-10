using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiftApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsUsedIntoDeliveryInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "VoucherDeliveryInfos",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "VoucherDeliveryInfos");
        }
    }
}
