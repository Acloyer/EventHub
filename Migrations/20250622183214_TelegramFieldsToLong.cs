using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Migrations
{
    public partial class TelegramFieldsToLong : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
<<<<<<< HEAD
            // Convert TelegramId
=======
            // Приводим TelegramId
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
            migrationBuilder.Sql(
                @"ALTER TABLE ""AspNetUsers""
                ALTER COLUMN ""TelegramId"" TYPE bigint
                USING ""TelegramId""::bigint;");

<<<<<<< HEAD
            // Convert VerificationCode
=======
            // Приводим VerificationCode
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
            migrationBuilder.Sql(
                @"ALTER TABLE ""TelegramVerifications""
                ALTER COLUMN ""VerificationCode"" TYPE bigint
                USING ""VerificationCode""::bigint;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
<<<<<<< HEAD
            // Rollback — back to integer
=======
            // Откат — обратно в integer
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
            migrationBuilder.Sql(
                @"ALTER TABLE ""AspNetUsers""
                ALTER COLUMN ""TelegramId"" TYPE integer
                USING ""TelegramId""::integer;");

            migrationBuilder.Sql(
                @"ALTER TABLE ""TelegramVerifications""
                ALTER COLUMN ""VerificationCode"" TYPE integer
                USING ""VerificationCode""::integer;");
        }

    }
}
