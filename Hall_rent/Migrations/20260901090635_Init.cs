using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hall_rent.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HallId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    From = table.Column<DateTime>(type: "datetime2", nullable: false),
                    To = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Favors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Halls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Persons = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Halls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HallBookingFavors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HallBookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FavorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriceAtBooking = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HallBookingFavors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HallBookingFavors_Bookings_HallBookingId",
                        column: x => x.HallBookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HallBookingFavors_Favors_FavorId",
                        column: x => x.FavorId,
                        principalTable: "Favors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HallFavorLinks",
                columns: table => new
                {
                    HallId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FavorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HallFavorLinks", x => new { x.HallId, x.FavorId });
                    table.ForeignKey(
                        name: "FK_HallFavorLinks_Favors_FavorId",
                        column: x => x.FavorId,
                        principalTable: "Favors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HallFavorLinks_Halls_HallId",
                        column: x => x.HallId,
                        principalTable: "Halls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HallBookingFavors_FavorId",
                table: "HallBookingFavors",
                column: "FavorId");

            migrationBuilder.CreateIndex(
                name: "IX_HallBookingFavors_HallBookingId_FavorId",
                table: "HallBookingFavors",
                columns: new[] { "HallBookingId", "FavorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HallFavorLinks_FavorId",
                table: "HallFavorLinks",
                column: "FavorId");

            migrationBuilder.CreateIndex(
                name: "IX_Halls_Name",
                table: "Halls",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HallBookingFavors");

            migrationBuilder.DropTable(
                name: "HallFavorLinks");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Favors");

            migrationBuilder.DropTable(
                name: "Halls");
        }
    }
}
