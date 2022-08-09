using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcessingQueue.Migrations
{
    public partial class InitialMigrate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "processing");

            migrationBuilder.CreateTable(
                name: "ProcessingQueueItem",
                schema: "processing",
                columns: table => new
                {
                    ProcessingQueueItemKey = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    State = table.Column<int>(type: "int", nullable: false),
                    ProcessAttempts = table.Column<int>(type: "int", nullable: false),
                    InsertedTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SkippedTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WaitingTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WaitingForProcessingQueueItemId = table.Column<int>(type: "int", nullable: false),
                    ReadyForProcessingTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventEntityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventInstanceKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventTypeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventCreationTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenantUserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessingQueueItem", x => x.ProcessingQueueItemKey);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessingQueueItem",
                schema: "processing");
        }
    }
}
