# Migration & Deployment Commands

## Pre-Deployment Verification

### 1. Verify Build (Already Done ✅)
```bash
dotnet build Flower-shop.slnx
# Result: Build successful
```

### 2. Verify Migration File Created
```bash
# Check migration file exists
ls SweetFlowerShop.Infrastructure/Migrations/20260802081804_AddOrderItemSnapshot.cs

# View migration details
cat SweetFlowerShop.Infrastructure/Migrations/20260802081804_AddOrderItemSnapshot.cs
```

### 3. Review Migration SQL (Optional)
```bash
dotnet ef migrations script -f 20260727091717 -t 20260802081804 `
  -s Flower-shop.Server -p SweetFlowerShop.Infrastructure
```

---

## Development Environment

### Apply Migration to Development Database

```bash
cd D:\Projects\source\flower-Shop

# Option 1: Using PowerShell
dotnet ef database update `
  -s Flower-shop.Server `
  -p SweetFlowerShop.Infrastructure

# Option 2: Using Command Line
dotnet ef database update ^
  -s Flower-shop.Server ^
  -p SweetFlowerShop.Infrastructure
```

### Verify Migration Applied
```bash
# Check migration history in database
SELECT migration, applied_on FROM __EFMigrationsHistory 
ORDER BY applied_on DESC 
LIMIT 5;

# Expected output includes:
# | 20260802081804_AddOrderItemSnapshot | 2026-08-02 HH:MM:SS... |
```

### Verify Table Structure
```bash
-- Check OrderItems table columns
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'OrderItems'
ORDER BY ordinal_position;

-- Expected new columns:
-- | UnitPrice_Currency | character varying | NO  |
-- | Notes              | character varying | YES |
```

---

## Rollback (If Needed)

### Remove Last Migration (Development Only)
```bash
cd D:\Projects\source\flower-Shop

dotnet ef migrations remove `
  -s Flower-shop.Server `
  -p SweetFlowerShop.Infrastructure
```

**Important**: This removes the migration file. You'll need to regenerate if needed.

---

## Staging Environment

### 1. Create Migration Script for Staging
```bash
cd D:\Projects\source\flower-Shop

# Generate SQL script to apply migration
dotnet ef migrations script -f 20260727091717 `
  -s Flower-shop.Server `
  -p SweetFlowerShop.Infrastructure `
  -o .\migration_20260802_for_staging.sql

# Review the generated SQL
cat .\migration_20260802_for_staging.sql
```

### 2. Apply to Staging Database
```bash
# Option A: Using dotnet ef (if DbContext can connect to staging)
dotnet ef database update `
  --connection "Server=staging-db-server;Database=flowerShop_staging;..." `
  -s Flower-shop.Server `
  -p SweetFlowerShop.Infrastructure

# Option B: Run generated SQL script manually (safer)
# Copy migration_20260802_for_staging.sql to staging server
# Execute via SQL client (psql for PostgreSQL, SqlCmd for SQL Server, etc.)
```

### 3. Smoke Test on Staging
```bash
# Test order placement with new snapshot logic
curl -X POST https://staging-api.flowerShop.com/api/orders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
	"customerId": "00000000-0000-0000-0000-000000000001",
	"items": [
	  {
		"productId": "00000000-0000-0000-0000-000000000002",
		"quantity": 2,
		"notes": "Red roses, gift wrap"
	  }
	],
	"notes": "Deliver by Friday"
  }'

# Expected response includes:
# "items": [{
#   "productId": "...",
#   "productName": "Rose Bouquet",
#   "unitPrice": 49.99,
#   "currency": "USD",        # NEW FIELD
#   "quantity": 2,
#   "totalPrice": 99.98
# }]
```

---

## Production Environment

### Pre-Production Checklist
- [ ] All tests pass (unit + integration)
- [ ] Staging smoke tests successful
- [ ] Security review completed
- [ ] Database backup scheduled
- [ ] Rollback plan verified
- [ ] Change log documented
- [ ] On-call engineer aware

### 1. Backup Production Database
```bash
# PostgreSQL example
pg_dump -h prod-db.server.com -U admin flowerShop > backup_20260802.sql

# SQL Server example
sqlcmd -S prod-db.server.com -U admin -d flowerShop -Q "BACKUP DATABASE flowerShop TO DISK='backup_20260802.bak'"
```

### 2. Generate Production Migration Script
```bash
dotnet ef migrations script -f 20260727091717 `
  -s Flower-shop.Server `
  -p SweetFlowerShop.Infrastructure `
  -o .\migration_20260802_production.sql

# Review and validate SQL
# Apply only during maintenance window
```

### 3. Apply Migration to Production
```bash
# Option A: Via DbContext (requires secure connection string)
dotnet ef database update `
  -s Flower-shop.Server `
  -p SweetFlowerShop.Infrastructure

# Option B: Via SQL script (recommended for production)
# 1. Extract server-specific connection from appsettings
# 2. Connect to production database
# 3. Execute migration_20260802_production.sql
# 4. Verify __EFMigrationsHistory table updated
```

### 4. Verify Migration in Production
```bash
-- Check migration applied
SELECT migration, applied_on FROM __EFMigrationsHistory 
WHERE migration LIKE '%AddOrderItemSnapshot%';

-- Verify table structure
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'OrderItems'
  AND column_name IN ('UnitPrice_Currency', 'Notes', 'ProductName')
ORDER BY ordinal_position;

-- Sample query: orders with snapshots
SELECT o.Id, o.OrderDate, oi.ProductName, oi.UnitPrice, oi.UnitPrice_Currency, oi.Quantity
FROM Orders o
JOIN OrderItems oi ON o.Id = oi.OrderId
WHERE o.OrderDate > DATE_TRUNC('hour', NOW())
LIMIT 5;
```

