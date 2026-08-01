using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackMuvi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEpisodeWatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EpisodeWatches",
                columns: table => new
                {
                    TitleKey = table.Column<string>(type: "TEXT", nullable: false),
                    SeasonNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    EpisodeNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    WatchedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpisodeWatches", x => new { x.TitleKey, x.SeasonNumber, x.EpisodeNumber });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EpisodeWatches");
        }
    }
}
