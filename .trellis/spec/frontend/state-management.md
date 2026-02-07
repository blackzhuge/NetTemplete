# 状态管理

> 本项目的状态管理方式。

---

## 核心原则

使用 **Pinia Setup Store** + `storeToRefs()` 解构响应式状态。

---

## 状态分类

| 类别 | 位置 | 示例 |
|------|------|------|
| 局部状态 | 组件内 `ref()` | 表单输入、UI 开关 |
| 共享状态 | Pinia store | 用户配置 |
| 服务器状态 | Composable + API | 获取的数据 |

---

## Store 模式

```typescript
// src/stores/config.ts
export const useConfigStore = defineStore('config', () => {
  // 状态
  const config = ref<ScaffoldConfig>({
    projectName: 'MyProject',
    database: 'SQLite',
  })

  // Getters
  const isValid = computed(() => config.value.projectName.length > 0)

  // Actions
  function updateConfig(partial: Partial<ScaffoldConfig>) {
    config.value = { ...config.value, ...partial }
  }

  return { config, isValid, updateConfig }
})
```

---

## 消费 Store

```typescript
import { storeToRefs } from 'pinia'
import { useConfigStore } from '@/stores/config'

const store = useConfigStore()

// ✅ 响应式状态：storeToRefs
const { config, isValid } = storeToRefs(store)

// ✅ 方法：直接解构
const { updateConfig } = store
```

---

## 预设自动应用

```typescript
function applyPreset(presetId: string) {
  const preset = presets.value.find(p => p.id === presetId)
  if (!preset) return

  // 批量更新（只触发一次 watch）
  Object.assign(config.value, preset.config)

  // 立即刷新预览（不走防抖）
  refreshPreview()
}
```

---

## computed + watch 联动

```typescript
// computed 负责派生状态
const fileTree = computed(() => buildTree(config.value))

// watch 负责副作用
watch(config, () => {
  if (selectedFile.value) {
    const newNode = findInTree(fileTree.value, selectedFile.value.name)
    if (newNode) {
      selectedFile.value = newNode
      fetchPreviewDebounced()
    } else {
      selectedFile.value = null
    }
  }
}, { deep: true })
```

**原则**：`computed` = 纯派生，`watch` = 副作用

---

## 常见错误

| 错误 | 正确做法 |
|------|----------|
| `const { config } = useConfigStore()` | 使用 `storeToRefs()` |
| Options API 风格 store | Setup Store 风格 |
| 直接修改 `store.config.x = y` | 使用 action |
| 存储可计算的值 | 使用 `computed` |
| UI 状态放全局 | 使用局部 `ref()` |
