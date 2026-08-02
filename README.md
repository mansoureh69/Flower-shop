# Sweet Flower Shop

A modern full-stack flower storefront built with Angular and ASP.NET Core. The project pairs a polished, responsive shopping experience with a domain-driven backend for products, carts, orders, payments, customers, and authentication.

## Highlights

- Responsive storefront with occasion, featured-product, delivery, brand-story, and testimonial sections
- Product browsing with category and availability filters
- JWT-based registration and login
- Authenticated shopping carts and order placement
- Role-protected product creation for administrators
- PostgreSQL persistence through Entity Framework Core
- Domain events, value objects, audit fields, and soft deletion
- CQRS request handling with MediatR and validation with FluentValidation
- Domain and application unit tests with xUnit

## Tech stack

| Area | Technology |
| --- | --- |
| Frontend | Angular 21, TypeScript 5.9, RxJS, CSS |
| API | ASP.NET Core 10, C# |
| Application | MediatR, FluentValidation, CQRS |
| Data | PostgreSQL, Entity Framework Core 10, Npgsql |
| Security | ASP.NET Core Identity, JWT bearer authentication |
| Testing | xUnit, Vitest, Angular testing utilities, axe-core |

## Architecture

The backend follows Clean Architecture, keeping business rules independent from frameworks and infrastructure:

```text
flower-shop.client/              Angular storefront
Flower-shop.Server/              ASP.NET Core API and composition root
SweetFlowerShop.Application/     Use cases, commands, queries, validation
SweetFlowerShop.Domain/          Entities, value objects, events, business rules
SweetFlowerShop.Infrastructure/  EF Core, Identity, JWT, repositories, migrations
tests/                           Domain and application tests
```

Dependencies point inward: the API composes the application and infrastructure layers, the application depends on the domain, and the domain remains framework-independent.

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js](https://nodejs.org/) with npm 10 or later
- [PostgreSQL](https://www.postgresql.org/download/)
- EF Core CLI tools (`dotnet tool install --global dotnet-ef`) if you need to manage migrations

### 1. Clone and restore

```bash
git clone <your-repository-url>
cd flower-Shop
dotnet restore Flower-shop.slnx
npm install --prefix flower-shop.client
```

### 2. Configure local secrets

Copy the example configuration:

```powershell
Copy-Item Flower-shop.Server/appsettings.Local.example.json Flower-shop.Server/appsettings.Local.json
```

Then add your PostgreSQL connection and JWT settings to `Flower-shop.Server/appsettings.Local.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=flower_shop;Username=postgres;Password=your_password"
  },
  "JwtSettings": {
    "Secret": "replace-with-a-random-secret-at-least-32-characters-long",
    "Issuer": "SweetFlowerShop",
    "Audience": "SweetFlowerShopClient",
    "ExpirationInMinutes": 60
  }
}
```

`appsettings.Local.json` is ignored by Git. For shared or hosted environments, use environment variables or a secrets manager instead of committing credentials.

### 3. Create the database

```bash
dotnet ef database update --startup-project Flower-shop.Server --project SweetFlowerShop.Infrastructure
```

### 4. Run the application

Start the API from the repository root:

```bash
dotnet run --project Flower-shop.Server
```

In Development, the ASP.NET Core SPA proxy starts the Angular client automatically. The default addresses are:

- Storefront: `https://localhost:3697`
- API: `https://localhost:7185`
- OpenAPI document: `https://localhost:7185/openapi/v1.json`

If the local HTTPS certificate is not trusted yet, run:

```bash
dotnet dev-certs https --trust
```

To run the frontend separately, use `npm start --prefix flower-shop.client` while the API is running.

## API overview

| Method | Endpoint | Access | Purpose |
| --- | --- | --- | --- |
| `POST` | `/api/auth/register` | Public | Create an account |
| `POST` | `/api/auth/login` | Public | Receive a JWT |
| `GET` | `/api/products` | Public | List and filter products |
| `POST` | `/api/products` | Admin | Create a product |
| `POST` | `/api/carts/items` | Authenticated | Add an item to a cart |
| `POST` | `/api/orders` | Authenticated | Place an order |

Send authenticated requests with an `Authorization: Bearer <token>` header.

## Common commands

```bash
# Build the complete solution
dotnet build Flower-shop.slnx

# Run backend tests
dotnet test Flower-shop.slnx

# Build the Angular client
npm run build --prefix flower-shop.client

# Run frontend tests
npm test --prefix flower-shop.client
```

## Current status

The repository includes the core storefront UI and backend flows for authentication, products, carts, and orders. Additional commerce screens and production concerns, such as checkout UI, payment-provider integration, administration, deployment automation, and end-to-end tests, can be built on the existing architecture.

## Contributing

Contributions are welcome. Fork the repository, create a focused branch, add tests where appropriate, and open a pull request describing the change and how it was verified.

## License

No license has been added yet. If you plan to make this repository open source, add a license before accepting external contributions.
