using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Migrations
{
    public partial class CleanupEventCommentsMapping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventComments_AspNetUsers_UserId1",
                table: "EventComments");
            migrationBuilder.DropForeignKey(
                name: "FK_EventComments_Events_EventId1",
                table: "EventComments");

            migrationBuilder.DropIndex(
                name: "IX_EventComments_UserId1",
                table: "EventComments");
            migrationBuilder.DropIndex(
                name: "IX_EventComments_EventId1",
                table: "EventComments");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "EventComments");
            migrationBuilder.DropColumn(
                name: "EventId1",
                table: "EventComments");
        }

<<<<<<< HEAD
        // Leave Down() empty — schema is not needed on rollback
=======
        // оставляем Down() пустым — при откате схема уже не нужна
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
