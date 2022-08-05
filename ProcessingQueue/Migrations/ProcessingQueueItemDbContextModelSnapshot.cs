﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProcessingQueue.Infrastructure;

#nullable disable

namespace ProcessingQueue.Migrations
{
    [DbContext(typeof(ProcessingQueueItemDbContext))]
    partial class ProcessingQueueItemDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("ProcessingQueue.Domain.Aggregates.ProcessingQueueItemAggregate.ProcessingQueueItem", b =>
                {
                    b.Property<int>("ProcessingQueueItemKey")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ProcessingQueueItemKey"), 1L, 1);

                    b.Property<string>("EventContent")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("EventCreationTimestamp")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("EventId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("EventInstanceKey")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EventName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EventTypeName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("FailedTimestamp")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("InsertedTimestamp")
                        .HasColumnType("datetime2");

                    b.Property<int>("ProcessAttempts")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ProcessedTimestamp")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("ReadyForProcessingTimestamp")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("SkippedTimestamp")
                        .HasColumnType("datetime2");

                    b.Property<int>("State")
                        .HasColumnType("int");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TenantUserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("WaitingForProcessingQueueItemId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("WaitingTimestamp")
                        .HasColumnType("datetime2");

                    b.HasKey("ProcessingQueueItemKey");

                    b.ToTable("ProcessingQueueItem", "processing");
                });
#pragma warning restore 612, 618
        }
    }
}
