using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Migrations
{
    /// <inheritdoc />
    public partial class FixPostReactionMapping2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostReactions_Events_EventId",
                table: "PostReactions");

            migrationBuilder.AddForeignKey(
                name: "FK_PostReactions_Events_PostId",
                table: "PostReactions",
                column: "PostId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostReactions_Events_PostId",
                table: "PostReactions");

            migrationBuilder.RenameColumn(
                name: "PostId",
                table: "PostReactions",
                newName: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostReactions_Events_EventId",
                table: "PostReactions",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
