# ⚡ Entity Validation Pattern - Quick Reference

## The Rule (TL;DR)

> **ALWAYS validate and load related entities from repositories BEFORE performing aggregate operations.**

## The Checklist

```
For every command handler that creates or modifies an aggregate:

☐ Load all related entities from repositories
☐ Check if entity is null → Failure("Entity not found")
☐ Check if entity.IsDeleted → Failure("Entity not found")  
☐ Check business rules (IsAvailable, Status, etc.) → Failure("...")
☐ Pass ENTITY OBJECTS to aggregate methods (not IDs or primitives)
☐ Use Result<>.Failure() for validation errors
☐ Aggregate methods handle domain logic validation
```

## Before & After Examples

### Example 1: Adding to Order ❌ → ✅

```csharp
// ❌ WRONG - No validation, passing primitives
order.AddItem(productId, "Product Name", 99.99m, 2);

// ✅ CORRECT - Validate first, pass entity
var product = await productRepository.GetByIdAsync(productId, ct);
if (product is null || product.IsDeleted)
	return Result<OrderResponse>.Failure("Product not found");
if (!product.IsAvailable)
	return Result<OrderResponse>.Failure("Product unavailable");

order.AddItem(product.Id, product, 2, notes);
```

### Example 2: Adding to Cart ❌ → ✅

```csharp
// ❌ WRONG - No product validation
var cart = await cartRepository.GetByCustomerIdAsync(customerId, ct);
cart.AddItem(productId, quantity);

// ✅ CORRECT - Validate product first
var product = await productRepository.GetByIdAsync(productId, ct);
if (product is null || product.IsDeleted)
	return Result<CartResponse>.Failure("Product not found");
if (!product.IsAvailable)
	return Result<CartResponse>.Failure("Product unavailable");

var cart = await cartRepository.GetByCustomerIdAsync(customerId, ct);
if (cart is null) cart = new Cart(customerId);
cart.AddItem(product.Id, product, quantity);
```

### Example 3: Creating Product ❌ → ✅

```csharp
// ❌ WRONG - No category validation
var product = new Product(name, description, price, categoryId);

// ✅ CORRECT - Validate category first
var category = await categoryRepository.GetByIdAsync(categoryId, ct);
if (category is null)
	return Result<ProductResponse>.Failure("Category not found");

var product = new Product(name, description, price, categoryId);
```

## The Pattern in 4 Steps

```csharp
public async Task<Result<ResponseType>> Handle(Command request, CancellationToken ct)
{
	// STEP 1: LOAD related entities
	var relatedEntity = await repository.GetByIdAsync(request.RelatedId, ct);

	// STEP 2: VALIDATE loaded entities
	if (relatedEntity is null || relatedEntity.IsDeleted)
		return Result<ResponseType>.Failure("Not found");
	if (!relatedEntity.IsAvailable) 
		return Result<ResponseType>.Failure("Not available");

	// STEP 3: CREATE aggregate & ADD items (passing entity objects)
	var aggregate = new Aggregate(...);
	aggregate.AddItem(relatedEntity.Id, relatedEntity, ...);

	// STEP 4: PERSIST & RETURN
	await repository.AddAsync(aggregate, ct);
	await unitOfWork.SaveChangesAsync(ct);
	return Result<ResponseType>.Success(aggregate.ToResponse());
}
```

## Why This Pattern?

| Reason | Explanation |
|--------|-------------|
| **Data Safety** | Validate before state changes |
| **Consistency** | Pass entity objects, not primitives—no stale/inconsistent data |
| **Error Handling** | Use Result<> pattern, not exceptions |
| **Encapsulation** | Aggregate validates domain rules, handler orchestrates |
| **Testing** | Easy to mock: just mock repository returns |

## Entities That Require Validation

| Entity | Check |
|--------|-------|
| **Product** | `is null`, `IsDeleted`, `IsAvailable` |
| **Category** | `is null`, `IsDeleted` |
| **Order** | `is null`, `Status` |
| **Cart** | Create if null, otherwise load |
| **Customer** | `is null`, `IsDeleted` |

## Anti-Patterns ❌

```csharp
// ❌ Throw exceptions in handlers
if (product is null) throw new ProductNotFoundException();

// ❌ Lazy-load entities inside aggregate
public void AddItem(Guid id) {
	var product = _repo.GetById(id); // NO!
}

// ❌ Skip validation, trust the request
cart.AddItem(request.ProductId, request.Quantity); // NO validation!

// ❌ Pass only IDs, validate later
order.AddItem(productId); // Validation deferred = late errors
```

## FYI: Price Snapshots

**OrderItem & CartItem** both capture prices at operation time:

```csharp
// When adding to order
OrderItem stores: UnitPrice (Money) = product.Price at that moment
// Order total = SUM(OrderItem.UnitPrice * Quantity) — immutable

// When adding to cart
CartItem stores: SnapshotPrice (Money) = product.Price at add time
// Cart shows add-time prices for display
// Checkout uses current product price
```

This ensures orders don't change if product prices change later! ✅

---

**For complete documentation:** See `ENTITY_VALIDATION_PATTERN.md`  
**For implementation review:** See `ENTITY_VALIDATION_IMPLEMENTATION_REVIEW.md`
