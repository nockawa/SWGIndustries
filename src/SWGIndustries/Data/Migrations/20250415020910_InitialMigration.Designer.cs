﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SWGIndustries.Data;

#nullable disable

namespace SWGIndustries.data.migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250415020910_InitialMigration")]
    partial class InitialMigration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.3");

            modelBuilder.Entity("SWGIndustries.Data.AppAccountEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CorrelationId")
                        .HasMaxLength(30)
                        .HasColumnType("TEXT");

                    b.Property<int?>("CrewId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasMaxLength(32)
                        .HasColumnType("TEXT");

                    b.Property<string>("SWGServerName")
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.Property<int>("ThemeMode")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CorrelationId")
                        .IsUnique();

                    b.HasIndex("CrewId");

                    b.ToTable("AppAccounts", (string)null);
                });

            modelBuilder.Entity("SWGIndustries.Data.BuildingEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("BuildingForCrew")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ClusterId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Comments")
                        .HasMaxLength(1024)
                        .HasColumnType("TEXT");

                    b.Property<string>("FullClass")
                        .HasMaxLength(128)
                        .HasColumnType("TEXT");

                    b.Property<int>("HarvesterBER")
                        .HasColumnType("INTEGER");

                    b.Property<int>("HarvesterHopperSize")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("HarvesterSelfPowered")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("HarvestingResourceId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("HarvestingResourceType")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsRunning")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastRunningDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastStoppedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("MaintenanceAmount")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("MaintenanceLastUpdate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.Property<int?>("OwnerId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PowerAmount")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("PowerLastUpdate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("PutDownById")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("PutDownDateTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("PutDownPlanet")
                        .HasColumnType("INTEGER");

                    b.Property<float>("ResourceConcentration")
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.HasIndex("ClusterId");

                    b.HasIndex("FullClass");

                    b.HasIndex("HarvestingResourceId");

                    b.HasIndex("OwnerId");

                    b.HasIndex("PutDownById");

                    b.ToTable("Buildings", (string)null);
                });

            modelBuilder.Entity("SWGIndustries.Data.CharacterEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("GameAccountId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsCrewMember")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MaxLotsForCrew")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasMaxLength(30)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("GameAccountId");

                    b.ToTable("Characters", (string)null);
                });

            modelBuilder.Entity("SWGIndustries.Data.ClusterEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Comments")
                        .HasMaxLength(1024)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreationDateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<int?>("CrewId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("GameAccountId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasMaxLength(32)
                        .HasColumnType("TEXT");

                    b.Property<int>("Planet")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ResourceId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Waypoint")
                        .HasMaxLength(128)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CrewId");

                    b.HasIndex("GameAccountId");

                    b.HasIndex("ResourceId");

                    b.ToTable("Clusters", (string)null);
                });

            modelBuilder.Entity("SWGIndustries.Data.CrewEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("CrewLeaderId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CrewLeaderId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Crews", (string)null);
                });

            modelBuilder.Entity("SWGIndustries.Data.CrewInvitationEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("FromAccountId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("InviteOrRequestToJoin")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ToAccountId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("FromAccountId");

                    b.HasIndex("ToAccountId");

                    b.ToTable("CrewInvitations", (string)null);
                });

            modelBuilder.Entity("SWGIndustries.Data.GameAccountEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasMaxLength(30)
                        .HasColumnType("TEXT");

                    b.Property<int?>("OwnerAppAccountId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("OwnerAppAccountId");

                    b.ToTable("GameAccounts", (string)null);
                });

            modelBuilder.Entity("SWGIndustries.Data.NamedSeriesEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Counter")
                        .IsConcurrencyToken()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("NamedSeries", (string)null);
                });

            modelBuilder.Entity("SWGIndustries.Data.ResourceEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("AvailableSince")
                        .HasColumnType("TEXT");

                    b.Property<ushort>("CD")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("CI0")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("CI1")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("CI2")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("CI3")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("CI4")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("CI5")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("CI6")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("CI7")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("CR")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("CategoryIndex")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("DR")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("DepletedSince")
                        .HasColumnType("TEXT");

                    b.Property<ushort>("ER")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("FL")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GameServerId")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("HR")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("MA")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.Property<ushort>("OQ")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("PE")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Planets")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ReportedBy")
                        .HasColumnType("TEXT");

                    b.Property<ushort>("SR")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SWGAideId")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("UT")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("GameServerId", "CI0");

                    b.HasIndex("GameServerId", "CI1");

                    b.HasIndex("GameServerId", "CI2");

                    b.HasIndex("GameServerId", "CI3");

                    b.HasIndex("GameServerId", "CI4");

                    b.HasIndex("GameServerId", "CI5");

                    b.HasIndex("GameServerId", "CI6");

                    b.HasIndex("GameServerId", "CI7");

                    b.HasIndex("GameServerId", "CategoryIndex");

                    b.HasIndex("GameServerId", "DepletedSince");

                    b.HasIndex("GameServerId", "Name")
                        .IsUnique();

                    b.HasIndex("GameServerId", "SWGAideId")
                        .IsUnique();

                    b.ToTable("Resources", (string)null);
                });

            modelBuilder.Entity("SWGIndustries.Data.AppAccountEntity", b =>
                {
                    b.HasOne("SWGIndustries.Data.CrewEntity", "Crew")
                        .WithMany("Members")
                        .HasForeignKey("CrewId");

                    b.Navigation("Crew");
                });

            modelBuilder.Entity("SWGIndustries.Data.BuildingEntity", b =>
                {
                    b.HasOne("SWGIndustries.Data.ClusterEntity", "Cluster")
                        .WithMany("Buildings")
                        .HasForeignKey("ClusterId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("SWGIndustries.Data.ResourceEntity", "HarvestingResource")
                        .WithMany()
                        .HasForeignKey("HarvestingResourceId");

                    b.HasOne("SWGIndustries.Data.GameAccountEntity", "Owner")
                        .WithMany("Buildings")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SWGIndustries.Data.CharacterEntity", "PutDownBy")
                        .WithMany("PutDownBuildings")
                        .HasForeignKey("PutDownById")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("Cluster");

                    b.Navigation("HarvestingResource");

                    b.Navigation("Owner");

                    b.Navigation("PutDownBy");
                });

            modelBuilder.Entity("SWGIndustries.Data.CharacterEntity", b =>
                {
                    b.HasOne("SWGIndustries.Data.GameAccountEntity", "GameAccount")
                        .WithMany("Characters")
                        .HasForeignKey("GameAccountId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("GameAccount");
                });

            modelBuilder.Entity("SWGIndustries.Data.ClusterEntity", b =>
                {
                    b.HasOne("SWGIndustries.Data.CrewEntity", "Crew")
                        .WithMany("Clusters")
                        .HasForeignKey("CrewId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SWGIndustries.Data.GameAccountEntity", "GameAccount")
                        .WithMany("Clusters")
                        .HasForeignKey("GameAccountId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SWGIndustries.Data.ResourceEntity", "Resource")
                        .WithMany()
                        .HasForeignKey("ResourceId");

                    b.Navigation("Crew");

                    b.Navigation("GameAccount");

                    b.Navigation("Resource");
                });

            modelBuilder.Entity("SWGIndustries.Data.CrewEntity", b =>
                {
                    b.HasOne("SWGIndustries.Data.AppAccountEntity", "CrewLeader")
                        .WithMany()
                        .HasForeignKey("CrewLeaderId");

                    b.Navigation("CrewLeader");
                });

            modelBuilder.Entity("SWGIndustries.Data.CrewInvitationEntity", b =>
                {
                    b.HasOne("SWGIndustries.Data.AppAccountEntity", "FromAccount")
                        .WithMany()
                        .HasForeignKey("FromAccountId");

                    b.HasOne("SWGIndustries.Data.AppAccountEntity", "ToAccount")
                        .WithMany()
                        .HasForeignKey("ToAccountId");

                    b.Navigation("FromAccount");

                    b.Navigation("ToAccount");
                });

            modelBuilder.Entity("SWGIndustries.Data.GameAccountEntity", b =>
                {
                    b.HasOne("SWGIndustries.Data.AppAccountEntity", "OwnerAppAccount")
                        .WithMany("GameAccounts")
                        .HasForeignKey("OwnerAppAccountId");

                    b.Navigation("OwnerAppAccount");
                });

            modelBuilder.Entity("SWGIndustries.Data.AppAccountEntity", b =>
                {
                    b.Navigation("GameAccounts");
                });

            modelBuilder.Entity("SWGIndustries.Data.CharacterEntity", b =>
                {
                    b.Navigation("PutDownBuildings");
                });

            modelBuilder.Entity("SWGIndustries.Data.ClusterEntity", b =>
                {
                    b.Navigation("Buildings");
                });

            modelBuilder.Entity("SWGIndustries.Data.CrewEntity", b =>
                {
                    b.Navigation("Clusters");

                    b.Navigation("Members");
                });

            modelBuilder.Entity("SWGIndustries.Data.GameAccountEntity", b =>
                {
                    b.Navigation("Buildings");

                    b.Navigation("Characters");

                    b.Navigation("Clusters");
                });
#pragma warning restore 612, 618
        }
    }
}
