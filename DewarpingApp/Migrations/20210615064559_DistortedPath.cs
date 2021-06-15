using Microsoft.EntityFrameworkCore.Migrations;

namespace DewarpingApp.Migrations
{
    public partial class DistortedPath : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DistortedPath",
                table: "ImageFiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistortedPath",
                table: "ImageFiles");
        }
    }
}
