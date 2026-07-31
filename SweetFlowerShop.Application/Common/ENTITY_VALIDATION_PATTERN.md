# Entity Validation Pattern - CQRS Handler Rule

## Overview
This document establishes the pattern for validating related entities before creation or modification in command handlers. This ensures data consistency, aggregate integrity, and proper error handling at the application layer.

## Pattern Rule: "Validate Before Aggregate Action"

### Core Principle
**ALWAYS validate and load related entities from repositories BEFORE performing aggregate operations (Create, Add, Update).**

### Pattern Structure

```csharp
// ❌ WRONG: Direct aggregate operation without validation
order.AddItem(productId, productName, price, quantity);

// ✅ CORRECT: Validate related entity first, then pass entity object
var product = await productRepository.GetByIdAsync(item.ProductId, cancellationToken);
if (product is null || product.IsDeleted)
	return Result<OrderResponse>.Failure($"Product not found: {item.ProductId}");

if (!product.IsAvailable)
	return Result<OrderResponse>.Failure($"Product is not available: {product.Name}");

order.AddItem(product.Id, product, item.Quantity, item.Notes);
```

## Implementation Guidelines

### 1. **Load Related Entities First**
   - Fetch from repository before aggregate operation
   - Check for null/deleted status
   - Validate business rules (availability, status, etc.)

### 2. **Return Failure on Invalid State**
   - Don't throw exceptions in handlers
   - Use `Result<TResponse>.Failure(message)` for validation failures
   - Provide descriptive error messages for API consumers

### 3. **Pass Entity Objects, Not Primitives**
   - ❌ Don't: `aggregate.AddItem(id, name, price, quantity)`
   - ✅ Do: `aggregate.AddItem(id, relatedEntity, quantity, notes)`
   - Benefits:
	 - Snapshot prices/details at order time (data consistency)
	 - Leverage entity validation in aggregate
	 - Single source of truth

### 4. **Aggregate Methods Handle Domain Logic**
   - Aggregate validates quantity, status, business rules
   - Keep handlers orchestrators only
   - Handlers: Load → Validate → Delegate to aggregate → Persist

## Applied Examples

### Example 1: PlaceOrderCommandHandler ✅
```csharp
foreach (var item in request.Items)
{
	// Step 1: Load related entity
	var product = await productRepository.GetByIdAsync(item.ProductId, cancellationToken);

	// Step 2: Validate state
	if (product is null || product.IsDeleted)
		return Result<OrderResponse>.Failure($"Product not found: {item.ProductId}");
	if (!product.IsAvailable)
		return Result<OrderResponse>.Failure($"Product is not available: {product.Name}");

	// Step 3: Pass entity object to aggregate
	order.AddItem(product.Id, product, item.Quantity, item.Notes);
}
```

### Example 2: CreateProductCommandHandler ✅
```csharp
// Step 1: Load related entity
var category = await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);

// Step 2: Validate state
if (category is null)
	return Result<ProductResponse>.Failure("Category not found.");

// Step 3: Create aggregate with validated dependency
var product = new Product(
	request.Name,
	request.Description,
	new Money(request.Price, request.Currency),
	request.CategoryId); // CategoryId already validated
```

### Example 3: AddToCartCommandHandler ❌ (Needs Update)
**Current Issue:** No product validation before adding to cart
```csharp
// ❌ CURRENT (Invalid)
cart.AddItem(request.ProductId, request.Quantity);

// ✅ SHOULD BE
var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
if (product is null || product.IsDeleted)
	return Result<CartResponse>.Failure($"Product not found: {request.ProductId}");

if (!product.IsAvailable)
	return Result<CartResponse>.Failure($"Product is not available.");

cart.AddItem(product.Id, product, request.Quantity);
```

## Validation Checklist for Command Handlers

- [ ] All related entity references are loaded from repositories
- [ ] Null checks on loaded entities
- [ ] Soft-delete checks (IsDeleted == false)
- [ ] Availability/status checks where applicable
- [ ] Failure results returned instead of exceptions
- [ ] Entity objects passed to aggregate methods (not primitives)
- [ ] Handlers act as orchestrators only
- [ ] Aggregate methods contain domain logic

## Benefits of This Pattern

| Benefit | Explanation |
|---------|-------------|
| **Data Consistency** | Prices/details snapshotted at operation time |
| **Aggregate Integrity** | Related entities validated before aggregate state change |
| **Error Handling** | Proper Result<> pattern for API responses |
| **Encapsulation** | Aggregate owns business logic, handler is a thin orchestrator |
| **Maintainability** | Consistent pattern across all handlers |
| **Testing** | Easier to mock repositories and test scenarios |

## Anti-Patterns to Avoid

❌ **Don't load entity inside aggregate method**
```csharp
// BAD: Breaks aggregate encapsulation
public void AddItem(Guid productId)
{
	var product = _productRepository.GetById(productId); // NO!
}
```

❌ **Don't pass only IDs and load lazily**
```csharp
// BAD: Deferred validation
order.AddItem(productId); // Validation happens later = late errors
```

❌ **Don't throw exceptions in handlers**
```csharp
// BAD: Exceptions escape to controller
if (product is null)
	throw new ProductNotFoundException(); // NO!

// GOOD: Use Result<>
if (product is null)
	return Result<TResponse>.Failure("Product not found");
```

## Related Entities Requiring Validation

| Aggregate | Related Entity | Validation |
|----------|----------------|-----------|
| **Order** | Product | Exists, Not Deleted, IsAvailable |
| **Order** | Customer | Exists, Not Deleted (implicit via CustomerId) |
| **Cart** | Product | Exists, Not Deleted, IsAvailable |
| **Product** | Category | Exists, Not Deleted |
| **Payment** | Order | Exists, Status = Confirmed |

---

**Last Updated:** 2025  
**Enforced By:** Architecture Review  
**Pattern Status:** MANDATORY for all command handlers
