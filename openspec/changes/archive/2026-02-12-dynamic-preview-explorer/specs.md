# Specs: Dynamic Preview Explorer

## R1: 后端提供文件树 API

### 场景 1.1: 正常获取文件树
**Given** 用户配置了 projectName="MyProject", architecture="Simple", orm="SqlSugar"
**When** 前端调用 POST /api/scaffold/preview/tree
**Then** 返回 200，包含与 ScaffoldPlan.Files 一致的树结构

**约束**：
- 响应时间 < 200ms
- 树结构与生成结果 100% 一致

### 场景 1.2: 空配置
**Given** 请求体为空或缺少必填字段
**When** 调用 API
**Then** 返回 400

### 场景 1.3: 无效配置组合
**Given** 请求包含不支持的配置组合
**When** 调用 API
**Then** 返回 422

---

## R2: 前端调用后端获取文件树

### 场景 2.1: 配置变化时自动更新
**Given** 用户在预览面板打开状态下
**When** 切换 architecture 从 Simple 到 CleanArchitecture
**Then** 300ms 后文件树自动更新，显示新架构的目录结构

**约束**：
- 防抖 300ms
- 更新期间显示 loading 状态

### 场景 2.2: 选中文件在新树中不存在
**Given** 用户选中了 SqlSugarSetup.cs
**When** 切换 ORM 到 EFCore
**Then** 文件树更新后，选中状态清除，代码预览清空

---

## R3: 文件树反映架构差异

### 场景 3.1: Simple 架构
**Given** architecture="Simple"
**When** 获取文件树
**Then** 显示 src/{ProjectName}.Api + src/{ProjectName}.Web

### 场景 3.2: CleanArchitecture 架构
**Given** architecture="CleanArchitecture"
**When** 获取文件树
**Then** 显示 Api + Domain + Application + Infrastructure 四层

### 场景 3.3: VerticalSlice 架构
**Given** architecture="VerticalSlice"
**When** 获取文件树
**Then** 显示 Api/Features 目录

### 场景 3.4: ModularMonolith 架构
**Given** architecture="ModularMonolith"
**When** 获取文件树
**Then** 显示 Api/Modules 目录

---

## R4: 文件树反映 ORM 差异

### 场景 4.1-4.4: 各 ORM 对应文件
| ORM | 预期文件 |
|-----|----------|
| SqlSugar | SqlSugarSetup.cs |
| EFCore | AppDbContext.cs, Migrations/ |
| Dapper | DapperSetup.cs |
| FreeSql | FreeSqlSetup.cs |

---

## R5: 文件树反映 UI 库差异

### 场景 5.1-5.3: 各 UI 库对应文件
| UI 库 | 预期文件 |
|-------|----------|
| ElementPlus | 无额外配置 |
| ShadcnVue/TailwindHeadless | tailwind.config.js, postcss.config.js |
| NaiveUI | 无额外配置 |

---

## R6: 预览包信息

### 场景 6.1: NuGet 包显示
**Given** 用户添加了 NuGet 包 "AutoMapper@12.0.1"
**When** 预览 .csproj 文件
**Then** 内容包含 `<PackageReference Include="AutoMapper" Version="12.0.1" />`

### 场景 6.2: npm 包显示
**Given** 用户添加了 npm 包 "dayjs@1.11.10"
**When** 预览 package.json 文件
**Then** 内容包含 `"dayjs": "1.11.10"`

---

## PBT 属性

### P1: 树一致性不变式
**属性**: 对于任意有效配置 C，PreviewTree(C) 的文件路径集合 = Generate(C) 的文件路径集合
**伪造策略**: 随机生成配置组合，对比预览树路径与生成结果路径

### P2: 幂等性
**属性**: 相同配置多次调用 PreviewTree，返回完全相同的树结构
**伪造策略**: 同一配置连续调用 3 次，断言响应 JSON 完全相等

### P3: 树结构有效性
**属性**: 树中每个节点的 path 都是其 name 的合法路径拼接
**伪造策略**: 遍历树，验证 child.path = parent.path + "/" + child.name

### P4: 目录包含性
**属性**: isDirectory=true 的节点必须有 children 字段（可为空数组）
**伪造策略**: 遍历树，验证所有目录节点都有 children

### P5: 文件叶子性
**属性**: isDirectory=false 的节点不能有 children 字段
**伪造策略**: 遍历树，验证所有文件节点无 children
