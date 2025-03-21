using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWGIndustries.data.migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    CorrelationId = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    ThemeMode = table.Column<int>(type: "INTEGER", nullable: false),
                    CrewId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CrewInvitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    FromUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    ToUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    InviteOrRequestToJoin = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrewInvitations_ApplicationUsers_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CrewInvitations_ApplicationUsers_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Crews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CrewLeaderId = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crews_ApplicationUsers_CrewLeaderId",
                        column: x => x.CrewLeaderId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SWGAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnerApplicationUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SWGAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SWGAccounts_ApplicationUsers_OwnerApplicationUserId",
                        column: x => x.OwnerApplicationUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SWGCharacters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    SWGAccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsCrewMember = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SWGCharacters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SWGCharacters_SWGAccounts_SWGAccountId",
                        column: x => x.SWGAccountId,
                        principalTable: "SWGAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_CrewId",
                table: "ApplicationUsers",
                column: "CrewId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewInvitations_FromUserId",
                table: "CrewInvitations",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewInvitations_ToUserId",
                table: "CrewInvitations",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Crews_CrewLeaderId",
                table: "Crews",
                column: "CrewLeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_Crews_Name",
                table: "Crews",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SWGAccounts_OwnerApplicationUserId",
                table: "SWGAccounts",
                column: "OwnerApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SWGCharacters_SWGAccountId",
                table: "SWGCharacters",
                column: "SWGAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUsers_Crews_CrewId",
                table: "ApplicationUsers",
                column: "CrewId",
                principalTable: "Crews",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUsers_Crews_CrewId",
                table: "ApplicationUsers");

            migrationBuilder.DropTable(
                name: "CrewInvitations");

            migrationBuilder.DropTable(
                name: "SWGCharacters");

            migrationBuilder.DropTable(
                name: "SWGAccounts");

            migrationBuilder.DropTable(
                name: "Crews");

            migrationBuilder.DropTable(
                name: "ApplicationUsers");
        }
    }
}
