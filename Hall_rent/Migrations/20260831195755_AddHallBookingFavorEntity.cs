using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hall_rent.Migrations
{
    /// <inheritdoc />
    public partial class AddHallBookingFavorEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Favors",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "StartAt",
                table: "Bookings",
                newName: "To");

            migrationBuilder.RenameColumn(
                name: "EndAt",
                table: "Bookings",
                newName: "From");

            migrationBuilder.CreateTable(
                name: "HallBookingFavorEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HallBookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FavorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriceAtBooking = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HallBookingFavorEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HallBookingFavorEntity_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HallBookingFavorEntity_Favors_FavorId",
                        column: x => x.FavorId,
                        principalTable: "Favors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HallBookingFavorEntity_BookingId",
                table: "HallBookingFavorEntity",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_HallBookingFavorEntity_FavorId",
                table: "HallBookingFavorEntity",
                column: "FavorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HallBookingFavorEntity");

            migrationBuilder.RenameColumn(
                name: "To",
                table: "Bookings",
                newName: "StartAt");

            migrationBuilder.RenameColumn(
                name: "From",
                table: "Bookings",
                newName: "EndAt");

            migrationBuilder.AddColumn<string>(
                name: "Favors",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
