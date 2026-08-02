using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SweetFlowerShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CorrectOrderLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "Orders"
                SET "Status" = 'PendingPayment'
                WHERE "Status" = 'Pending';

                UPDATE "Orders"
                SET "Status" = 'Preparing'
                WHERE "Status" = 'Processing';

                UPDATE "Orders"
                SET "Status" = 'OutForDelivery'
                WHERE "Status" = 'Shipped';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "Orders"
                SET "Status" = 'Pending'
                WHERE "Status" = 'PendingPayment';

                UPDATE "Orders"
                SET "Status" = 'Processing'
                WHERE "Status" IN ('Preparing', 'ReadyForDelivery');

                UPDATE "Orders"
                SET "Status" = 'Shipped'
                WHERE "Status" = 'OutForDelivery';
                """);
        }
    }
}
