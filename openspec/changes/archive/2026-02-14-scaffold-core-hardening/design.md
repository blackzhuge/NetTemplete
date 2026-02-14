# Scaffold Core Hardening - Design

## Decision Record

| 决策项 | 选择 | 理由 |
| ------ | ---- | ---- |
| D1: using 条件表达式 | 显式列举 `EFCore/Dapper/FreeSql` | 比 `!= SqlSugar` 更安全，新增 ORM 时不会意外引入 Setup 命名空间 |
| D2: Dapper 驱动版本 | SQLite 9.0.0, MySqlConnector 2.4.0, SqlClient 5.2.2 | .NET 9 生态最新稳定版，Codex 已验证 NuGet 兼容性 |
| D3: 驱动包添加位置 | OrmModule.cs + Api.csproj.sbn 双写 | 与 EFCore/FreeSql 现有模式一致（模块添加 + 模板静态声明） |
| D4: eslint 配置模板 | 静态模板（不按 UI 库动态生成） | UI 库不影响 eslint 规则；Gemini 分析确认低风险 |
| D5: eslint 包注册方式 | 通过 plan.AddNpmPackage (isDevDependency) | 走模块系统，与现有 vitest/playwright 包注册方式一致 |
| D6: lint 脚本 | lint 和 lint:fix 分开 | CI 用 lint（不修改），开发用 lint:fix（自动修复） |
| D7: .prettierrc | 新增静态模板 | 引入 @vue/eslint-config-prettier 后需要确定性配置 |
| D8: ArchitectureModule 重复包 | 本次不修复 | HC-5 最小改动原则；被 ScaffoldPlan dedup 掩盖，无运行时 bug |

---

## Data Structures

### Dapper 驱动映射（新增逻辑）

```csharp
// OrmModule.AddDapperFiles() 中新增
switch (request.Backend.Database)
{
    case DatabaseProvider.SQLite:
        plan.AddNugetPackage(new PackageReference("Microsoft.Data.Sqlite", "9.0.0"));
        break;
    case DatabaseProvider.MySQL:
        plan.AddNugetPackage(new PackageReference("MySqlConnector", "2.4.0"));
        break;
    case DatabaseProvider.SQLServer:
        plan.AddNugetPackage(new PackageReference("Microsoft.Data.SqlClient", "5.2.2"));
        break;
}
```

### eslint devDependencies（新增）

```csharp
// FrontendModule.ContributeAsync() 中新增
plan.AddNpmPackage(new PackageReference("eslint", "^9.17.0", isDevDependency: true));
plan.AddNpmPackage(new PackageReference("@eslint/js", "^9.17.0", isDevDependency: true));
plan.AddNpmPackage(new PackageReference("eslint-plugin-vue", "^9.32.0", isDevDependency: true));
plan.AddNpmPackage(new PackageReference("@vue/eslint-config-typescript", "^14.1.4", isDevDependency: true));
plan.AddNpmPackage(new PackageReference("@vue/eslint-config-prettier", "^10.1.0", isDevDependency: true));
plan.AddNpmPackage(new PackageReference("typescript-eslint", "^8.18.2", isDevDependency: true));
plan.AddNpmPackage(new PackageReference("prettier", "^3.4.0", isDevDependency: true));
```

---

## Template Changes

### Program.cs.sbn（REQ-1）

```diff
 using {{ namespace }}.Api.Extensions;
+{{~ if orm == "EFCore" || orm == "Dapper" || orm == "FreeSql" ~}}
+using {{ namespace }}.Api.Setup;
+{{~ end ~}}
 using Serilog;
```

### Api.csproj.sbn Dapper 分支（REQ-2）

```diff
 {{~ else if orm == "Dapper" ~}}
     <PackageReference Include="Dapper" Version="2.1.35" />
+{{~ if db_type == "SQLite" ~}}
+    <PackageReference Include="Microsoft.Data.Sqlite" Version="9.0.0" />
+{{~ else if db_type == "MySQL" ~}}
+    <PackageReference Include="MySqlConnector" Version="2.4.0" />
+{{~ else if db_type == "SQLServer" ~}}
+    <PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.2" />
+{{~ end ~}}
```

### package.json.sbn lint 脚本（REQ-4）

```diff
-    "lint": "eslint . --ext .vue,.js,.jsx,.cjs,.mjs,.ts,.tsx,.cts,.mts --fix"
+    "lint": "eslint .",
+    "lint:fix": "eslint . --fix"
```

### eslint.config.js.sbn（REQ-4，新增）

参照 `src/apps/web-configurator/eslint.config.js`，静态模板。

### .prettierrc.sbn（REQ-4，新增）

```json
{
  "semi": false,
  "singleQuote": true,
  "trailingComma": "es5",
  "printWidth": 100,
  "tabWidth": 2
}
```

---

## File Impact Summary

| 文件 | 变更 | REQ |
| ---- | ---- | --- |
| `templates/backend/Program.cs.sbn` | 添加条件 using | REQ-1 |
| `templates/backend/Api.csproj.sbn` | Dapper 分支添加驱动包 | REQ-2 |
| `Modules/OrmModule.cs` | AddDapperFiles 添加驱动包 | REQ-2 |
| `Modules/FrontendUnitTestModule.cs` | 输出路径加 `src/` 前缀 | REQ-3 |
| `Modules/FrontendE2EModule.cs` | 输出路径加 `src/` 前缀 | REQ-3 |
| `templates/frontend/package.json.sbn` | 修正 lint 脚本 | REQ-4 |
| `templates/frontend/eslint.config.js.sbn` | 新增 | REQ-4 |
| `templates/frontend/.prettierrc.sbn` | 新增 | REQ-4 |
| `Modules/FrontendModule.cs` | 添加 eslint/prettier 包 + 模板注册 | REQ-4 |
