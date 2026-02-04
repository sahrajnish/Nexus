using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Nexus.Identity.API.Migrations
{
    /// <inheritdoc />
    public partial class TempUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TempUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EmailExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EmailAddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OtpCode = table.Column<string>(type: "text", nullable: true),
                    OtpCreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OtpExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsOtpUsed = table.Column<bool>(type: "boolean", nullable: false),
                    OtpAttempts = table.Column<int>(type: "integer", nullable: false),
                    OtpResendAttempts = table.Column<int>(type: "integer", nullable: false),
                    OtpReattemptAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResendReattemptAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempUsers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TempUsers_Email",
                table: "TempUsers",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TempUsers");
        }
    }
}
