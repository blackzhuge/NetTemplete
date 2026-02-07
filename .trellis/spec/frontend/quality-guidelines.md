# 质量规范

> 前端开发的代码质量标准。

---

## 质量检查命令

```bash
pnpm build    # vue-tsc && vite build
pnpm lint     # eslint . --ext .vue,.ts --fix
```

---

## 禁止模式

| 模式 | 正确做法 |
|------|----------|
| `any` 类型 | 具体类型或 `unknown` |
| `const { config } = useConfigStore()` | `storeToRefs()` |
| 组件内 API 调用 | 抽取到 composable |
| `../../../components/` | `@/components/` |
| 内联样式 | `<style scoped>` |
| Options API | Composition API |

---

## 必需模式

### 1. Script setup + TypeScript

```vue
<script setup lang="ts">
// 所有组件必须使用此模式
</script>
```

### 2. Refs 显式类型

```typescript
const config = ref<ScaffoldConfig>({...})
const user = ref<User | null>(null)
```

### 3. 异步错误处理

```typescript
async function fetchData() {
  loading.value = true
  try {
    data.value = await api.getData()
  } catch (e: unknown) {
    error.value = e instanceof Error ? e.message : '未知错误'
  } finally {
    loading.value = false
  }
}
```

---

## 导入顺序

```typescript
// 1. 第三方库
import { ElMessage } from 'element-plus'

// 2. 本地组件
import ConfigForm from '@/components/ConfigForm.vue'

// 3. Composables 和 stores
import { useGenerator } from '@/composables/useGenerator'

// 4. 类型
import type { ScaffoldConfig } from '@/types'
```

---

## 代码审查清单

- [ ] `pnpm build` 通过
- [ ] 无 `any` 类型
- [ ] 无 `console.log`
- [ ] Store 用 `storeToRefs()`
- [ ] 复杂逻辑在 composable
- [ ] 样式是 scoped