### 5. Monitor Production (First Hour)
```bash
-- Check for errors in order placement
SELECT COUNT(*) as failed_orders
FROM Orders o
WHERE o.OrderDate > NOW() - INTERVAL '1 hour'
  AND o.Status = 'Error';  -- or your error status

-- Check for data quality issues
SELECT COUNT(*) as missing_currency
FROM OrderItems
WHERE UnitPrice_Currency IS NULL;

-- Check performance
SELECT query, mean_exec_time, calls
FROM pg_stat_statements
WHERE query LIKE '%OrderItems%'
ORDER BY calls DESC
LIMIT 10;
```

### 6. Rollback Plan (Emergency Only)
```bash
-- If migration causes critical issues in production:

-- Option 1: Revert to previous EF Core model and remove migration
# This requires code rollback as well
# Only if new code has critical bug

-- Option 2: Restore from backup (safest)
# Stop application
# Restore database from backup_20260802.sql
# Restart application with previous release

-- Option 3: Drop columns (minimal impact)
ALTER TABLE OrderItems DROP COLUMN UnitPrice_Currency;
ALTER TABLE OrderItems DROP COLUMN Notes;
-- Delete from __EFMigrationsHistory
DELETE FROM __EFMigrationsHistory 
WHERE migration = '20260802081804_AddOrderItemSnapshot';
```

---

## Connection String Configuration

### Development (appsettings.Development.json)
```json
{
  "ConnectionStrings": {
	"FlowerShopDb": "Host=localhost;Database=flowerShop_dev;Username=dev;Password=devpass"
  }
}
```

### Staging (appsettings.Staging.json)
```json
{
  "ConnectionStrings": {
	"FlowerShopDb": "Host=staging-db.internal;Database=flowerShop_staging;Username=staging_user;Password=[secure_password]"
  }
}
```

### Production (appsettings.Production.json or Azure Key Vault)
```json
{
  "ConnectionStrings": {
	"FlowerShopDb": "Host=prod-db.internal;Database=flowerShop;Username=prod_user;Password=[secure_password]"
  }
}
```

---

## Entity Framework Core Commands Reference

### View Available Migrations
```bash
dotnet ef migrations list -s Flower-shop.Server -p SweetFlowerShop.Infrastructure
```

### Create New Migration (if more changes needed)
```bash
dotnet ef migrations add <MigrationName> `
  -s Flower-shop.Server `
  -p SweetFlowerShop.Infrastructure `
  -o SweetFlowerShop.Infrastructure/Migrations
```

### Revert to Specific Migration
```bash
dotnet ef database update <PreviousMigrationName> `
  -s Flower-shop.Server `
  -p SweetFlowerShop.Infrastructure
```

### Validate DbContext Against Database
```bash
# Check if model is in sync with migrations
dotnet build && dotnet ef dbcontext info `
  -s Flower-shop.Server `
  -p SweetFlowerShop.Infrastructure
```

---

## CI/CD Pipeline Integration

### GitHub Actions Example
```yaml
name: Deploy Migration

on:
  push:
	branches: [ main ]

jobs:
  migrate:
	runs-on: ubuntu-latest
	steps:
	  - uses: actions/checkout@v2

	  - name: Setup .NET
		uses: actions/setup-dotnet@v1
		with:
		  dotnet-version: '10.0.x'

	  - name: Restore dependencies
		run: dotnet restore

	  - name: Build
		run: dotnet build --configuration Release

	  - name: Apply Migration (Dev)
		run: |
		  dotnet ef database update \
			-s Flower-shop.Server \
			-p SweetFlowerShop.Infrastructure
		env:
		  ConnectionStrings__FlowerShopDb: ${{ secrets.DEV_DB_CONNECTION }}

	  - name: Run Tests
		run: dotnet test --configuration Release

	  - name: Deploy to Staging (on success)
		if: success()
		run: echo "Trigger staging deployment"
```

---

## Troubleshooting

### Migration Not Found
```
Error: The migration '20260802081804_AddOrderItemSnapshot' has not been applied to the database.
```
**Solution**: 
```bash
# Check available migrations
dotnet ef migrations list -s Flower-shop.Server -p SweetFlowerShop.Infrastructure

# Apply all pending migrations
dotnet ef database update -s Flower-shop.Server -p SweetFlowerShop.Infrastructure
```

### Connection String Issues
```
Error: Unable to connect to database
```
**Solution**:
```bash
# Test connection using EF
dotnet ef dbcontext info \
  -s Flower-shop.Server \
  -p SweetFlowerShop.Infrastructure \
  --connection "Host=...;Database=...;Username=...;Password=..."
```

### Concurrent Migration Issues
```
Error: Migration already applied or in progress
```
**Solution**:
- Ensure only one instance applying migrations
- Check __EFMigrationsHistory for locks
- If stuck, manually cleanup and retry

### Column Already Exists
```
Error: Column 'UnitPrice_Currency' already exists
```
**Solution**: Migration already applied. Check __EFMigrationsHistory:
```bash
SELECT * FROM __EFMigrationsHistory 
WHERE migration = '20260802081804_AddOrderItemSnapshot';
```

---

## Summary

| Environment | Command | Notes |
|---|---|---|
| **Dev** | `dotnet ef database update` | Direct application |
| **Staging** | `dotnet ef migrations script` then SQL execution | Safer for testing |
| **Production** | SQL script during maintenance window | Manual verification required |

All commands assume:
- Working directory: `D:\Projects\source\flower-Shop`
- Startup project: `-s Flower-shop.Server`
- Infrastructure project: `-p SweetFlowerShop.Infrastructure`

Ready for deployment! ✅
