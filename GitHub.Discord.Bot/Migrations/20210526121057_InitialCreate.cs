using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GitHub.Discord.Bot.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GDB_ISSUE_REFERENCE",
                columns: table => new
                {
                    ISSUE_REFERENCE_NR = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DSC_GUILD_ID = table.Column<ulong>(type: "INTEGER", nullable: false),
                    DSC_TEXT_CHANNEL_ID = table.Column<ulong>(type: "INTEGER", nullable: false),
                    DSC_MESSAGE_ID = table.Column<ulong>(type: "INTEGER", nullable: false),
                    GH_ISSUE_NR = table.Column<int>(type: "INTEGER", nullable: false),
                    GH_REPOSITORY_ID = table.Column<long>(type: "INTEGER", nullable: false),
                    EVT_UPDATED = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    GH_ISSUE_HTML_URL = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GDB_ISSUE_REFERENCE", x => x.ISSUE_REFERENCE_NR);
                });

            migrationBuilder.CreateTable(
                name: "GDB_USER",
                columns: table => new
                {
                    USER_NR = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DISCORD_USER_NR = table.Column<long>(type: "INTEGER", nullable: false),
                    DISCORD_USER_NAME = table.Column<string>(type: "TEXT", nullable: false, comment: "The discord username with discriminator, e.g. My_Name#1234")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GDB_USER", x => x.USER_NR);
                });

            migrationBuilder.CreateTable(
                name: "GDB_OPT_IN",
                columns: table => new
                {
                    OPT_IN_NR = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EVT_CREATED = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OPT_IN_STATUS = table.Column<int>(type: "INTEGER", nullable: false),
                    USER_NR = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GDB_OPT_IN", x => x.OPT_IN_NR);
                    table.ForeignKey(
                        name: "FK_GDB_OPT_IN_GDB_USER_USER_NR",
                        column: x => x.USER_NR,
                        principalTable: "GDB_USER",
                        principalColumn: "USER_NR",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GDB_OPT_IN_USER_NR",
                table: "GDB_OPT_IN",
                column: "USER_NR",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GDB_ISSUE_REFERENCE");

            migrationBuilder.DropTable(
                name: "GDB_OPT_IN");

            migrationBuilder.DropTable(
                name: "GDB_USER");
        }
    }
}
