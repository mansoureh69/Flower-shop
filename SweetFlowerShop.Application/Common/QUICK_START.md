# 🚀 QUICK START: Entity Validation Pattern

## What Is This?

A **mandatory rule** for all command handlers in the Flower Shop application:

### ⚡ The Rule
**Validate related entities from repositories BEFORE aggregate operations**

---

## 3-Line Version

1. Load entity: `var product = await repo.GetByIdAsync(id, ct);`
2. Validate: `if (product is null || !product.IsAvailable) return Failure("...");`
3. Pass to aggregate: `aggregate.AddItem(product.Id, product, qty);`

---

## Template for New Handlers

Copy-paste this template when creating a new command handler:

```csharp
public sealed class YourCommandHandler(
	IYourRepository repository,
	IRelatedRepository relatedRepository,  // ← New dependency
	IUnitOfWork unitOfWork)
	: IRequestHandler<YourCommand, Result<YourResponse>>
{
	public async Task<Result<YourResponse>> Handle(
		YourCommand request, 
		CancellationToken cancellationToken)
	{
		// ✅ STEP 1: Load related entity
		var related = await relatedRepository.GetByIdAsync(
			request.RelatedId, 
			cancellationToken);

		// ✅ STEP 2: Validate it
		if (related is null || related.IsDeleted)
			return Result<YourResponse>.Failure("Related not found");

		if (!related.IsAvailable)
			return Result<YourResponse>.Failure("Related not available");

		// ✅ STEP 3: Create aggregate with validated entity
		var aggregate = new Your(related, request.OtherField);

		// ✅ STEP 4: Persist & return
		await repository.AddAsync(aggregate, cancellationToken);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		return Result<YourResponse>.Success(aggregate.ToResponse());
	}
}
```

---

## Real Examples in This Codebase

### ✅ PlaceOrderCommandHandler
```csharp
// Load product
var product = await productRepository.GetByIdAsync(item.ProductId, ct);

// Validate
if (product is null || product.IsDeleted)
	return Failure($"Product not found");
if (!product.IsAvailable)
	return Failure($"Product unavailable");

// Delegate to aggregate
order.AddItem(product.Id, product, item.Quantity, item.Notes);
```

### ✅ AddToCartCommandHandler
```csharp
// Load product
var product = await productRepository.GetByIdAsync(
	request.ProductId, 
	cancellationToken);

// Validate
if (product is null || product.IsDeleted)
	return Failure($"Product not found");
if (!product.IsAvailable)
	return Failure($"Product not available");

// Delegate to aggregate
cart.AddItem(product.Id, product, request.Quantity);
```

### ✅ CreateProductCommandHandler
```csharp
// Load category (related entity)
var category = await categoryRepository.GetByIdAsync(
	request.CategoryId, 
	cancellationToken);

// Validate
if (category is null)
	return Failure("Category not found");

// Create aggregate
var product = new Product(
	request.Name,
	request.Description,
	new Money(request.Price, request.Currency),
	request.CategoryId);
```

---

## Checklist for PR Review

When reviewing a new handler:

```
☐ Does it load related entities from repositories?
☐ Does it check for null?
☐ Does it check for IsDeleted?
☐ Does it validate business rules (IsAvailable, etc.)?
☐ Does it use Result<>.Failure() (not throw)?
☐ Does it pass entity objects to aggregate (not IDs)?
☐ Does it have tests for validation failures?
☐ Is the handler a thin orchestrator (not doing domain logic)?
```

---

## Common Mistakes ❌

### ❌ WRONG: No validation
```csharp
order.AddItem(productId, quantity); // ← What if product doesn't exist?
```

### ✅ RIGHT: Validate first
```csharp
var product = await productRepository.GetByIdAsync(productId, ct);
if (product is null) return Failure("Product not found");
order.AddItem(product.Id, product, quantity);
```

---

### ❌ WRONG: Throw exception
```csharp
if (product is null)
	throw new ProductNotFoundException(); // ← Exception handling is for bugs
```

### ✅ RIGHT: Return Result
```csharp
if (product is null)
	return Result<Response>.Failure("Product not found"); // ← Normal flow
```

---

### ❌ WRONG: Pass primitives
```csharp
order.AddItem(productId, "Name", 99.99m, qty); // ← How do we validate name/price?
```

### ✅ RIGHT: Pass entity
```csharp
order.AddItem(product.Id, product, qty); // ← All validation in entity
```

---

## When to Apply This Rule

Apply this pattern to **every handler** that:
- Creates an aggregate
- Adds an item to an aggregate
- Modifies an aggregate state

**Bottom line:** If you load a related entity from a repository, validate it first.

---

## Price Snapshots 💰

Both **OrderItem** and **CartItem** capture product prices at operation time:

```
When adding to Order:
  OrderItem stores UnitPrice = product.Price (at that moment)
  Order total is immutable (won't change if product price changes)

When adding to Cart:
  CartItem stores SnapshotPrice = product.Price (at add time)
  Checkout uses current product price
  Cart display shows add-time prices
```

**Why?** Prevents disputes and ensures audit trails are accurate. ✅

---

## Where Are the Rules?

📄 **Full documentation in:**
- `SweetFlowerShop.Application/Common/RULE_VALIDATE_BEFORE_AGGREGATE.md` ← Start here!
- `SweetFlowerShop.Application/Common/ENTITY_VALIDATION_PATTERN.md` ← Detailed guide
- `SweetFlowerShop.Application/Common/ADR-001-ENTITY_VALIDATION_PATTERN.md` ← Architecture decision

---

## Questions?

1. **"Should I validate in the aggregate?"**  
   No, validate in the handler. Aggregates validate domain rules (quantity > 0), handlers validate entity existence.

2. **"Do I need to load the entity if I already have its ID?"**  
   Yes. ID alone isn't enough—you need to validate state (IsDeleted, IsAvailable).

3. **"Can I skip validation for some commands?"**  
   No. This is mandatory for all handlers.

4. **"What if the aggregate method internally loads the entity?"**  
   Don't do that. Breaks encapsulation and makes testing harder.

---

## TL;DR

```
Every command handler must:
1. Load related entities from repositories
2. Validate (null, IsDeleted, business rules)
3. Return Failure if invalid
4. Pass entity objects (not IDs) to aggregates
5. Use Result<> for all errors
```

**That's it!** 🎉

Copy the template above and you're good to go.
