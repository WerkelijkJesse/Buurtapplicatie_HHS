using Microsoft.EntityFrameworkCore.Migrations;

namespace Buurt_interactie_app_Semester3_WDPR.Migrations
{
    public partial class b : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Pseudonyms",
                table: "Pseudonyms");

            migrationBuilder.AlterColumn<string>(
                name: "SudoId",
                table: "Pseudonyms",
                nullable: true,
                defaultValueSql: "newsequentialid()",
                oldClrType: typeof(string),
                oldType: "varchar(255) CHARACTER SET utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pseudonyms",
                table: "Pseudonyms",
                column: "AnoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Pseudonyms",
                table: "Pseudonyms");

            migrationBuilder.AlterColumn<string>(
                name: "SudoId",
                table: "Pseudonyms",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pseudonyms",
                table: "Pseudonyms",
                columns: new[] { "AnoId", "SudoId" });
        }
    }
}
