using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackMuvi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTitleCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TitleCache",
                columns: table => new
                {
                    TitleKey = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    PosterPath = table.Column<string>(type: "TEXT", nullable: true),
                    ReleaseDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    NextEpisodeSeason = table.Column<int>(type: "INTEGER", nullable: true),
                    NextEpisodeNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    NextEpisodeName = table.Column<string>(type: "TEXT", nullable: true),
                    NextEpisodeAirDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    LastSyncedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TitleCache", x => x.TitleKey);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TitleCache");
        }
    }
}
