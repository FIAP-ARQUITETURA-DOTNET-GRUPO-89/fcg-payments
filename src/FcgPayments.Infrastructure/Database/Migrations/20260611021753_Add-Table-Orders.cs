using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FcgPayments.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddTableOrders : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Orders",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Customer = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false),
                TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                Cep = table.Column<string>(type: "character varying(8)", unicode: false, maxLength: 8, nullable: false),
                City = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false),
                State = table.Column<string>(type: "character varying(2)", unicode: false, maxLength: 2, nullable: false),
                Street = table.Column<string>(type: "character varying(150)", unicode: false, maxLength: 150, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Orders", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Orders");
    }
}
