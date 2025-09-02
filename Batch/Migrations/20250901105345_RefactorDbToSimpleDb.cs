using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Batch.Migrations
{
    /// <inheritdoc />
    public partial class RefactorDbToSimpleDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DisplayTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    AmountRows = table.Column<int>(type: "integer", nullable: false),
                    AmountColumns = table.Column<int>(type: "integer", nullable: false),
                    AmountDisplays = table.Column<int>(type: "integer", nullable: false),
                    CornersFormat = table.Column<string>(type: "text", nullable: false),
                    Resolution_Width = table.Column<double>(type: "double precision", nullable: false),
                    Resolution_Height = table.Column<double>(type: "double precision", nullable: false),
                    Format_Width = table.Column<double>(type: "double precision", nullable: false),
                    Format_Height = table.Column<double>(type: "double precision", nullable: false),
                    ScreenSize_Width = table.Column<double>(type: "double precision", nullable: false),
                    ScreenSize_Height = table.Column<double>(type: "double precision", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisplayTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Batches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DisplayTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayColor = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Cover = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Batches_DisplayTypes_DisplayTypeId",
                        column: x => x.DisplayTypeId,
                        principalTable: "DisplayTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Displays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Coordinates_X = table.Column<string>(type: "text", nullable: false),
                    Coordinates_Y = table.Column<string>(type: "text", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Color = table.Column<int>(type: "integer", nullable: false),
                    OriginalPhotoPath = table.Column<string>(type: "text", nullable: true),
                    CroppedPhotoPath = table.Column<string>(type: "text", nullable: true),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Displays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Displays_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Displays_DisplayTypes_DisplayTypeId",
                        column: x => x.DisplayTypeId,
                        principalTable: "DisplayTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Batches_DisplayTypeId",
                table: "Batches",
                column: "DisplayTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Displays_BatchId",
                table: "Displays",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Displays_DisplayTypeId",
                table: "Displays",
                column: "DisplayTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Displays");

            migrationBuilder.DropTable(
                name: "Batches");

            migrationBuilder.DropTable(
                name: "DisplayTypes");
        }
    }
}
