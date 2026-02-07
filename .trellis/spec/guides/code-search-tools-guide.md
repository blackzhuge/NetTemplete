# 代码搜索工具指南

> **目的**：选择正确的工具进行代码搜索和导航，提高效率和准确性。

---

## 工具选择矩阵

| 场景 | 推荐工具 | 原因 |
|------|----------|------|
| **语义化代码理解** | `mcp__ace-tool__search_context` | 自然语言查询，理解代码意图 |
| **精确符号查找** | LSP (`goToDefinition`, `findReferences`) | 类型感知，跨文件追踪 |
| **精确字符串/正则匹配** | `Grep` | 快速全文搜索 |
| **文件名模式匹配** | `Glob` | 按路径/扩展名查找文件 |
| **深度代码库探索** | `Task` + `subagent_type=Explore` | 多轮搜索，复杂问题 |

---

## LSP 工具详解

### 支持的语言

| 语言 | LSP 服务器 | 状态 |
|------|-----------|------|
| TypeScript/JavaScript | typescript-lsp | ✅ 开箱即用 |
| C# | csharp-ls + 适配器 | ✅ 需要适配器脚本 |

### LSP 操作一览

| 操作 | 用途 | 示例场景 |
|------|------|----------|
| `documentSymbol` | 获取文件内所有符号 | 了解类/函数结构 |
| `hover` | 查看符号类型信息 | 快速了解变量类型 |
| `goToDefinition` | 跳转到定义 | 查找接口/类定义 |
| `findReferences` | 查找所有引用 | 重构前影响分析 |
| `goToImplementation` | 查找接口实现 | 找到具体实现类 |
| `workspaceSymbol` | 跨文件搜索符号 | 找特定类/函数 |

### 何时用 LSP

✅ **适合场景**：
- 查找函数/类/接口的定义
- 查找符号的所有引用
- 重构前分析影响范围
- 大型项目的精确导航

❌ **不适合场景**：
- 全文搜索（用 Grep）
- 搜索注释/字符串内容（用 Grep）
- 理解整体架构（用 ace-tool）
- 查看文件结构（用 Glob）

### C# LSP 配置（csharp-ls 适配器）

由于 Claude Code LSP 客户端不支持某些标准方法，需要透明适配器：

```bash
# 1. 安装 csharp-ls
dotnet tool install -g csharp-ls --version 0.15.0

# 2. 备份原始二进制
mv ~/.dotnet/tools/csharp-ls ~/.dotnet/tools/csharp-ls-original

# 3. 创建适配器脚本（需要 Bun）
# 脚本拦截以下方法并返回 null：
# - window/workDoneProgress/create
# - window/workDoneProgress/cancel
# - client/registerCapability

# 4. 回滚方法
mv ~/.dotnet/tools/csharp-ls-original ~/.dotnet/tools/csharp-ls
```

---

## ace-tool 语义搜索

### 何时使用

✅ **适合场景**：
- 不知道具体文件位置
- 需要理解代码功能/流程
- 自然语言描述的问题
- 探索性搜索

### 查询技巧

**推荐格式**：自然语言描述 + 可选关键词

```
# 好的查询
"用户认证的处理逻辑在哪里？Keywords: auth, login, jwt"
"文件上传时分片合并的实现。Keywords: upload chunk merge"

# 不好的查询（太短/太抽象）
"auth"
"upload"
```

### 与 Grep 的区别

| 特性 | ace-tool | Grep |
|------|----------|------|
| 查询方式 | 自然语言 | 正则表达式 |
| 结果类型 | 语义相关代码 | 精确匹配行 |
| 适用场景 | 探索性搜索 | 精确定位 |
| 速度 | 较慢（需要语义分析） | 快速 |

---

## 搜索策略决策树

```
开始搜索
    │
    ├─ 知道确切的符号名？
    │   ├─ 是 → LSP findReferences / goToDefinition
    │   └─ 否 → 继续
    │
    ├─ 知道确切的字符串/模式？
    │   ├─ 是 → Grep
    │   └─ 否 → 继续
    │
    ├─ 需要按文件名/路径查找？
    │   ├─ 是 → Glob
    │   └─ 否 → 继续
    │
    ├─ 是探索性问题（不知道在哪）？
    │   ├─ 是 → ace-tool search_context
    │   └─ 否 → 继续
    │
    └─ 需要多轮深度搜索？
        └─ 是 → Task + Explore agent
```

---

## 常见错误

### 错误 1：用 Grep 做语义搜索

**问题**：
```bash
# 搜索"错误处理"但不知道具体实现方式
grep -r "error" .  # 结果太多，噪音大
```

**正确**：
```
ace-tool: "错误处理的统一逻辑在哪里？Keywords: error handler exception"
```

### 错误 2：用 ace-tool 做精确匹配

**问题**：
```
ace-tool: "GenerateScaffoldUseCase"  # 浪费时间
```

**正确**：
```bash
Grep: pattern="GenerateScaffoldUseCase"
# 或
LSP: findReferences on GenerateScaffoldUseCase
```

### 错误 3：不用 LSP 做重构分析

**问题**：
```bash
# 想改函数签名，用 Grep 找引用
grep -r "functionName" .  # 可能漏掉别名、接口引用
```

**正确**：
```
LSP: findReferences on functionName  # 类型感知，更准确
```

---

## 提交前检查清单

- [ ] 使用了正确的搜索工具
- [ ] LSP 用于精确符号操作
- [ ] ace-tool 用于语义理解
- [ ] Grep 用于精确文本匹配
- [ ] 重构前用 LSP 分析了所有引用
