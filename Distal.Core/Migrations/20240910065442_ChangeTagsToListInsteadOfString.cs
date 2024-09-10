using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Distal.Core.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTagsToListInsteadOfString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tags",
                table: "MeshFiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "MeshFiles",
                type: "text",
                nullable: true);
        }
    }
}
