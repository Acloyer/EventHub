using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Migrations
{
    public partial class TelegramFieldsToLong : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert TelegramId
            migrationBuilder.Sql(
                @"ALTER TABLE ""AspNetUsers""
                ALTER COLUMN ""TelegramId"" TYPE bigint
                USING ""TelegramId""::bigint;");

            // Convert VerificationCode
            migrationBuilder.Sql(
                @"ALTER TABLE ""TelegramVerifications""
                ALTER COLUMN ""VerificationCode"" TYPE bigint
                USING ""VerificationCode""::bigint;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback — back to integer
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
