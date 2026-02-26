using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhishingDetector.API.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailAnalyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Sender = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Headers = table.Column<string>(type: "text", nullable: false),
                    Urls = table.Column<string>(type: "text", nullable: false),
                    OverallRisk = table.Column<string>(type: "text", nullable: false),
                    UrlRiskScore = table.Column<int>(type: "integer", nullable: false),
                    HeaderRiskScore = table.Column<int>(type: "integer", nullable: false),
                    ContentRiskScore = table.Column<int>(type: "integer", nullable: false),
                    OverallRiskScore = table.Column<int>(type: "integer", nullable: false),
                    UrlAnalysis = table.Column<string>(type: "text", nullable: false),
                    HeaderAnalysis = table.Column<string>(type: "text", nullable: false),
                    ContentAnalysis = table.Column<string>(type: "text", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    Indicators = table.Column<string>(type: "text", nullable: false),
                    AnalyzedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessingTimeMs = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAnalyses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailAnalyses_AnalyzedAt",
                table: "EmailAnalyses",
                column: "AnalyzedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmailAnalyses_OverallRisk",
                table: "EmailAnalyses",
                column: "OverallRisk");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailAnalyses");
        }
    }
}
