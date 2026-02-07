# Package Manager Integration - 需求规格

## 功能需求

### REQ-001: NuGet 包搜索

**场景**: 用户在后端配置中搜索 NuGet 包

**Given**: 用户输入搜索关键词
**When**: 触发搜索请求
**Then**: 返回匹配的包列表，包含 Name, Version, Description

**约束**:
- 搜索防抖 300ms
- 默认返回前 20 条结果
- 版本号仅支持精确格式 (SemVer: x.y.z)

---

### REQ-002: npm 包搜索

**场景**: 用户在前端配置中搜索 npm 包

**Given**: 用户输入搜索关键词
**When**: 触发搜索请求
**Then**: 返回匹配的包列表，包含 Name, Version, Description

**约束**:
- 搜索防抖 300ms
- 默认返回前 20 条结果

---

### REQ-003: 包版本选择

**场景**: 用户选择特定包后选择版本

**Given**: 用户已选择一个包
**When**: 加载版本列表
**Then**: 显示所有可用版本，默认选中最新稳定版

**约束**:
- 仅显示精确版本号 (禁止 ^/~ 范围)
- 排除预发布版本 (alpha/beta/rc)

---

### REQ-004: 包源切换

**场景**: 用户切换包搜索源

**Given**: 用户点击包源切换按钮
**When**: 选择不同的包源
**Then**: 后续搜索使用新的包源地址

**约束**:
- 允许任意 URL (完全自由策略)
- 预置默认源: nuget.org, npmjs.org, 淘宝镜像

---

### REQ-005: 包冲突检测

**场景**: 用户选择的包与系统默认包同名

**Given**: 系统模块已添加某包 (如 SqlSugarCore)
**When**: 用户尝试添加同名包
**Then**: 拒绝操作并显示错误提示

**约束**:
- 包名比较忽略大小写
- 错误信息明确指出冲突的包名

---

### REQ-006: 生成模板渲染

**场景**: 生成项目时渲染包引用

**Given**: 用户已选择若干 NuGet/npm 包
**When**: 生成项目
**Then**: .csproj 包含 PackageReference，package.json 包含 dependencies

**约束**:
- 动态循环渲染，非硬编码
- 版本号来自用户选择

---

## PBT 属性 (Property-Based Testing)

### PROP-001: 包名唯一性 (Idempotency)

**不变式**: 同一包名只能出现一次

**属性定义**:
```
∀ packages: List<Package>
  packages.GroupBy(p => p.Name.ToLower()).All(g => g.Count() == 1)
```

**伪造策略**: 生成包含重复包名（不同大小写）的列表，验证系统拒绝

---

### PROP-002: 版本格式合法性 (Bounds)

**不变式**: 所有版本号符合 SemVer 精确格式

**属性定义**:
```
∀ version: string
  Regex.IsMatch(version, @"^\d+\.\d+\.\d+$")
```

**伪造策略**: 注入 `^1.0.0`, `~2.0`, `latest`, `1.0.0-beta` 等非法格式

---

### PROP-003: 搜索结果幂等性 (Idempotency)

**不变式**: 相同查询返回相同结果（缓存期内）

**属性定义**:
```
∀ query, source: string
  search(query, source) == search(query, source)  // within TTL
```

**伪造策略**: 短时间内发送相同请求，验证结果一致

---

### PROP-004: 冲突检测完备性 (Invariant Preservation)

**不变式**: 用户包与系统包不能同名共存

**属性定义**:
```
∀ userPkg, systemPkg: Package
  userPkg.Name.ToLower() != systemPkg.Name.ToLower()
```

**伪造策略**: 尝试添加系统已有包的不同大小写变体

---

### PROP-005: URL 格式合法性 (Bounds)

**不变式**: 包源地址必须是有效 URL

**属性定义**:
```
∀ sourceUrl: string
  Uri.TryCreate(sourceUrl, UriKind.Absolute, out _)
```

**伪造策略**: 注入 `not-a-url`, `ftp://invalid`, `javascript:alert(1)` 等

---

### PROP-006: 模板渲染完整性 (Round-trip)

**不变式**: 选择的包全部出现在生成文件中

**属性定义**:
```
∀ selectedPackages: List<Package>
  generatedFile.Contains(pkg.Name) && generatedFile.Contains(pkg.Version)
```

**伪造策略**: 选择 N 个包，验证生成文件包含 N 个 PackageReference
