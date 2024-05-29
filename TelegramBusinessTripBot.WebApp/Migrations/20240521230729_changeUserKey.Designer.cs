﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TelegramBusinessTripBot.WebApp.Entities;

#nullable disable

namespace TelegramBusinessTripBot.WebApp.Migrations
{
    [DbContext(typeof(TgContext))]
    [Migration("20240521230729_changeUserKey")]
    partial class changeUserKey
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.5")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true)
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("ChatOrChannelUsers", b =>
                {
                    b.Property<int>("ChatOrChannelId")
                        .HasColumnType("int");

                    b.Property<long>("UsersUserId")
                        .HasColumnType("bigint");

                    b.Property<long>("UsersUserHash")
                        .HasColumnType("bigint");

                    b.HasKey("ChatOrChannelId", "UsersUserId", "UsersUserHash");

                    b.HasIndex("UsersUserId", "UsersUserHash");

                    b.ToTable("UsersChatOrChannel", (string)null);
                });

            modelBuilder.Entity("TelegramBusinessTripBot.WebApp.Entities.ChatOrChannel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<long>("ChatOrChannelId")
                        .HasColumnType("bigint");

                    b.Property<long?>("Hash")
                        .HasColumnType("bigint");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ChatOrChannels");
                });

            modelBuilder.Entity("TelegramBusinessTripBot.WebApp.Entities.Users", b =>
                {
                    b.Property<long?>("UserId")
                        .HasColumnType("bigint");

                    b.Property<long?>("UserHash")
                        .HasColumnType("bigint");

                    b.Property<bool?>("AdminAccount")
                        .HasColumnType("bit");

                    b.Property<string>("FIO")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("TrevelAccount")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "UserHash");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ChatOrChannelUsers", b =>
                {
                    b.HasOne("TelegramBusinessTripBot.WebApp.Entities.ChatOrChannel", null)
                        .WithMany()
                        .HasForeignKey("ChatOrChannelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_UsersChatOrChannel_Users");

                    b.HasOne("TelegramBusinessTripBot.WebApp.Entities.Users", null)
                        .WithMany()
                        .HasForeignKey("UsersUserId", "UsersUserHash")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_UsersChatOrChannel_ChatOrChannel");
                });
#pragma warning restore 612, 618
        }
    }
}
