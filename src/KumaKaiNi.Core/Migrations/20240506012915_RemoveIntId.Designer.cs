﻿// <auto-generated />
using System;
using KumaKaiNi.Core.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KumaKaiNi.Core.Migrations
{
    [DbContext(typeof(KumaKaiNiDbContext))]
    [Migration("20240506012915_RemoveIntId")]
    partial class RemoveIntId
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.HasSequence("quote_id");

            modelBuilder.Entity("KumaKaiNi.Core.Database.Entities.ChatLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("next_id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<long?>("ChannelId")
                        .HasColumnType("bigint")
                        .HasColumnName("channel_id");

                    b.Property<DateTime>("InsertedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("inserted_at");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("last_modified");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("message");

                    b.Property<long?>("MessageId")
                        .HasColumnType("bigint")
                        .HasColumnName("message_id");

                    b.Property<bool>("Private")
                        .HasColumnType("boolean")
                        .HasColumnName("private");

                    b.Property<string>("SourceSystem")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("source_system");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("timestamp");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("username");

                    b.HasKey("Id");

                    b.ToTable("chat_logs");
                });

            modelBuilder.Entity("KumaKaiNi.Core.Database.Entities.CustomCommand", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("next_id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("Command")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("command");

                    b.Property<DateTime>("InsertedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("inserted_at");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("last_modified");

                    b.Property<string>("Response")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("response");

                    b.HasKey("Id");

                    b.HasIndex("Command")
                        .IsUnique();

                    b.ToTable("custom_commands");
                });

            modelBuilder.Entity("KumaKaiNi.Core.Database.Entities.DanbooruBlockList", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("next_id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<DateTime>("InsertedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("inserted_at");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("last_modified");

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("tag");

                    b.HasKey("Id");

                    b.ToTable("danbooru_blocklists");
                });

            modelBuilder.Entity("KumaKaiNi.Core.Database.Entities.ErrorLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("next_id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<DateTime>("InsertedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("inserted_at");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("last_modified");

                    b.Property<string>("Message")
                        .HasColumnType("text")
                        .HasColumnName("message");

                    b.Property<string>("Source")
                        .HasColumnType("text")
                        .HasColumnName("source");

                    b.Property<string>("StackTrace")
                        .HasColumnType("text")
                        .HasColumnName("stack_trace");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("timestamp");

                    b.HasKey("Id");

                    b.ToTable("error_logs");
                });

            modelBuilder.Entity("KumaKaiNi.Core.Database.Entities.GptResponse", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("next_id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<DateTime>("InsertedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("inserted_at");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("last_modified");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("message");

                    b.Property<bool>("Returned")
                        .HasColumnType("boolean")
                        .HasColumnName("returned");

                    b.HasKey("Id");

                    b.ToTable("gpt_responses");
                });

            modelBuilder.Entity("KumaKaiNi.Core.Database.Entities.Quote", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("next_id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<DateTime>("InsertedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("inserted_at");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("last_modified");

                    b.Property<long>("QuoteId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("quote_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("QuoteId"));

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("text");

                    b.HasKey("Id");

                    b.ToTable("quotes");
                });

            modelBuilder.Entity("KumaKaiNi.Core.Database.Entities.TelegramAllowList", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("next_id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<bool>("Approved")
                        .HasColumnType("boolean")
                        .HasColumnName("approved");

                    b.Property<long>("ChannelId")
                        .HasColumnType("bigint")
                        .HasColumnName("channel_id");

                    b.Property<DateTime>("InsertedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("inserted_at");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("last_modified");

                    b.Property<int>("Warnings")
                        .HasColumnType("integer")
                        .HasColumnName("warnings");

                    b.HasKey("Id");

                    b.ToTable("telegram_allowlists");
                });
#pragma warning restore 612, 618
        }
    }
}
