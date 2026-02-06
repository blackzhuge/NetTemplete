# Database Guidelines

> Database patterns and conventions for this project.

---

## Overview

**ORM**: SqlSugar 5.1.4.169
**Supported Databases**: SQLite (dev), MySQL (prod), SQL Server (enterprise)
**Package Management**: Central package management via `Directory.Packages.props`

```xml
<!-- src/Directory.Packages.props -->
<PackageVersion Include="SqlSugarCore" Version="5.1.4.169" />
```

---

## Entity Definition

### Pattern: Entity Class

Entities should be defined in a `Domain/Entities/` folder (to be created).

```csharp
// Example pattern (to be implemented)
namespace ScaffoldGenerator.Domain.Entities;

[SugarTable("users")]
public sealed class User
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    [SugarColumn(Length = 50, IsNullable = false)]
    public required string Username { get; set; }

    [SugarColumn(Length = 100)]
    public string? Email { get; set; }

    [SugarColumn(InsertServerTime = true)]
    public DateTime CreatedAt { get; set; }

    [SugarColumn(UpdateServerTime = true)]
    public DateTime? UpdatedAt { get; set; }
}
```

### Entity Conventions

| Rule | Convention |
|------|------------|
| Class modifier | `sealed` |
| Primary key | `Id` (long, auto-increment) |
| Required fields | Use `required` keyword |
| Nullable fields | Use nullable type (`?`) |
| Timestamps | `CreatedAt`, `UpdatedAt` |

---

## Repository Pattern

### Interface Definition (Application Layer)

```csharp
// Application/Abstractions/IUserRepository.cs
namespace ScaffoldGenerator.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default);
    Task<long> CreateAsync(User entity, CancellationToken ct = default);
    Task UpdateAsync(User entity, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
}
```

### Implementation (Infrastructure Layer)

```csharp
// Infrastructure/Repositories/UserRepository.cs
namespace ScaffoldGenerator.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ISqlSugarClient _db;

    public UserRepository(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<User?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await _db.Queryable<User>()
            .Where(u => u.Id == id)
            .FirstAsync(ct);
    }

    public async Task<long> CreateAsync(User entity, CancellationToken ct = default)
    {
        return await _db.Insertable(entity)
            .ExecuteReturnSnowflakeIdAsync();  // or ExecuteReturnIdentityAsync()
    }
}
```

---

## Query Patterns

### Simple Query

```csharp
// Single entity
var user = await _db.Queryable<User>()
    .Where(u => u.Id == id)
    .FirstAsync(ct);

// List with conditions
var users = await _db.Queryable<User>()
    .Where(u => u.Status == UserStatus.Active)
    .OrderBy(u => u.CreatedAt, OrderByType.Desc)
    .ToListAsync(ct);
```

### Pagination

```csharp
var (list, total) = await _db.Queryable<User>()
    .Where(u => u.Status == UserStatus.Active)
    .ToPageListAsync(pageIndex, pageSize, ct);
```

### Join Query

```csharp
var result = await _db.Queryable<Order, User>((o, u) => o.UserId == u.Id)
    .Select((o, u) => new OrderDto
    {
        OrderId = o.Id,
        Username = u.Username,
        Amount = o.Amount
    })
    .ToListAsync(ct);
```

---

## Transactions

### Single Operation (Auto Transaction)

```csharp
// SqlSugar handles single operations automatically
await _db.Insertable(entity).ExecuteCommandAsync(ct);
```

### Multiple Operations (Explicit Transaction)

```csharp
try
{
    await _db.Ado.BeginTranAsync();

    await _db.Insertable(order).ExecuteCommandAsync(ct);
    await _db.Insertable(orderItems).ExecuteCommandAsync(ct);
    await _db.Updateable(inventory).ExecuteCommandAsync(ct);

    await _db.Ado.CommitTranAsync();
}
catch
{
    await _db.Ado.RollbackTranAsync();
    throw;
}
```

---

## Migrations

SqlSugar supports code-first migrations:

```csharp
// In Program.cs or startup
_db.CodeFirst.InitTables<User, Order, OrderItem>();
```

**Best Practice**: Run migrations only in development. Use SQL scripts for production.

---

## Naming Conventions

| Type | Convention | Example |
|------|------------|---------|
| Table name | snake_case, plural | `users`, `order_items` |
| Column name | snake_case | `created_at`, `user_id` |
| Primary key | `id` | `id` |
| Foreign key | `{table}_id` | `user_id` |
| Index name | `ix_{table}_{columns}` | `ix_users_email` |
| Unique constraint | `uq_{table}_{columns}` | `uq_users_username` |

---

## Common Mistakes

| Mistake | Correct Approach |
|---------|------------------|
| N+1 queries | Use `.Includes()` for eager loading |
| Missing `CancellationToken` | Always pass `ct` to async methods |
| Raw SQL for simple queries | Use Queryable API first |
| Forgetting transactions | Wrap multiple writes in transaction |
| Selecting all columns | Use `.Select()` for projections |

---

## Status

> **Note**: Database layer is not yet implemented in the codebase. This guideline defines the expected patterns when implementing.
