# UI 布局重构 - 需求规格

## REQ-001: 右侧预览 Drawer

**场景**: 用户需要预览生成的文件结构和代码

**Given**: 用户在配置页面
**When**: 点击"预览"按钮
**Then**: 右侧滑出 Drawer，包含文件树和代码预览 Tab

**约束**:

- Drawer 方向: `rtl`（从右侧滑出）
- Drawer 宽度: 50%（最小 400px）
- 默认激活 Tab: 文件树（Explorer）
- 点击文件自动切换到代码预览 Tab

---

## REQ-002: 包搜索弹窗

**场景**: 用户添加 NuGet/npm 依赖包

**Given**: 用户点击"添加依赖"按钮
**When**: 弹窗打开并输入搜索关键词
**Then**: 展示搜索结果列表，包含丰富信息

**约束**:

- 弹窗宽度: 700px
- 搜索防抖: 300ms
- 默认返回: 20 条结果
- 必须展示: 包名、描述、下载量、最后更新时间
- 支持按下载量排序

---

## REQ-003: 后端 API 扩展

**场景**: 前端需要获取包的额外信息

**Given**: 调用包搜索 API
**When**: 返回搜索结果
**Then**: 包含 downloadCount 和 lastUpdated 字段

**约束**:

- NuGet: `downloadCount` = `totalDownloads`，`lastUpdated` = null（暂不实现）
- npm: `downloadCount` = `downloads.weekly`，`lastUpdated` = `package.date`
- 字段可空，前端需容错处理

---

## PBT 属性

### PROP-001: Drawer 状态幂等性

**不变量**: 连续多次点击打开按钮，Drawer 状态保持一致

**伪造策略**: 快速点击 10 次，验证 `showPreviewDrawer` 最终为 true

### PROP-002: 搜索结果排序稳定性

**不变量**: 相同查询 + 相同排序条件 = 相同结果顺序

**伪造策略**: 连续 3 次相同搜索，比较结果顺序

### PROP-003: 下载量字段边界

**不变量**: `downloadCount >= 0 || downloadCount === null`

**伪造策略**: 解析各种 API 响应，验证不出现负数
