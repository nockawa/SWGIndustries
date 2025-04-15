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
                    GameServerId = table.Column<int>(type: "INTEGER", nullable: false),
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
                    SWGServerName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
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
                    CreationDateTime = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CrewId = table.Column<int>(type: "INTEGER", nullable: true),
                    GameAccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    ResourceId = table.Column<int>(type: "INTEGER", nullable: true),
                    Planet = table.Column<int>(type: "INTEGER", nullable: false),
                    Waypoint = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    Comments = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clusters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clusters_Crews_CrewId",
                        column: x => x.CrewId,
                        principalTable: "Crews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Clusters_GameAccounts_GameAccountId",
                        column: x => x.GameAccountId,
                        principalTable: "GameAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Clusters_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Buildings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnerId = table.Column<int>(type: "INTEGER", nullable: true),
                    FullClass = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    PutDownById = table.Column<int>(type: "INTEGER", nullable: true),
                    PutDownDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    BuildingForCrew = table.Column<bool>(type: "INTEGER", nullable: false),
                    PutDownPlanet = table.Column<int>(type: "INTEGER", nullable: false),
                    ClusterId = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Comments = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
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
                    HarvestingResourceType = table.Column<int>(type: "INTEGER", nullable: false),
                    HarvestingResourceId = table.Column<int>(type: "INTEGER", nullable: true),
                    ResourceConcentration = table.Column<float>(type: "REAL", nullable: false)
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
                    table.ForeignKey(
                        name: "FK_Buildings_Resources_HarvestingResourceId",
                        column: x => x.HarvestingResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAccounts_CorrelationId",
                table: "AppAccounts",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppAccounts_CrewId",
                table: "AppAccounts",
                column: "CrewId");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_ClusterId",
                table: "Buildings",
                column: "ClusterId");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_FullClass",
                table: "Buildings",
                column: "FullClass");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_HarvestingResourceId",
                table: "Buildings",
                column: "HarvestingResourceId");

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
                name: "IX_Clusters_CrewId",
                table: "Clusters",
                column: "CrewId");

            migrationBuilder.CreateIndex(
                name: "IX_Clusters_GameAccountId",
                table: "Clusters",
                column: "GameAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Clusters_ResourceId",
                table: "Clusters",
                column: "ResourceId");

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
                name: "IX_Resources_GameServerId_CategoryIndex",
                table: "Resources",
                columns: new[] { "GameServerId", "CategoryIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_GameServerId_CI0",
                table: "Resources",
                columns: new[] { "GameServerId", "CI0" });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_GameServerId_CI1",
                table: "Resources",
                columns: new[] { "GameServerId", "CI1" });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_GameServerId_CI2",
                table: "Resources",
                columns: new[] { "GameServerId", "CI2" });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_GameServerId_CI3",
                table: "Resources",
                columns: new[] { "GameServerId", "CI3" });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_GameServerId_CI4",
                table: "Resources",
                columns: new[] { "GameServerId", "CI4" });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_GameServerId_CI5",
                table: "Resources",
                columns: new[] { "GameServerId", "CI5" });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_GameServerId_CI6",
                table: "Resources",
                columns: new[] { "GameServerId", "CI6" });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_GameServerId_CI7",
                table: "Resources",
                columns: new[] { "GameServerId", "CI7" });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_GameServerId_DepletedSince",
                table: "Resources",
                columns: new[] { "GameServerId", "DepletedSince" });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_GameServerId_Name",
                table: "Resources",
                columns: new[] { "GameServerId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resources_GameServerId_SWGAideId",
                table: "Resources",
                columns: new[] { "GameServerId", "SWGAideId" },
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
                name: "Characters");

            migrationBuilder.DropTable(
                name: "Clusters");

            migrationBuilder.DropTable(
                name: "GameAccounts");

            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "Crews");

            migrationBuilder.DropTable(
                name: "AppAccounts");
        }
    }
}
