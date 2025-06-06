using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagement.Web.Migrations
{
    /// <inheritdoc />
    public partial class ChangeInBookTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TatalCopies",
                table: "Books",
                newName: "TotalCopies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalCopies",
                table: "Books",
                newName: "TatalCopies");
        }
    }
}
