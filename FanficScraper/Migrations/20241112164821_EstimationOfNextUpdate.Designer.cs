﻿// <auto-generated />
using System;
using FanficScraper.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FanficScraper.Migrations
{
    [DbContext(typeof(StoryContext))]
    [Migration("20241112164821_EstimationOfNextUpdate")]
    partial class EstimationOfNextUpdate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.7");

            modelBuilder.Entity("FanficScraper.Data.DownloadJob", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("AddedDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("FileName")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("FinishDate")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Force")
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("RunnerId")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("AddedDate");

                    b.HasIndex("Status", "AddedDate");

                    b.ToTable("DownloadJobs");
                });

            modelBuilder.Entity("FanficScraper.Data.Story", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AuthorName")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .UseCollation("NOCASE");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsComplete")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("NextUpdateIn")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("StoryAdded")
                        .HasColumnType("TEXT");

                    b.Property<string>("StoryName")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .UseCollation("NOCASE");

                    b.Property<DateTime>("StoryUpdated")
                        .HasColumnType("TEXT");

                    b.Property<string>("StoryUrl")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Website")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("FileName");

                    b.HasIndex("LastUpdated");

                    b.HasIndex("NextUpdateIn");

                    b.HasIndex("StoryAdded");

                    b.HasIndex("StoryName");

                    b.HasIndex("StoryUpdated");

                    b.ToTable("Stories");
                });

            modelBuilder.Entity("FanficScraper.Data.StoryData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Category")
                        .HasColumnType("TEXT");

                    b.Property<string>("Characters")
                        .HasColumnType("TEXT");

                    b.Property<string>("DescriptionParagraphs")
                        .HasColumnType("TEXT");

                    b.Property<string>("Genre")
                        .HasColumnType("TEXT");

                    b.Property<int?>("NumChapters")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("NumWords")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Rating")
                        .HasColumnType("TEXT");

                    b.Property<string>("Relationships")
                        .HasColumnType("TEXT");

                    b.Property<int>("StoryId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Warnings")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("StoryId")
                        .IsUnique();

                    b.ToTable("StoryData");
                });

            modelBuilder.Entity("FanficScraper.Data.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsActivated")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Login")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Login")
                        .IsUnique();

                    b.HasIndex("CreationDate", "IsActivated");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("FanficScraper.Data.StoryData", b =>
                {
                    b.HasOne("FanficScraper.Data.Story", "Story")
                        .WithOne("StoryData")
                        .HasForeignKey("FanficScraper.Data.StoryData", "StoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Story");
                });

            modelBuilder.Entity("FanficScraper.Data.Story", b =>
                {
                    b.Navigation("StoryData");
                });
#pragma warning restore 612, 618
        }
    }
}
