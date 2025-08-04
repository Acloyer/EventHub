using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtToEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "TelegramVerifications",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "ChatId",
                table: "TelegramVerifications",
                newName: "TelegramId");

            migrationBuilder.AddColumn<DateTime>(
                name: "MuteUntil",
                table: "UserMuteEntries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "TelegramVerifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Events",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "EventComments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MuteUntil",
                table: "UserMuteEntries");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "TelegramVerifications");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "EventComments");

            migrationBuilder.RenameColumn(
                name: "TelegramId",
                table: "TelegramVerifications",
                newName: "ChatId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "TelegramVerifications",
                newName: "ExpiresAt");
        }
    }
}
