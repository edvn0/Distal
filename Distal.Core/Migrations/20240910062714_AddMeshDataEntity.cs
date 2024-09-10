using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Distal.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddMeshDataEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "MeshFiles");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "MeshFiles");

            migrationBuilder.CreateTable(
                name: "MeshData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false),
                    MeshFileId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeshData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeshData_MeshFiles_MeshFileId",
                        column: x => x.MeshFileId,
                        principalTable: "MeshFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeshData_MeshFileId",
                table: "MeshData",
                column: "MeshFileId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeshData");

            migrationBuilder.AddColumn<byte[]>(
                name: "Content",
                table: "MeshFiles",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "Size",
                table: "MeshFiles",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
