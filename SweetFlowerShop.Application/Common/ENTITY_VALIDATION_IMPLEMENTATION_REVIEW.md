# Entity Validation Pattern - Implementation Review

## Summary of Changes

This document provides a review of how the **"Validate Before Aggregate Action"** pattern has been applied across the codebase.

---

## Pattern Implementation Status

### ✅ IMPLEMENTED: PlaceOrderCommandHandler

**File:** `SweetFlowerShop.Application/Features/Orders/PlaceOrder/PlaceOrderCommandHandler.cs`

**Pattern Applied:**
```csharp
foreach (var item in request.Items)
{
	// Validate product
	var product = await productRepository.GetByIdAsync(item.ProductId, cancellationToken);
	if (product is null || product.IsDeleted)
		return Result<OrderResponse>.Failure($"Product not found: {item.ProductId}");

	if (!product.IsAvailable)
		return Result<OrderResponse>.Failure($"Product is not available: {product.Name}");

	// Pass entity object
	order.AddItem(product.Id, product, item.Quantity, item.Notes);
}
```

**Changes:**
- ✅ `Order.AddItem()` updated to accept `Product` object instead of primitives
- ✅ `OrderItem` constructor validates and stores product details
- ✅ Price snapshotted at order time for consistency

**Benefits:**
- Validates product availability before order item creation
- Ensures data consistency
- Proper error handling with Result<>

---

### ✅ IMPLEMENTED: AddToCartCommandHandler

**File:** `SweetFlowerShop.Application/Features/Carts/AddToCart/AddToCartCommandHandler.cs`

**Pattern Applied:**
```csharp
// Validate product
var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
if (product is null || product.IsDeleted)
	return Result<CartResponse>.Failure($"Product not found: {request.ProductId}");

if (!product.IsAvailable)
	return Result<CartResponse>.Failure($"Product is not available: {product.Name}");

// Pass entity object
cart.AddItem(product.Id, product, request.Quantity);
```

**Related Changes:**
- **`Cart.AddItem()`** - Updated signature:
  - From: `AddItem(Guid productId, int quantity)`
  - To: `AddItem(Guid productId, Product product, int quantity)`

- **`CartItem` entity** - Enhanced with:
  - `Product` reference for navigation
  - `SnapshotPrice` (Money value object) - captures price at cart add time
  - Constructor now accepts `Product` object and validates

- **`CartItemConfiguration`** - Updated EF Core mapping:
  - Configured `SnapshotPrice` as complex property (Money)
  - Handles Amount and Currency fields

- **`CartResponse` DTO** - Updated to include:
  - `ProductName` - for display in API responses
  - `SnapshotPrice` - the price at time of cart addition

**Benefits:**
- Validates product before adding to cart
- Price consistency - cart shows price at add time, not current price
- Better API response includes product name and price

---

### ✅ IMPLEMENTED: CreateProductCommandHandler

**File:** `SweetFlowerShop.Application/Features/Products/CreateProduct/CreateProductCommandHandler.cs`

**Pattern Applied:**
```csharp
// Validate related entity (Category)
var category = await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
if (category is null)
	return Result<ProductResponse>.Failure("Category not found.");

// Create product (CategoryId already validated)
var product = new Product(
	request.Name,
	request.Description,
	new Money(request.Price, request.Currency),
	request.CategoryId);
```

**Key Points:**
- ✅ Category validation before product creation
- ✅ Failure result instead of exception
- ✅ Product aggregate ensures validity

---

## Related Entity Validation Matrix

| Aggregate | Related Entity | Validation Level | Status |
|-----------|----------------|-----------------|--------|
| **Order** | Product | Exists, Not Deleted, IsAvailable | ✅ IMPLEMENTED |
| **Order** | Customer | Implicit (valid CustomerId) | ✅ IMPLEMENTED |
| **Cart** | Product | Exists, Not Deleted, IsAvailable | ✅ IMPLEMENTED |
| **Product** | Category | Exists, Not Null | ✅ IMPLEMENTED |
| **Payment** | Order | Exists, Status = Confirmed | ⏳ TODO (future) |
| **CartItem** | Product | Snapshot + eager load via Product nav | ✅ IMPLEMENTED |

---

## Key Design Decisions

### 1. **Entity Objects vs Primitives**
```csharp
// ❌ Old way (fragile)
order.AddItem(productId, productName, unitPrice, quantity);

// ✅ New way (safe, consistent)
order.AddItem(product.Id, product, quantity, notes);
```

**Why:** 
- Single source of truth for product data
- Validation happens in entity constructor
- Price/details snapshotted at operation time

### 2. **Price Snapshots in Carts & Orders**

