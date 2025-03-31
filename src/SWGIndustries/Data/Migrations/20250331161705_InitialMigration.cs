using System;
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
                name: "NamedSeries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Counter = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NamedSeries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CategoryIndex = table.Column<ushort>(type: "INTEGER", nullable: false),
                    SWGAideId = table.Column<int>(type: "INTEGER", nullable: false),
                    Planets = table.Column<int>(type: "INTEGER", nullable: false),
                    CI0 = table.Column<ushort>(type: "INTEGER", nullable: false),
                    CI1 = table.Column<ushort>(type: "INTEGER", nullable: false),
                    CI2 = table.Column<ushort>(type: "INTEGER", nullable: false),
                    CI3 = table.Column<ushort>(type: "INTEGER", nullable: false),
                    CI4 = table.Column<ushort>(type: "INTEGER", nullable: false),
                    CI5 = table.Column<ushort>(type: "INTEGER", nullable: false),
                    CI6 = table.Column<ushort>(type: "INTEGER", nullable: false),
                    CI7 = table.Column<ushort>(type: "INTEGER", nullable: false),
                    CR = table.Column<ushort>(type: "INTEGER", nullable: false),
                    CD = table.Column<ushort>(type: "INTEGER", nullable: false),
                    DR = table.Column<ushort>(type: "INTEGER", nullable: false),
                    ER = table.Column<ushort>(type: "INTEGER", nullable: false),
                    FL = table.Column<ushort>(type: "INTEGER", nullable: false),
                    HR = table.Column<ushort>(type: "INTEGER", nullable: false),
                    MA = table.Column<ushort>(type: "INTEGER", nullable: false),
                    OQ = table.Column<ushort>(type: "INTEGER", nullable: false),
                    PE = table.Column<ushort>(type: "INTEGER", nullable: false),
                    SR = table.Column<ushort>(type: "INTEGER", nullable: false),
                    UT = table.Column<ushort>(type: "INTEGER", nullable: false),
                    AvailableSince = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DepletedSince = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReportedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    CorrelationId = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    ThemeMode = table.Column<int>(type: "INTEGER", nullable: false),
                    CrewId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CrewInvitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    FromAccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    ToAccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    InviteOrRequestToJoin = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrewInvitations_AppAccounts_FromAccountId",
                        column: x => x.FromAccountId,
                        principalTable: "AppAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CrewInvitations_AppAccounts_ToAccountId",
                        column: x => x.ToAccountId,
                        principalTable: "AppAccounts",
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
                        name: "FK_Crews_AppAccounts_CrewLeaderId",
                        column: x => x.CrewLeaderId,
                        principalTable: "AppAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GameAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnerAppAccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameAccounts_AppAccounts_OwnerAppAccountId",
                        column: x => x.OwnerAppAccountId,
                        principalTable: "AppAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    GameAccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsCrewMember = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaxLotsForCrew = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Characters_GameAccounts_GameAccountId",
                        column: x => x.GameAccountId,
                        principalTable: "GameAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clusters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnerId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clusters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clusters_GameAccounts_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "GameAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Buildings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnerId = table.Column<int>(type: "INTEGER", nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    SubType = table.Column<int>(type: "INTEGER", nullable: false),
                    PutDownById = table.Column<int>(type: "INTEGER", nullable: true),
                    PutDownPlanet = table.Column<int>(type: "INTEGER", nullable: false),
                    ClusterId = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Comments = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    MaintenanceAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    MaintenanceLastUpdate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PowerAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    PowerLastUpdate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsRunning = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastRunningDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastStoppedDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HarvesterSelfPowered = table.Column<bool>(type: "INTEGER", nullable: false),
                    HarvesterBER = table.Column<int>(type: "INTEGER", nullable: false),
                    HarvesterHopperSize = table.Column<int>(type: "INTEGER", nullable: false),
                    HarvestingResourceType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buildings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Buildings_Characters_PutDownById",
                        column: x => x.PutDownById,
                        principalTable: "Characters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Buildings_Clusters_ClusterId",
                        column: x => x.ClusterId,
                        principalTable: "Clusters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Buildings_GameAccounts_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "GameAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAccounts_CrewId",
                table: "AppAccounts",
                column: "CrewId");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_ClusterId",
                table: "Buildings",
                column: "ClusterId");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_OwnerId",
                table: "Buildings",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_PutDownById",
                table: "Buildings",
                column: "PutDownById");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_GameAccountId",
                table: "Characters",
                column: "GameAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Clusters_OwnerId",
                table: "Clusters",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewInvitations_FromAccountId",
                table: "CrewInvitations",
                column: "FromAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewInvitations_ToAccountId",
                table: "CrewInvitations",
                column: "ToAccountId");

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
                name: "IX_GameAccounts_OwnerAppAccountId",
                table: "GameAccounts",
                column: "OwnerAppAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_NamedSeries_Name",
                table: "NamedSeries",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resources_CategoryIndex",
                table: "Resources",
                column: "CategoryIndex");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_CI0",
                table: "Resources",
                column: "CI0");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_CI1",
                table: "Resources",
                column: "CI1");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_CI2",
                table: "Resources",
                column: "CI2");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_CI3",
                table: "Resources",
                column: "CI3");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_CI4",
                table: "Resources",
                column: "CI4");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_CI5",
                table: "Resources",
                column: "CI5");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_CI6",
                table: "Resources",
                column: "CI6");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_CI7",
                table: "Resources",
                column: "CI7");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_DepletedSince",
                table: "Resources",
                column: "DepletedSince");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Name",
                table: "Resources",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppAccounts_Crews_CrewId",
                table: "AppAccounts",
                column: "CrewId",
                principalTable: "Crews",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppAccounts_Crews_CrewId",
                table: "AppAccounts");

            migrationBuilder.DropTable(
                name: "Buildings");

            migrationBuilder.DropTable(
                name: "CrewInvitations");

            migrationBuilder.DropTable(
                name: "NamedSeries");

            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropTable(
                name: "Clusters");

            migrationBuilder.DropTable(
                name: "GameAccounts");

            migrationBuilder.DropTable(
                name: "Crews");

            migrationBuilder.DropTable(
                name: "AppAccounts");
        }
    }
}
