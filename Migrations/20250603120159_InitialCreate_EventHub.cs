using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EventHub.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_EventHub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PlannedEvents",
                table: "PlannedEvents");

            migrationBuilder.DropIndex(
                name: "IX_PlannedEvents_UserId",
                table: "PlannedEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FavoriteEvents",
                table: "FavoriteEvents");

            migrationBuilder.DropIndex(
                name: "IX_FavoriteEvents_UserId",
                table: "FavoriteEvents");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserRoles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "PlannedEvents",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "EventId1",
                table: "PlannedEvents",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "FavoriteEvents",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "EventId1",
                table: "FavoriteEvents",
                type: "integer",
                nullable: true);

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

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Events",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsFavorite",
                table: "Events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPlanned",
                table: "Events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlannedEvents",
                table: "PlannedEvents",
                columns: new[] { "UserId", "EventId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_FavoriteEvents",
                table: "FavoriteEvents",
                columns: new[] { "UserId", "EventId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlannedEvents_EventId1",
                table: "PlannedEvents",
                column: "EventId1");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteEvents_EventId1",
                table: "FavoriteEvents",
                column: "EventId1");

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteEvents_Events_EventId1",
                table: "FavoriteEvents",
                column: "EventId1",
                principalTable: "Events",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlannedEvents_Events_EventId1",
                table: "PlannedEvents",
                column: "EventId1",
                principalTable: "Events",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteEvents_Events_EventId1",
                table: "FavoriteEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_PlannedEvents_Events_EventId1",
                table: "PlannedEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlannedEvents",
                table: "PlannedEvents");

            migrationBuilder.DropIndex(
                name: "IX_PlannedEvents_EventId1",
                table: "PlannedEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FavoriteEvents",
                table: "FavoriteEvents");

            migrationBuilder.DropIndex(
                name: "IX_FavoriteEvents_EventId1",
                table: "FavoriteEvents");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "EventId1",
                table: "PlannedEvents");

            migrationBuilder.DropColumn(
                name: "EventId1",
                table: "FavoriteEvents");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "IsFavorite",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "IsPlanned",
                table: "Events");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "PlannedEvents",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "FavoriteEvents",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

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
                name: "PK_PlannedEvents",
                table: "PlannedEvents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FavoriteEvents",
                table: "FavoriteEvents",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PlannedEvents_UserId",
                table: "PlannedEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteEvents_UserId",
                table: "FavoriteEvents",
                column: "UserId");
        }
    }
}
