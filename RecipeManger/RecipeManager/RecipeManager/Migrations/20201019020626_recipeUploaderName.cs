using Microsoft.EntityFrameworkCore.Migrations;

namespace RecipeManager.Migrations
{
    public partial class recipeUploaderName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UploaderName",
                table: "Recipes",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UploaderName",
                table: "Recipes");
        }
    }
}
