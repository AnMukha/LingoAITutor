﻿// <auto-generated />
using System;
using LingoAITutor.Host.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LingoAITutor.Host.Migrations
{
    [DbContext(typeof(LingoDbContext))]
    [Migration("20231028195744_texts")]
    partial class texts
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("LingoAITutor.Host.Entities.Irregular", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<bool>("Optional")
                        .HasColumnType("boolean");

                    b.Property<string>("V1")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("V2")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("V3")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Irregulars");
                });

            modelBuilder.Entity("LingoAITutor.Host.Entities.RangeProgress", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<double?>("Progress")
                        .HasColumnType("double precision");

                    b.Property<int>("StartPosition")
                        .HasColumnType("integer");

                    b.Property<Guid>("UserProgressId")
                        .HasColumnType("uuid");

                    b.Property<int>("WordsCount")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("UserProgressId");

                    b.ToTable("RangeProgresses");
                });

            modelBuilder.Entity("LingoAITutor.Host.Entities.Text", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("OwnerUserId")
                        .HasColumnType("uuid");

                    b.Property<int>("SentenceCount")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Texts");
                });

            modelBuilder.Entity("LingoAITutor.Host.Entities.TextSentence", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Content")
                        .HasColumnType("text");

                    b.Property<int>("Number")
                        .HasColumnType("integer");

                    b.Property<Guid>("ParentTextId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ParentTextId");

                    b.ToTable("TextSentence");
                });

            modelBuilder.Entity("LingoAITutor.Host.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("ConcurrencyStamp")
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NormalizedEmail")
                        .HasColumnType("text");

                    b.Property<string>("NormalizedUserName")
                        .HasColumnType("text");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("LingoAITutor.Host.Entities.UserProgress", b =>
                {
                    b.Property<Guid>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("EstimationNumber")
                        .HasColumnType("integer");

                    b.Property<int>("ExerciseNumber")
                        .HasColumnType("integer");

                    b.HasKey("UserId");

                    b.ToTable("UserProgresses");
                });

            modelBuilder.Entity("LingoAITutor.Host.Entities.UserTextProgress", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("SentenceNumber")
                        .HasColumnType("integer");

                    b.Property<Guid>("TextId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.ToTable("UserTextProgresses");
                });

            modelBuilder.Entity("LingoAITutor.Host.Entities.UserWordProgress", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("CorrectUses")
                        .HasColumnType("integer");

                    b.Property<int?>("EstimationExerciseNumber")
                        .HasColumnType("integer");

                    b.Property<bool?>("EstimationExerciseResult")
                        .HasColumnType("boolean");

                    b.Property<bool>("FailedToUseFlag")
                        .HasColumnType("boolean");

                    b.Property<string>("FailedToUseSencence")
                        .HasColumnType("text");

                    b.Property<int>("NonUses")
                        .HasColumnType("integer");

                    b.Property<int>("ReplacedBySynonyms")
                        .HasColumnType("integer");

                    b.Property<Guid>("UserID")
                        .HasColumnType("uuid");

                    b.Property<Guid>("WordID")
                        .HasColumnType("uuid");

                    b.Property<string>("WordText")
                        .HasColumnType("text");

                    b.Property<DateTime?>("WorkOutStart")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("UserID");

                    b.HasIndex("WordID");

                    b.ToTable("UserWordProgresses");
                });

            modelBuilder.Entity("LingoAITutor.Host.Entities.Word", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<float>("FrequencyRank")
                        .HasColumnType("real");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("XOnMap")
                        .HasColumnType("integer");

                    b.Property<int>("YOnMap")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Words");
                });

            modelBuilder.Entity("LingoAITutor.Host.Entities.RangeProgress", b =>
                {
                    b.HasOne("LingoAITutor.Host.Entities.UserProgress", "UserProgress")
                        .WithMany("RangeProgresses")
                        .HasForeignKey("UserProgressId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserProgress");
                });

            modelBuilder.Entity("LingoAITutor.Host.Entities.TextSentence", b =>
                {
                    b.HasOne("LingoAITutor.Host.Entities.Text", "ParentText")
                        .WithMany("Sentences")
                        .HasForeignKey("ParentTextId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ParentText");
                });

            modelBuilder.Entity("LingoAITutor.Host.Entities.UserWordProgress", b =>
                {
                    b.HasOne("LingoAITutor.Host.Entities.User", "User")
                        .WithMany("UserWordProgresses")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("LingoAITutor.Host.Entities.Word", "Word")
                        .WithMany("UserWordProgresses")
                        .HasForeignKey("WordID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");

                    b.Navigation("Word");
                });

            modelBuilder.Entity("LingoAITutor.Host.Entities.Text", b =>
                {
                    b.Navigation("Sentences");
                });

            modelBuilder.Entity("LingoAITutor.Host.Entities.User", b =>
                {
                    b.Navigation("UserWordProgresses");
                });

            modelBuilder.Entity("LingoAITutor.Host.Entities.UserProgress", b =>
                {
                    b.Navigation("RangeProgresses");
                });

            modelBuilder.Entity("LingoAITutor.Host.Entities.Word", b =>
                {
                    b.Navigation("UserWordProgresses");
                });
#pragma warning restore 612, 618
        }
    }
}