**OrderItem:**
- Stores Product reference + snapshot in `UnitPrice` (Money)
- Ensures order total cannot change due to price updates

**CartItem:**
- Stores Product reference + snapshot in `SnapshotPrice` (Money)
- Ensures cart display shows add-time prices
- Uses current product price only at checkout

### 3. **Failure Results vs Exceptions**

```csharp
// ❌ Old way
if (product is null)
	throw new ProductNotFoundException();

// ✅ New way
if (product is null)
	return Result<TResponse>.Failure("Product not found");
```

**Why:**
- Exception handling belongs in middleware, not handlers
- Result<> pattern allows API to return proper error responses
- Handlers focus on orchestration, not error routing

---

## Migration Path for Remaining Handlers

If new command handlers are added for other entities:

### Template: Entity Validation Pattern

```csharp
public sealed class Create[Entity]CommandHandler(
	I[Entity]Repository [entity]Repository,
	I[RelatedEntity]Repository [relatedEntity]Repository,
	IUnitOfWork unitOfWork)
	: IRequestHandler<Create[Entity]Command, Result<[Entity]Response>>
{
	public async Task<Result<[Entity]Response>> Handle(Create[Entity]Command request, CancellationToken cancellationToken)
	{
		// Step 1: Validate all related entities
		var relatedEntity = await [relatedEntity]Repository.GetByIdAsync(request.[RelatedEntity]Id, cancellationToken);
		if (relatedEntity is null || relatedEntity.IsDeleted)
			return Result<[Entity]Response>.Failure($"[RelatedEntity] not found: {request.[RelatedEntity]Id}");

		// Step 2: Check business rules
		if (!relatedEntity.IsAvailable)
			return Result<[Entity]Response>.Failure($"[RelatedEntity] is not available");

		// Step 3: Create aggregate passing validated entity
		var [entity] = new [Entity](
			request.OtherField,
			relatedEntity,  // Pass entity object
			request.AnotherField);

		// Step 4: Persist and return
		await [entity]Repository.AddAsync([entity], cancellationToken);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		return Result<[Entity]Response>.Success([entity].ToResponse());
	}
}
```

---

## Database Migration Required

A new migration is needed for CartItem changes:

```bash
dotnet ef migrations add UpdateCartItemWithSnapshot --project SweetFlowerShop.Infrastructure --startup-project Flower-shop.Server
dotnet ef database update --project SweetFlowerShop.Infrastructure --startup-project Flower-shop.Server
```

**Migration will:**
- Remove old `Price` column
- Add `SnapshotPrice_Amount` (decimal)
- Add `SnapshotPrice_Currency` (string)
- Add `Product_Id` foreign key (nullable, for navigation)

---

## Testing Considerations

### Unit Tests - OrderPlacementService
```csharp
[Fact]
public async Task AddToOrder_WithInvalidProduct_ReturnFailure()
{
	// Arrange
	var invalidProductId = Guid.NewGuid();
	mockProductRepository.Setup(x => x.GetByIdAsync(invalidProductId, It.IsAny<CancellationToken>()))
		.ReturnsAsync((Product?)null);

	// Act
	var result = await handler.Handle(new PlaceOrderCommand(...), CancellationToken.None);

	// Assert
	Assert.False(result.IsSuccess);
	Assert.Contains("Product not found", result.Error);
}
```

### Integration Tests - AddToCart
```csharp
[Fact]
public async Task AddToCart_WithUnavailableProduct_ReturnFailure()
{
	// Arrange
	var unavailableProduct = new Product(...);
	unavailableProduct.Deactivate();
	await productRepository.AddAsync(unavailableProduct);

	// Act
	var result = await handler.Handle(new AddToCartCommand(...), CancellationToken.None);

	// Assert
	Assert.False(result.IsSuccess);
	Assert.Contains("not available", result.Error);
}
```

---

## Checklist for Code Review

When reviewing new handlers, verify:

- [ ] All related entities loaded from repositories before use
- [ ] Null checks on loaded entities
- [ ] Soft-delete checks (`IsDeleted == false`)
- [ ] Business state checks (Availability, Status, etc.)
- [ ] Uses `Result<>.Failure()` instead of throwing exceptions
- [ ] Passes complete entity objects to aggregate methods
- [ ] Price/details snapshotted where applicable
- [ ] Handler acts as thin orchestrator only
- [ ] DTOs include all necessary fields for API response

---

## References

- [Entity Validation Pattern Documentation](./ENTITY_VALIDATION_PATTERN.md)
- Related PR: Product validation in carts and orders
- Copilot Instructions: `CQRS + MediatR + FluentValidation` pattern

---

**Last Updated:** 2025  
**Reviewed By:** Architecture  
**Status:** COMPLETED
