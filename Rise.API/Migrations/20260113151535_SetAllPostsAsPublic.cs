using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.API.Migrations
{
    /// <inheritdoc />
    public partial class SetAllPostsAsPublic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Posts\" SET \"IsPublic\" = true;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Posts\" SET \"IsPublic\" = false;");
        }
    }
}
