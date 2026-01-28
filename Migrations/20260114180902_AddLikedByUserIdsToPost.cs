using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.API.Migrations
{
    /// <inheritdoc />
    public partial class AddLikedByUserIdsToPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<Guid>>(
                name: "LikedByUserIds",
                table: "Posts",
                type: "uuid[]",
                nullable: true,
                defaultValueSql: "ARRAY[]::uuid[]");

            migrationBuilder.AddColumn<string>(
                name: "LikesData",
                table: "Posts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LikedByUserIds",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "LikesData",
                table: "Posts");
        }
    }
}
