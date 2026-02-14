# Scaffold Core Hardening

## Context

脚手架生成器的核心承诺是"生成后可直接运行"。当前非 SqlSugar ORM 组合、Dapper 驱动依赖、前端测试路径、前端 lint 闭环四个方面存在稳定性缺口，导致生成的项目无法编译或无法执行基础质量命令。

本次变更聚焦修复这四个 P0 问题，不引入任何新功能。

---

## Constraints

### Hard Constraints

1. **HC-1**: 所有 ORM x Database 组合（4 ORM x 3 DB = 12 种）生成的项目必须 `dotnet build` 通过
2. **HC-2**: 修复不得破坏现有 SqlSugar 组合的正常生成
3. **HC-3**: 前端测试配置文件必须落在与 `package.json` 同级目录下
4. **HC-4**: 生成的前端项目 `pnpm install && pnpm lint` 必须零错误退出
5. **HC-5**: 最小改动原则——只修复问题，不重构、不扩展

### Soft Constraints

1. **SC-1**: 遵循现有模块模式（OrmModule/FrontendModule 的代码风格）
2. **SC-2**: 包版本与项目中已有的版本声明保持一致
3. **SC-3**: eslint 配置应与 web-configurator 的 `eslint.config.js` 风格对齐（flat config）

---

## Requirements

### REQ-1: Non-SqlSugar ORM 命名空间可见性

**问题根因**: `Program.cs.sbn` 第 1 行仅 `using {{ namespace }}.Api.Extensions;`，但 EFCore/Dapper/FreeSql 的 Setup 类位于 `{{ namespace }}.Api.Setup` 命名空间。扩展方法 `AddEFCore()`、`AddDapper()`、`AddFreeSql()` 对 Program.cs 不可见，编译失败。

**影响范围**: ORM = EFCore / Dapper / FreeSql 的所有 9 种组合

**修复方案**: 在 `Program.cs.sbn` 中，当 ORM 非 SqlSugar 时，添加 `using {{ namespace }}.Api.Setup;`

**验收**:
- `dotnet build` 通过：EFCore + SQLite/MySQL/SQLServer
- `dotnet build` 通过：Dapper + SQLite/MySQL/SQLServer
- `dotnet build` 通过：FreeSql + SQLite/MySQL/SQLServer
- SqlSugar 组合不受影响

### REQ-2: Dapper 数据库驱动依赖补全

**问题根因**: `OrmModule.AddDapperFiles()` 和 `Api.csproj.sbn` 的 Dapper 分支仅添加 `Dapper` 包，未添加数据库驱动包。`DapperSetup.cs.sbn` 模板中使用了 `Microsoft.Data.Sqlite`、`MySqlConnector`、`Microsoft.Data.SqlClient`，但这些包未被引入。

**对比**: EFCore 和 FreeSql 均按 Database 类型添加了对应 Provider 包。

**影响范围**: ORM = Dapper 的所有 3 种数据库组合

**修复方案**:
- `Api.csproj.sbn` Dapper 分支：按 `db_type` 添加驱动包
- `OrmModule.AddDapperFiles()`：按 `request.Backend.Database` 添加驱动包
- 驱动映射：SQLite → `Microsoft.Data.Sqlite`，MySQL → `MySqlConnector`，SQLServer → `Microsoft.Data.SqlClient`

**验收**:
- Dapper + SQLite 生成项目 `dotnet build` 通过
- Dapper + MySQL 生成项目 `dotnet build` 通过
- Dapper + SQLServer 生成项目 `dotnet build` 通过

### REQ-3: 前端测试配置输出路径修正

**问题根因**: `FrontendUnitTestModule.cs` 输出路径为 `$"{projectName}.Web/vitest.config.ts"`，缺少 `src/` 前缀。`FrontendE2EModule.cs` 同样缺少。而 `FrontendModule.cs` 使用 `$"src/{projectName}.Web"` 是正确的。

