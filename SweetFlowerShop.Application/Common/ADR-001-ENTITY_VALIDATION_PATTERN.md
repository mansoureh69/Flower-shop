# ADR-001: Entity Validation Before Aggregate Operations

## Status
**ACCEPTED** - 2025

## Decision
Establish a mandatory pattern requiring validation and loading of all related entities from repositories **before** performing aggregate operations (Create, Add, Update).

## Context

### Problem
Without consistent validation:
1. **Data Inconsistency** - Aggregates could reference deleted/unavailable entities
2. **Silent Failures** - Invalid operations might be persisted without error feedback
3. **Late Validation** - Errors discovered during persistence instead of upfront
4. **Primitive Parameters** - Passing IDs/amounts directly bypasses entity validation

### Example Issues

```csharp
// ❌ Without validation
order.AddItem(unknownProductId, "unknown", 0m, 0); // Silently fails validation rules

// ❌ Without validation
cart.AddItem(deletedProductId, quantity); // System allows deleted products in cart
```

## Solution

Implement **3-step validation pattern** in all command handlers:

1. **Load** - Fetch related entity from repository
2. **Validate** - Check null, soft-delete, business rules
3. **Delegate** - Pass entity object to aggregate method

## Example: PlaceOrderCommandHandler

```csharp
public async Task<Result<OrderResponse>> Handle(PlaceOrderCommand request, CancellationToken ct)
{
	var order = new Order(request.CustomerId, request.Notes);

	foreach (var item in request.Items)
	{
		// Step 1: Load
		var product = await productRepository.GetByIdAsync(item.ProductId, ct);

		// Step 2: Validate
		if (product is null || product.IsDeleted)
			return Result<OrderResponse>.Failure($"Product not found: {item.ProductId}");
		if (!product.IsAvailable)
			return Result<OrderResponse>.Failure($"Product unavailable: {product.Name}");

		// Step 3: Delegate to aggregate with entity object
		order.AddItem(product.Id, product, item.Quantity, item.Notes);
	}

	await orderRepository.AddAsync(order, ct);
	await unitOfWork.SaveChangesAsync(ct);
	return Result<OrderResponse>.Success(order.ToResponse());
}
```

## Key Design Points

### 1. Pass Entity Objects, Not Primitives

**Why?**
- Entity objects contain validation logic
- Single source of truth for entity data
- Prevents data skew between request and actual entity

```csharp
// ❌ Primitives
aggregate.AddItem(id, "name", 99.99m, qty);

// ✅ Entity object
aggregate.AddItem(entity.Id, entity, qty);
```

### 2. Use Result<> Pattern, Not Exceptions

**Why?**
- Validation failures are expected, not exceptional
- Exceptions indicate bugs; validation failures are normal API responses
- Result<> propagates to controller for HTTP response

```csharp
// ❌ Exception (escapes to middleware)
if (product is null) throw new ProductNotFoundException();

// ✅ Result<> (handled by handler)
if (product is null) return Result<TResponse>.Failure("Product not found");
```

### 3. Snapshot Prices for Immutable Orders

**Why?**
- Orders must reflect prices at order-time, not current prices
- Provides accurate audit trail
- Prevents disputes over price changes

```csharp
// OrderItem stores price snapshot
OrderItem: { UnitPrice = product.Price (at this moment) }
OrderTotal = immutable (won't change if product price changes)
```

## Affected Entities & Validation Rules

| Aggregate | Dependency | Validation Rules |
|-----------|------------|------------------|
| Order | Product | NOT NULL, NOT DELETED, IS AVAILABLE |
| Cart | Product | NOT NULL, NOT DELETED, IS AVAILABLE |
| Product | Category | NOT NULL, NOT DELETED |
| Payment | Order | NOT NULL, EXISTS, STATUS = CONFIRMED |

## Trade-offs

### Pros ✅
- **Data Integrity** - Prevents invalid state
- **Better UX** - Immediate, clear error messages
- **Testability** - Easy to mock repositories
- **Consistency** - Single pattern across all handlers
- **Audit Trail** - Snapshots create historical records

### Cons ❌
- **Additional Queries** - Each handler loads related entities (mitigated by caching)
- **More Code** - Validation logic in every handler (mitigated by templates)
- **Performance** - N+1 queries if not careful (mitigated by batching)

## Consequences

### Mandatory Changes
- [x] PlaceOrderCommandHandler - Validate products
- [x] AddToCartCommandHandler - Validate products  
- [x] CreateProductCommandHandler - Validate categories
- [ ] Future: Payment handlers, etc.

### Migration Path
```sql
-- New CartItem columns (Price snapshot)
ALTER TABLE CartItems 
ADD SnapshotPrice_Amount DECIMAL(18,2),
	SnapshotPrice_Currency NVARCHAR(3);
```

### Testing
All handlers must have tests for:
- ✅ Valid related entity → Success
- ✅ Null related entity → Failure
- ✅ Deleted related entity → Failure  
- ✅ Unavailable entity → Failure

## References
- **Pattern Guide:** `ENTITY_VALIDATION_PATTERN.md`
- **Quick Reference:** `RULE_VALIDATE_BEFORE_AGGREGATE.md`
- **Implementation Review:** `ENTITY_VALIDATION_IMPLEMENTATION_REVIEW.md`
- **CQRS Pattern:** CQRS + MediatR + FluentValidation (see copilot-instructions.md)

## Alternatives Considered

### Alternative 1: Validation in Aggregate Constructor
**Rejected:** Requires DI in aggregates (breaks domain model purity)

### Alternative 2: Custom Validation Behavior (Pipeline)
**Rejected:** Adds abstraction; explicit validation clearer for code review

### Alternative 3: Lazy Loading on Navigation
**Rejected:** Creates hidden queries; explicit loading more testable

## Approval
- **Date:** 2025
- **Decision Maker:** Architecture Team
- **Status:** MANDATORY for new handlers

---

## How to Apply This Decision

When implementing a new command handler:

```csharp
public sealed class YourCommandHandler(
	IYourRepository yourRepository,
	IRelatedRepository relatedRepository,  // ← Add this
	IUnitOfWork unitOfWork)
	: IRequestHandler<YourCommand, Result<YourResponse>>
{
	public async Task<Result<YourResponse>> Handle(YourCommand request, CancellationToken ct)
	{
		// 1. Load related entities
		var related = await relatedRepository.GetByIdAsync(request.RelatedId, ct);

		// 2. Validate
		if (related is null)
			return Result<YourResponse>.Failure("related not found");

		// 3. Create aggregate with validated entity
		var aggregate = new Your(related, ...);

		// 4. Persist & return
		await yourRepository.AddAsync(aggregate, ct);
		await unitOfWork.SaveChangesAsync(ct);
		return Result<YourResponse>.Success(aggregate.ToResponse());
	}
}
```

**See also:** RULE_VALIDATE_BEFORE_AGGREGATE.md for quick reference
