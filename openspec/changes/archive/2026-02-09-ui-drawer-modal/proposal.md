# UI 布局重构：Drawer 交互 + 弹窗式包搜索

## 背景

当前 web-configurator 的 UI 布局过于拥挤，配置区域空间不足。EXPLORER 和代码预览区域始终占用固定空间，导致设置表单显示不全。

## 目标

优化空间利用率，让配置区域获得最大视觉空间，同时提升包搜索体验。

## 约束集合

### 硬约束（技术限制）

- 使用 Element Plus 组件库（el-drawer、el-dialog、el-tabs）
- Vue 3 + Composition API + Pinia 架构
- 后端 .NET 9 + Minimal API
- NuGet API 返回 `totalDownloads`，`lastUpdated` 需额外请求（暂不实现）
- npm API 返回 `downloads.weekly` 和 `package.date`

### 软约束（设计偏好）

- Drawer 保持暗色 IDE 风格（背景 #1e1e1e）
- 弹窗使用标准亮色主题
- 参考 start.spring.io 的 Add Dependencies 交互模式

## 成功判据

- [ ] EXPLORER + 代码预览合并为右侧 Drawer，Tab 切换
- [ ] Drawer 默认收起，配置区域获得全宽
- [ ] 包搜索改为模态弹窗触发
- [ ] 弹窗展示：包名、描述、下载量、最后更新时间
- [ ] 弹窗支持按下载量排序
- [ ] 弹窗内可选择版本
- [ ] 支持批量选择和移除

## 开放问题

（已确认，无遗留问题）

- ~~EXPLORER 和代码预览是否合并？~~ → 合并为一个 Drawer，Tab 切换
- ~~包搜索额外展示哪些信息？~~ → 下载量、最后更新时间、版本选择
- ~~是否保留固定预览模式？~~ → 不保留，仅 Drawer 交互