**影响范围**: 选择 Vitest / Playwright / Cypress 的所有前端测试组合

**修复方案**:
- `FrontendUnitTestModule.cs`: `$"src/{projectName}.Web/vitest.config.ts"`
- `FrontendE2EModule.cs`: `$"src/{projectName}.Web/playwright.config.ts"` 和 `$"src/{projectName}.Web/cypress.config.ts"`

**验收**:
- 生成的 `vitest.config.ts` 与 `package.json` 在同一目录
- 生成的 `playwright.config.ts` 与 `package.json` 在同一目录
- 生成的 `cypress.config.ts` 与 `package.json` 在同一目录

### REQ-4: 前端 lint 闭环

**问题根因**（三重缺失）:
1. `package.json.sbn` lint 脚本使用 `--ext` 标志（eslint < 9 语法），与 flat config 不兼容
2. 无 `eslint.config.js.sbn` 模板——生成项目没有 eslint 配置文件
3. devDependencies 中未包含 eslint 及其插件包

**影响范围**: 所有生成的前端项目

**修复方案**:
1. 修正 lint 脚本为 `"lint": "eslint ."`, `"lint:fix": "eslint . --fix"`
2. 新增 `eslint.config.js.sbn` 模板（参照 web-configurator 的 flat config 风格）
3. 在 `FrontendModule` 或新建 `FrontendLintModule` 中添加 eslint 相关 devDependencies：
   - `eslint`、`eslint-plugin-vue`、`@vue/eslint-config-typescript`、`@vue/eslint-config-prettier`、`@eslint/js`
4. 在模块中注册 eslint.config.js 模板文件输出

**验收**:
- 生成项目 `pnpm install && pnpm lint` 零错误退出
- eslint.config.js 存在且语法正确

---

## Success Criteria

| # | 判据 | 验证命令 |
|---|------|----------|
| 1 | 12 种 ORM x DB 组合全部 `dotnet build` 通过 | 逐组合生成 → `dotnet build` |
| 2 | 前端测试配置文件路径正确 | 检查 ZIP 内文件树 |
| 3 | 生成项目 `pnpm install && pnpm lint` 通过 | 解压 → `pnpm install` → `pnpm lint` |
| 4 | 现有单元测试全部通过 | `dotnet test` |
| 5 | SqlSugar 默认组合无回归 | 默认配置生成 → `dotnet build` |

---

## Resolved Decisions

1. **eslint 包版本**: eslint 9.x flat config，与 web-configurator 对齐：
   - `eslint`: `^9.17.0`
   - `@eslint/js`: `^9.17.0`
   - `eslint-plugin-vue`: `^9.32.0`
   - `@vue/eslint-config-typescript`: `^14.1.4`
   - `@vue/eslint-config-prettier`: `^10.1.0`
   - `typescript-eslint`: `^8.18.2`
2. **Dapper 驱动版本**: .NET 9 最新稳定版：
   - `Microsoft.Data.Sqlite`: `9.0.0`
   - `MySqlConnector`: `2.4.0`
   - `Microsoft.Data.SqlClient`: `5.2.2`
3. **lint 模块归属**: 直接放在 `FrontendModule` 中，不新建模块。

---

## Affected Files

| 文件 | 变更类型 | 关联需求 |
|------|----------|----------|
| `templates/backend/Program.cs.sbn` | 修改 | REQ-1 |
| `templates/backend/Api.csproj.sbn` | 修改 | REQ-2 |
| `Modules/OrmModule.cs` | 修改 | REQ-2 |
| `Modules/FrontendUnitTestModule.cs` | 修改 | REQ-3 |
| `Modules/FrontendE2EModule.cs` | 修改 | REQ-3 |
| `templates/frontend/package.json.sbn` | 修改 | REQ-4 |
| `templates/frontend/eslint.config.js.sbn` | 新增 | REQ-4 |
| `Modules/FrontendModule.cs` | 修改 | REQ-4 |
