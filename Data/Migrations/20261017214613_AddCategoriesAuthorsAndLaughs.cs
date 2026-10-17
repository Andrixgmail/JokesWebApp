using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JokesWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoriesAuthorsAndLaughs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorId",
                table: "Joke",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorName",
                table: "Joke",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Joke",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Joke",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<int>(
                name: "Likes",
                table: "Joke",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Views",
                table: "Joke",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Joke_AuthorId",
                table: "Joke",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Joke_Category",
                table: "Joke",
                column: "Category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Joke_AuthorId",
                table: "Joke");

            migrationBuilder.DropIndex(
                name: "IX_Joke_Category",
                table: "Joke");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Joke");

            migrationBuilder.DropColumn(
                name: "AuthorName",
                table: "Joke");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Joke");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Joke");

            migrationBuilder.DropColumn(
                name: "Likes",
                table: "Joke");

            migrationBuilder.DropColumn(
                name: "Views",
                table: "Joke");
        }
    }
}
