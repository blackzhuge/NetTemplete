# 数据库规范

> 本项目的数据库模式和约定。

---

## 概述

**ORM**：SqlSugar 5.1.4.169
**支持的数据库**：SQLite（开发）、MySQL（生产）、SQL Server（企业版）
**包管理**：通过 `Directory.Packages.props` 集中管理

```xml
<!-- src/Directory.Packages.props -->
<PackageVersion Include="SqlSugarCore" Version="5.1.4.169" />
```

---

## 实体定义

### 模式：实体类

实体应定义在 `Domain/Entities/` 文件夹中（待创建）。

```csharp
// 示例模式（待实现）
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

### 实体约定

| 规则 | 约定 |
|------|------|
| 类修饰符 | `sealed` |
| 主键 | `Id`（long，自增） |
| 必填字段 | 使用 `required` 关键字 |
| 可空字段 | 使用可空类型（`?`） |
| 时间戳 | `CreatedAt`、`UpdatedAt` |

---

## Repository 模式

### 接口定义（Application 层）

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

### 实现（Infrastructure 层）

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
            .ExecuteReturnSnowflakeIdAsync();  // 或 ExecuteReturnIdentityAsync()
    }
}
```

---

## 查询模式

### 简单查询

```csharp
// 单个实体
var user = await _db.Queryable<User>()
    .Where(u => u.Id == id)
    .FirstAsync(ct);

// 带条件的列表
var users = await _db.Queryable<User>()
    .Where(u => u.Status == UserStatus.Active)
    .OrderBy(u => u.CreatedAt, OrderByType.Desc)
    .ToListAsync(ct);
```

### 分页

```csharp
var (list, total) = await _db.Queryable<User>()
    .Where(u => u.Status == UserStatus.Active)
    .ToPageListAsync(pageIndex, pageSize, ct);
```

### 联表查询

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

## 事务

### 单操作（自动事务）

```csharp
// SqlSugar 自动处理单操作
await _db.Insertable(entity).ExecuteCommandAsync(ct);
```

### 多操作（显式事务）

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

## 迁移

SqlSugar 支持 Code-First 迁移：

```csharp
// 在 Program.cs 或启动代码中
_db.CodeFirst.InitTables<User, Order, OrderItem>();
```

**最佳实践**：仅在开发环境运行迁移。生产环境使用 SQL 脚本。

---

## 命名约定

| 类型 | 约定 | 示例 |
|------|------|------|
| 表名 | snake_case，复数 | `users`、`order_items` |
| 列名 | snake_case | `created_at`、`user_id` |
| 主键 | `id` | `id` |
| 外键 | `{table}_id` | `user_id` |
| 索引名 | `ix_{table}_{columns}` | `ix_users_email` |
| 唯一约束 | `uq_{table}_{columns}` | `uq_users_username` |

---

## 常见错误

| 错误 | 正确做法 |
|------|----------|
| N+1 查询 | 使用 `.Includes()` 预加载 |
| 缺少 `CancellationToken` | 始终传递 `ct` 给异步方法 |
| 简单查询使用原生 SQL | 优先使用 Queryable API |
| 忘记事务 | 多个写操作包装在事务中 |
| 查询所有列 | 使用 `.Select()` 进行投影 |

---

## 状态

> **注意**：数据库层尚未在代码库中实现。本规范定义了实现时的预期模式。
