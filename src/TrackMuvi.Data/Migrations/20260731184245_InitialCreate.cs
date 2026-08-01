using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackMuvi.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonalStatuses",
                columns: table => new
                {
                    TitleKey = table.Column<string>(type: "TEXT", nullable: false),
                    Want = table.Column<bool>(type: "INTEGER", nullable: false),
                    Watched = table.Column<bool>(type: "INTEGER", nullable: false),
                    Favorite = table.Column<bool>(type: "INTEGER", nullable: false),
                    Following = table.Column<bool>(type: "INTEGER", nullable: false),
                    Pending = table.Column<bool>(type: "INTEGER", nullable: false),
                    Abandoned = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalStatuses", x => x.TitleKey);
                });

            migrationBuilder.CreateTable(
                name: "ViewHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TitleKey = table.Column<string>(type: "TEXT", nullable: false),
                    WatchedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewHistory", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ViewHistory_TitleKey",
                table: "ViewHistory",
                column: "TitleKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonalStatuses");

            migrationBuilder.DropTable(
                name: "ViewHistory");
        }
    }
}
