using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CategoriesService.Migrations
{
    /// <inheritdoc />
    public partial class v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanhMucs",
                columns: table => new
                {
                    IDDanhMuc = table.Column<int>(type: "int", nullable: false),
                    TenDanhMuc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CapDanhMuc = table.Column<int>(type: "int", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    IsLeaf = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucs", x => x.IDDanhMuc);
                });

            migrationBuilder.CreateTable(
                name: "SanPhams",
                columns: table => new
                {
                    IDSanPham = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IDDanhMuc = table.Column<int>(type: "int", nullable: false),
                    IDCuaHang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenSanPham = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhams", x => x.IDSanPham);
                });

            migrationBuilder.CreateTable(
                name: "HinhAnhDanhMucs",
                columns: table => new
                {
                    IDHinhAnhDanhMuc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IDDanhMuc = table.Column<int>(type: "int", nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HinhAnhDanhMucs", x => x.IDHinhAnhDanhMuc);
                    table.ForeignKey(
                        name: "FK_HinhAnhDanhMucs_DanhMucs_IDDanhMuc",
                        column: x => x.IDDanhMuc,
                        principalTable: "DanhMucs",
                        principalColumn: "IDDanhMuc",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhDanhMucs_IDDanhMuc",
                table: "HinhAnhDanhMucs",
                column: "IDDanhMuc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HinhAnhDanhMucs");

            migrationBuilder.DropTable(
                name: "SanPhams");

            migrationBuilder.DropTable(
                name: "DanhMucs");
        }
    }
}
