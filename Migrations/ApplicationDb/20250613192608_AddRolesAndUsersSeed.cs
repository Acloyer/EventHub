using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EventHub.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class AddRolesAndUsersSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PlannedEvents",
                table: "PlannedEvents");

            // migrationBuilder.DropIndex(
            //     name: "IX_PlannedEvents_UserId_EventId",
            //     table: "PlannedEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FavoriteEvents",
                table: "FavoriteEvents");

            // migrationBuilder.DropIndex(
            //     name: "IX_FavoriteEvents_UserId_EventId",
            //     table: "FavoriteEvents");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PlannedEvents");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "FavoriteEvents");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlannedEvents",
                table: "PlannedEvents",
                columns: new[] { "UserId", "EventId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_FavoriteEvents",
                table: "FavoriteEvents",
                columns: new[] { "UserId", "EventId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PlannedEvents",
                table: "PlannedEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FavoriteEvents",
                table: "FavoriteEvents");

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlannedEvents",
                table: "PlannedEvents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FavoriteEvents",
                table: "FavoriteEvents",
                column: "Id");

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
    }
}
