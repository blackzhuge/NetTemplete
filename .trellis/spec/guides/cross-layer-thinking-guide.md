# 跨层思维指南

> **目的**：实现前思考跨层数据流。

---

## 问题

**大多数 bug 发生在层边界**，而不是层内部。

常见跨层 bug：
- API 返回格式 A，前端期望格式 B
- 数据库存储 X，服务转换为 Y，但丢失了数据
- 多个层以不同方式实现相同逻辑

---

## 实现跨层功能前

### 步骤 1：映射数据流

画出数据流动路径：

```
来源 → 转换 → 存储 → 检索 → 转换 → 显示
```

对于每个箭头，问：
- 数据是什么格式？
- 可能出什么问题？
- 谁负责验证？

### 步骤 2：识别边界

| 边界 | 常见问题 |
|------|----------|
| API ↔ Service | 类型不匹配、缺少字段 |
| Service ↔ Database | 格式转换、空值处理 |
| Backend ↔ Frontend | 序列化、日期格式 |
| Component ↔ Component | Props 结构变化 |

### 步骤 3：定义契约

对于每个边界：
- 确切的输入格式是什么？
- 确切的输出格式是什么？
- 可能发生什么错误？

---

## 常见跨层错误

### 错误 1：隐式格式假设

**错误**：不检查就假设日期格式

**正确**：在边界处显式格式转换

### 错误 2：分散的验证

**错误**：在多个层验证同一件事

**正确**：在入口点验证一次

### 错误 3：泄漏的抽象

**错误**：组件知道数据库 schema

**正确**：每层只知道相邻层

### 错误 4：契约变更未同步测试（Critical）

**场景**：DTO 结构从扁平改为嵌套

```csharp
// 旧契约（扁平）
public sealed record Request { public string ProjectName { get; init; } }

// 新契约（嵌套）
public sealed record Request { public BasicOptions Basic { get; init; } }
public sealed record BasicOptions { public string ProjectName { get; init; } }
```

**错误**：只改 DTO，测试仍用旧结构 → 编译失败

```csharp
// ❌ 测试使用旧结构
var request = new Request { ProjectName = "Test" };  // 编译错误！
```

**正确**：契约变更时必须同步更新：
1. 所有调用点（UseCase、Modules）
2. 所有测试代码
3. 前端 API 调用

```csharp
// ✅ 测试辅助方法封装创建逻辑
private static GenerateScaffoldRequest CreateRequest(
    string projectName = "TestProject",
    DatabaseProvider database = DatabaseProvider.SQLite) =>
    new()
    {
        Basic = new() { ProjectName = projectName, Namespace = "Test" },
        Backend = new() { Database = database }
    };

// 测试使用辅助方法
var request = CreateRequest(projectName: "MyApp");
```

**检查清单**：
- [ ] Grep 搜索 `new Request` 找所有创建点
- [ ] 更新所有 Module 中的属性访问路径
- [ ] 更新测试的请求构造代码
- [ ] 验证前端 API 请求结构匹配

### 错误 5：前端硬编码与后端配置不一致（Critical）

**场景**：前端硬编码数据结构（如文件树、路径），与后端配置（如 manifest.json）各自维护。

```typescript
// ❌ 错误：前端硬编码路径，与后端 manifest 不一致
const files = [
  { name: 'SqlSugarSetup.cs', path: 'Extensions/SqlSugarSetup.cs' }  // 相对路径
]

// 后端 manifest.json 定义的是完整路径
// "output": "src/{{project_name}}.Api/Extensions/SqlSugarSetup.cs"
```

**后果**：
- 路径不匹配导致预览失败
- 前端显示的文件数量与实际生成不一致
- 后端新增文件，前端不知道

**正确**：
```typescript
// ✅ 方案 1：前端路径与后端 manifest 保持一致
const basePath = `src/${projectName}.Api/Extensions`
const files = [
  { name: 'SqlSugarSetup.cs', path: `${basePath}/SqlSugarSetup.cs` }
]

// ✅ 方案 2（更好）：从后端 API 获取文件树
const fileTree = await api.getFileTree(config)
```

**检查清单**：
- [ ] 前端显示的路径/结构是否与后端生成逻辑一致？
- [ ] 如果后端有 manifest/schema，前端是否同步？
- [ ] 修改后端配置时，是否有机制提醒前端同步？

---

## 跨层功能检查清单

实现前：
- [ ] 映射完整数据流
- [ ] 识别所有层边界
- [ ] 定义每个边界的格式
- [ ] 决定验证发生的位置

实现后：
- [ ] 用边界情况测试（null、空、无效）
- [ ] 验证每个边界的错误处理
- [ ] 检查数据往返是否完整

---

## 何时创建流程文档

在以下情况创建详细流程文档：
- 功能跨越 3+ 层
- 涉及多个团队
- 数据格式复杂
- 功能之前导致过 bug
