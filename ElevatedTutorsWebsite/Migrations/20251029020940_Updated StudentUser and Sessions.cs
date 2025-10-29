using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElevatedTutorsWebsite.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedStudentUserandSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalSessions",
                table: "Sessions");

            migrationBuilder.AddColumn<int>(
                name: "MaxSessions",
                table: "StudentUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxSessions",
                table: "StudentUsers");

            migrationBuilder.AddColumn<int>(
                name: "TotalSessions",
                table: "Sessions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
