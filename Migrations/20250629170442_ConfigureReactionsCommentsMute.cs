using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EventHub.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureReactionsCommentsMute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserMuteEntries_AspNetUsers_UserId1",
                table: "UserMuteEntries");

            migrationBuilder.DropIndex(
                name: "IX_UserMuteEntries_UserId1",
                table: "UserMuteEntries");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserMuteEntries");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserMuteEntries",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "EventId1",
                table: "EventComments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "EventComments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PostReactions_UserId",
                table: "PostReactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EventComments_EventId1",
                table: "EventComments",
                column: "EventId1");

            migrationBuilder.CreateIndex(
                name: "IX_EventComments_UserId1",
                table: "EventComments",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_EventComments_AspNetUsers_UserId1",
                table: "EventComments",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventComments_Events_EventId1",
                table: "EventComments",
                column: "EventId1",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostReactions_AspNetUsers_UserId",
                table: "PostReactions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddColumn<string>(
                name: "Emoji",
                table: "PostReactions",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
            // ensure the column actually exists
            migrationBuilder.AddColumn<int>(
                name: "EventId",
                table: "PostReactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_PostReactions_Events_EventId",
                table: "PostReactions",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMuteEntries_AspNetUsers_UserId",
                table: "UserMuteEntries",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventComments_AspNetUsers_UserId1",
                table: "EventComments");

            migrationBuilder.DropForeignKey(
                name: "FK_EventComments_Events_EventId1",
                table: "EventComments");

            migrationBuilder.DropForeignKey(
                name: "FK_PostReactions_AspNetUsers_UserId",
                table: "PostReactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PostReactions_Events_EventId",
                table: "PostReactions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMuteEntries_AspNetUsers_UserId",
                table: "UserMuteEntries");

            migrationBuilder.DropIndex(
                name: "IX_PostReactions_UserId",
                table: "PostReactions");

            migrationBuilder.DropIndex(
                name: "IX_EventComments_EventId1",
                table: "EventComments");

            migrationBuilder.DropIndex(
                name: "IX_EventComments_UserId1",
                table: "EventComments");

            migrationBuilder.DropColumn(
                name: "EventId1",
                table: "EventComments");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "EventComments");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserMuteEntries",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "UserMuteEntries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserMuteEntries_UserId1",
                table: "UserMuteEntries",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserMuteEntries_AspNetUsers_UserId1",
                table: "UserMuteEntries",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
