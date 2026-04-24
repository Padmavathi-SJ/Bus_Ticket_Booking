using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Notifications");

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedBookingId",
                table: "Notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RelatedBookingId",
                table: "Notifications",
                column: "RelatedBookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Bookings_RelatedBookingId",
                table: "Notifications",
                column: "RelatedBookingId",
                principalTable: "Bookings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Bookings_RelatedBookingId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_RelatedBookingId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RelatedBookingId",
                table: "Notifications");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
