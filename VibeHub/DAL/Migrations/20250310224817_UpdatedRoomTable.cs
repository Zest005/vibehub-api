using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedRoomTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MusicIds",
                table: "Rooms");

            migrationBuilder.AddColumn<Guid>(
                name: "RoomId",
                table: "Musics",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Musics_RoomId",
                table: "Musics",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Musics_Rooms_RoomId",
                table: "Musics",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Musics_Rooms_RoomId",
                table: "Musics");

            migrationBuilder.DropIndex(
                name: "IX_Musics_RoomId",
                table: "Musics");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "Musics");

            migrationBuilder.AddColumn<List<Guid>>(
                name: "MusicIds",
                table: "Rooms",
                type: "uuid[]",
                nullable: true);
        }
    }
}
