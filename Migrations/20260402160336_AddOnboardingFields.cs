using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cleo.Migrations
{
    /// <inheritdoc />
    public partial class AddOnboardingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserNotes");

            migrationBuilder.AddColumn<string>(
                name: "AgeGroup",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CycleLength",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PeriodLength",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastActive",
                value: new DateTime(2026, 4, 2, 16, 3, 32, 491, DateTimeKind.Utc).AddTicks(9033));

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastActive",
                value: new DateTime(2026, 4, 2, 16, 3, 32, 491, DateTimeKind.Utc).AddTicks(9038));

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastActive",
                value: new DateTime(2026, 4, 2, 16, 3, 32, 491, DateTimeKind.Utc).AddTicks(9040));

            migrationBuilder.UpdateData(
                table: "Articles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PublishDate",
                value: new DateTime(2026, 4, 2, 16, 3, 32, 491, DateTimeKind.Utc).AddTicks(9872));

            migrationBuilder.UpdateData(
                table: "Articles",
                keyColumn: "Id",
                keyValue: 2,
                column: "PublishDate",
                value: new DateTime(2026, 4, 2, 16, 3, 32, 492, DateTimeKind.Utc).AddTicks(73));

            migrationBuilder.UpdateData(
                table: "Articles",
                keyColumn: "Id",
                keyValue: 3,
                column: "PublishDate",
                value: new DateTime(2026, 4, 2, 16, 3, 32, 492, DateTimeKind.Utc).AddTicks(75));

            migrationBuilder.UpdateData(
                table: "Articles",
                keyColumn: "Id",
                keyValue: 4,
                column: "PublishDate",
                value: new DateTime(2026, 4, 2, 16, 3, 32, 492, DateTimeKind.Utc).AddTicks(142));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgeGroup",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CycleLength",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PeriodLength",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "UserNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotes", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastActive",
                value: new DateTime(2026, 4, 2, 13, 33, 46, 599, DateTimeKind.Utc).AddTicks(3474));

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastActive",
                value: new DateTime(2026, 4, 2, 13, 33, 46, 599, DateTimeKind.Utc).AddTicks(3479));

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastActive",
                value: new DateTime(2026, 4, 2, 13, 33, 46, 599, DateTimeKind.Utc).AddTicks(3481));

            migrationBuilder.UpdateData(
                table: "Articles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PublishDate",
                value: new DateTime(2026, 4, 2, 13, 33, 46, 599, DateTimeKind.Utc).AddTicks(3583));

            migrationBuilder.UpdateData(
                table: "Articles",
                keyColumn: "Id",
                keyValue: 2,
                column: "PublishDate",
                value: new DateTime(2026, 4, 2, 13, 33, 46, 599, DateTimeKind.Utc).AddTicks(3588));

            migrationBuilder.UpdateData(
                table: "Articles",
                keyColumn: "Id",
                keyValue: 3,
                column: "PublishDate",
                value: new DateTime(2026, 4, 2, 13, 33, 46, 599, DateTimeKind.Utc).AddTicks(3589));

            migrationBuilder.UpdateData(
                table: "Articles",
                keyColumn: "Id",
                keyValue: 4,
                column: "PublishDate",
                value: new DateTime(2026, 4, 2, 13, 33, 46, 599, DateTimeKind.Utc).AddTicks(3590));
        }
    }
}
