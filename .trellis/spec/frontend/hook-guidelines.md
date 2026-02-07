# Hook 规范

> 本项目的 composables（hooks）使用方式。

---

## Composable 结构

```typescript
export function useFeatureName() {
  // 1. 响应式状态
  const loading = ref(false)
  const data = ref<DataType | null>(null)
  const error = ref<string | null>(null)

  // 2. 计算属性
  const isEmpty = computed(() => !data.value)

  // 3. 方法
  async function fetchData() {
    if (loading.value) return  // 防重复
    loading.value = true
    try {
      data.value = await api.getData()
    } catch (err: any) {
      error.value = err.message
    } finally {
      loading.value = false
    }
  }

  // 4. 返回对象
  return { loading, data, error, isEmpty, fetchData }
}
```

---

## 命名约定

| 规则 | 示例 |
|------|------|
| 前缀 `use` | `useGenerator`, `useFileTree` |
| 文件名匹配函数名 | `useGenerator.ts` |

---

## 防抖规范

| 场景 | 时间 |
|------|------|
| 配置表单输入 | 300ms |
| 搜索输入 | 500ms |
| 预设选择 | 0ms（立即） |

```typescript
import { useDebounceFn } from '@vueuse/core'

const debouncedRefresh = useDebounceFn(() => {
  refreshPreview()
}, 300)

watch(() => config.value, () => {
  debouncedRefresh()
}, { deep: true })
```

---

## 包冲突检测

```typescript
function addNugetPackage(pkg: PackageReference): boolean {
  // 检查与系统包冲突
  if (systemPackages.value.some(p =>
    p.toLowerCase() === pkg.name.toLowerCase()
  )) {
    return false  // 冲突
  }
  customPackages.value.push(pkg)
  return true
}
```

---

## 常见错误

| 错误 | 正确做法 |
|------|----------|
| 缺少 loading/error 状态 | 完整状态处理 |
| 未防止重复请求 | `if (loading.value) return` |
| 暴露内部辅助函数 | 只返回必要的 |
| `ref({})` 无类型 | `ref<Type | null>(null)` |
