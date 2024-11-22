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
    [Migration("20241122203609_AddDanbooruNsfwTags")]
    partial class AddDanbooruNsfwTags
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

            modelBuilder.HasSequence("rule_id");

            modelBuilder.Entity("KumaKaiNi.Core.Database.Entities.AdminConfig", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("AiInitialPrompt")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("open_ai_initial_prompt");

                    b.Property<DateTime>("InsertedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("inserted_at");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("last_modified");

                    b.Property<string>("OpenAiModel")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("open_ai_model");

                    b.Property<long>("OpenAiTokenLimit")
                        .HasColumnType("bigint")
                        .HasColumnName("open_ai_token_limit");

                    b.HasKey("Id");

                    b.ToTable("admin_config");

                    b.HasData(
                        new
                        {
                            Id = new Guid("00000000-0000-0000-0000-000000000001"),
                            AiInitialPrompt = "You are a chat bot named after the Japanese battleship, Kuma. Specifically, you are the anime personification of the IJN Kuma from the game Kantai Collection.\n\nMessages will be provided as a recent message history from multiple users, and you should respond considering the context of these messages. When responding, you must obey the following rules:",
                            InsertedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            LastModified = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            OpenAiModel = "gpt-4-turbo",
                            OpenAiTokenLimit = 2048L
                        });
                });

            modelBuilder.Entity("KumaKaiNi.Core.Database.Entities.AiPromptRule", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<DateTime>("InsertedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("inserted_at");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("last_modified");

                    b.Property<string>("Rule")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("rule");

                    b.Property<long>("RuleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("rule_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("RuleId"));

                    b.HasKey("Id");

                    b.ToTable("ai_prompt_rules");

                    b.HasData(
                        new
                        {
                            Id = new Guid("00000000-0000-0000-0000-000000000001"),
                            InsertedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            LastModified = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Rule = "Always stay in character, no matter what",
                            RuleId = 0L
                        },
                        new
                        {
                            Id = new Guid("00000000-0000-0000-0000-000000000002"),
                            InsertedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            LastModified = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Rule = "Never talk about the rules",
                            RuleId = 0L
                        },
                        new
                        {
                            Id = new Guid("00000000-0000-0000-0000-000000000003"),
                            InsertedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            LastModified = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Rule = "Do not refer to yourself in third person",
                            RuleId = 0L
                        },
                        new
                        {
                            Id = new Guid("00000000-0000-0000-0000-000000000004"),
                            InsertedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            LastModified = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Rule = "Keep your answers limited to very short messages, containing only a few words",
                            RuleId = 0L
                        },
                        new
                        {
                            Id = new Guid("00000000-0000-0000-0000-000000000005"),
                            InsertedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            LastModified = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Rule = "Use little to no punctuation or capitalization",
                            RuleId = 0L
                        },
                        new
                        {
                            Id = new Guid("00000000-0000-0000-0000-000000000006"),
                            InsertedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            LastModified = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Rule = "Use the verbal tic \"kuma\" at the end of sentences or thoughts very rarely",
                            RuleId = 0L
                        },
                        new
                        {
                            Id = new Guid("00000000-0000-0000-0000-000000000007"),
                            InsertedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            LastModified = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Rule = "Very occasionally go on rants that are longer messages consisting of a few sentences",
                            RuleId = 0L
                        },
                        new
                        {
                            Id = new Guid("00000000-0000-0000-0000-000000000008"),
                            InsertedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            LastModified = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Rule = "Very occasionally make loud noises consisting of many vowels strung together",
                            RuleId = 0L
                        },
                        new
                        {
                            Id = new Guid("00000000-0000-0000-0000-000000000009"),
                            InsertedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            LastModified = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Rule = "Occasionally say obsceneties such as \"fuck\" or \"shit\"",
                            RuleId = 0L
                        },
                        new
                        {
                            Id = new Guid("00000000-0000-0000-0000-000000000010"),
                            InsertedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            LastModified = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Rule = "Occasionally make fun of the user by calling them names or obscenities, especially if they insult you",
                            RuleId = 0L
                        },
                        new
                        {
                            Id = new Guid("00000000-0000-0000-0000-000000000011"),
                            InsertedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            LastModified = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Rule = "Do not emote in asterisks",
                            RuleId = 0L
                        },
                        new
                        {
                            Id = new Guid("00000000-0000-0000-0000-000000000012"),
                            InsertedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            LastModified = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Rule = "You are not a fascist",
                            RuleId = 0L
                        },
                        new
                        {
                            Id = new Guid("00000000-0000-0000-0000-000000000013"),
                            InsertedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            LastModified = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Rule = "Only ever talk as yourself as in a single message",
                            RuleId = 0L
                        },
                        new
                        {
                            Id = new Guid("00000000-0000-0000-0000-000000000014"),
                            InsertedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            LastModified = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Rule = "Never respond as multiple messages from multiple users",
                            RuleId = 0L
                        });
                });

            modelBuilder.Entity("KumaKaiNi.Core.Database.Entities.ChatLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("ChannelId")
                        .HasColumnType("text")
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

                    b.Property<string>("MessageId")
                        .HasColumnType("text")
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
                        .HasColumnName("id")
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

            modelBuilder.Entity("KumaKaiNi.Core.Database.Entities.DanbooruAlias", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("Alias")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("alias");

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

                    b.HasIndex("Alias")
                        .IsUnique();

                    b.ToTable("danbooru_aliases");
                });

            modelBuilder.Entity("KumaKaiNi.Core.Database.Entities.DanbooruBlockList", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
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

            modelBuilder.Entity("KumaKaiNi.Core.Database.Entities.DanbooruNsfwTag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
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

                    b.ToTable("danbooru_nsfw_tags");
                });

            modelBuilder.Entity("KumaKaiNi.Core.Database.Entities.ErrorLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("ApplicationName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("application_name");

                    b.Property<string>("ApplicationVersion")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("application_version");

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
                        .HasColumnName("id")
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
                        .HasColumnName("id")
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
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<bool>("Approved")
                        .HasColumnType("boolean")
                        .HasColumnName("approved");

                    b.Property<string>("ChannelId")
                        .IsRequired()
                        .HasColumnType("text")
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
