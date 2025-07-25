using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BasketSurvay.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "6dc6528a-b280-4770-9eae-82671ee81ef7",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAECFdYRH0N9Q/1vWopOwqjWken0x7biV7Sa7slwy+LrjeWIn3UFAmPhkTk3N+HtX1AQ==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "6dc6528a-b280-4770-9eae-82671ee81ef7",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAECy23xfjCMGZkh5x+DU3n7vgTTS7pKU806Tn/qQcBw0dtUt6guT+NZfsQhIzKSR/uQ==");
        }
    }
}
