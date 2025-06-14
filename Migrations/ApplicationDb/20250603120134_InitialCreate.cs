using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EventHub.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlannedEvents",
                table: "PlannedEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FavoriteEvents",
                table: "FavoriteEvents");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserRoles",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "PlannedEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "FavoriteEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "MaxParticipants",
                table: "Events",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Events",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlannedEvents",
                table: "PlannedEvents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FavoriteEvents",
                table: "FavoriteEvents",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlannedEvents_UserId_EventId",
                table: "PlannedEvents",
                columns: new[] { "UserId", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteEvents_UserId_EventId",
                table: "FavoriteEvents",
                columns: new[] { "UserId", "EventId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlannedEvents",
                table: "PlannedEvents");

            migrationBuilder.DropIndex(
                name: "IX_PlannedEvents_UserId_EventId",
                table: "PlannedEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FavoriteEvents",
                table: "FavoriteEvents");

            migrationBuilder.DropIndex(
                name: "IX_FavoriteEvents_UserId_EventId",
                table: "FavoriteEvents");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PlannedEvents");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "FavoriteEvents");

            migrationBuilder.AlterColumn<int>(
                name: "MaxParticipants",
                table: "Events",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Events",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlannedEvents",
                table: "PlannedEvents",
                columns: new[] { "UserId", "EventId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_FavoriteEvents",
                table: "FavoriteEvents",
                columns: new[] { "UserId", "EventId" });
        }
    }
}
