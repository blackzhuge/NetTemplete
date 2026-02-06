# 类型安全

> 本项目的类型安全模式。

---

## 概述

本项目使用 **TypeScript 5.3+** 的**严格模式**。类型集中在 `src/types/` 目录。

**关键工具**：
- TypeScript 严格模式用于编译时检查
- Zod 3.22 用于运行时验证（表单）
- vee-validate 4.12 用于表单处理

---

## TypeScript 配置

**文件**：`tsconfig.json`

```json
{
  "compilerOptions": {
    "strict": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "noFallthroughCasesInSwitch": true,
    "baseUrl": ".",
    "paths": {
      "@/*": ["src/*"]
    }
  }
}
```

**严格模式启用**：
- `strictNullChecks`：无隐式 `null`/`undefined`
- `strictFunctionTypes`：正确的函数类型检查
- `noImplicitAny`：必须显式声明类型

---

## 类型组织

### 目录结构

```
src/types/
└── index.ts          # 所有共享类型
```

### 类型定义模式

**文件**：`src/apps/web-configurator/src/types/index.ts`

```typescript
// 联合类型和别名使用 type
export type DatabaseProvider = 'SQLite' | 'MySQL' | 'SQLServer'
export type CacheProvider = 'None' | 'MemoryCache' | 'Redis'

// 对象结构使用 interface
export interface ScaffoldConfig {
  projectName: string
  namespace: string
  database: DatabaseProvider
  cache: CacheProvider
  enableSwagger: boolean
}

// 嵌套结构使用 interface
export interface FileTreeNode {
  name: string
  path: string
  isDirectory: boolean
  children?: FileTreeNode[]
}

// API 响应使用 interface
export interface ApiResponse<T> {
  success: boolean
  data: T
  error?: string
}
```

---

## 何时使用 `type` vs `interface`

| 使用 `type` | 使用 `interface` |
|-------------|------------------|
| 联合类型 | 对象结构 |
| 字面量类型 | 可扩展结构 |
| 类型别名 | API 契约 |
| 映射类型 | Props 定义 |

```typescript
// type 用于联合类型
type Status = 'pending' | 'success' | 'error'

// interface 用于对象
interface User {
  id: string
  name: string
  status: Status
}
```

---

## 常用类型模式

### 泛型 Refs

```typescript
// ✅ 显式类型参数
const config = ref<ScaffoldConfig>({
  projectName: 'MyProject',
  // ...
})

// ✅ 可空类型
const data = ref<User | null>(null)
const error = ref<Error | null>(null)
```

### 带默认值的 Props

```typescript
interface Props {
  title: string
  loading?: boolean
  items?: string[]
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  items: () => []
})
```

### Emit 类型安全

```typescript
const emit = defineEmits<{
  submit: [data: FormData]
  cancel: []
  update: [key: string, value: unknown]
}>()
```

---

## Zod 验证

运行时验证（特别是表单）：

```typescript
import { z } from 'zod'

// 定义 schema
export const configSchema = z.object({
  projectName: z.string().min(1, '必填').max(50),
  namespace: z.string().min(1, '必填'),
  database: z.enum(['SQLite', 'MySQL', 'SQLServer']),
  cache: z.enum(['None', 'MemoryCache', 'Redis']),
  enableSwagger: z.boolean()
})

// 从 schema 推断 TypeScript 类型
export type ScaffoldConfig = z.infer<typeof configSchema>

// 配合 vee-validate 使用
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'

const { handleSubmit, errors } = useForm({
  validationSchema: toTypedSchema(configSchema)
})
```

---

## 类型导入

类型仅导入始终使用 `import type`：

```typescript
// ✅ 正确：类型仅导入
import type { ScaffoldConfig, FileTreeNode } from '@/types'
import type { Ref, ComputedRef } from 'vue'

// ❌ 错误：类型使用值导入
import { ScaffoldConfig } from '@/types'
```

---

## 路径别名

`src/` 内所有导入使用 `@/`：

```typescript
// ✅ 正确
import { useConfigStore } from '@/stores/config'
import type { ScaffoldConfig } from '@/types'

// ❌ 错误
import { useConfigStore } from '../../../stores/config'
```

---

## 禁止模式

### 1. 使用 `any`

```typescript
// ❌ 禁止
const data: any = response.data
function process(input: any) {}

// ✅ 正确：使用正确类型或 unknown
const data: unknown = response.data
function process(input: Record<string, unknown>) {}
```

### 2. 无验证的类型断言

```typescript
// ❌ 危险：无运行时检查
const config = response.data as ScaffoldConfig

// ✅ 更安全：运行时验证
const parsed = configSchema.parse(response.data)
```

### 3. 滥用非空断言

```typescript
// ❌ 危险
const name = user!.name

// ✅ 正确：空值检查
const name = user?.name ?? 'Unknown'
```

### 4. catch 块中的隐式 any

```typescript
// ❌ 错误：err 是隐式 any
try { ... } catch (err) {
  console.log(err.message)  // err 是 any
}

// ✅ 正确：类型化错误
try { ... } catch (err: unknown) {
  if (err instanceof Error) {
    console.log(err.message)
  }
}
```

---

## 自动生成的类型

项目自动生成类型文件：

| 文件 | 用途 | 生成器 |
|------|------|--------|
| `auto-imports.d.ts` | Vue/Pinia/Router API 类型 | unplugin-auto-import |
| `components.d.ts` | 组件类型声明 | unplugin-vue-components |

**不要手动编辑这些文件** —— 它们在构建时重新生成。

---

## 最佳实践总结

1. **集中类型**在 `src/types/`
2. **对象结构用 `interface`**，联合类型用 `type`
3. **始终类型化 refs**，使用泛型：`ref<Type>()`
4. **类型仅导入用 `import type`**
5. **边界处验证**，API 数据用 Zod
6. **永不使用 `any`** —— 不确定时用 `unknown`
7. **路径别名**，内部导入用 `@/`
