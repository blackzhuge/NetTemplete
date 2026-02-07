# 测试规范

> 前端测试的最佳实践。

---

## 技术栈

- **Vitest** - 单元测试框架
- **@vue/test-utils** - Vue 组件测试
- **Playwright** - E2E 测试

---

## 命名规范

格式：`should xxx when yyy`

```typescript
it('should update config when form value changes', () => {})
it('should show error when validation fails', () => {})
it('should call API when generate button clicked', () => {})
```

---

## Store 测试

```typescript
import { describe, it, expect, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useConfigStore } from '@/stores/config'

describe('useConfigStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('should have default config values', () => {
    const store = useConfigStore()

    expect(store.config.projectName).toBe('MyProject')
    expect(store.config.database).toBe('SQLite')
  })

  it('should update config partially', () => {
    const store = useConfigStore()
    store.updateConfig({ projectName: 'NewProject' })

    expect(store.config.projectName).toBe('NewProject')
    expect(store.config.database).toBe('SQLite')  // 未变
  })

  it('should apply preset correctly', () => {
    const store = useConfigStore()
    store.applyPreset('full-stack')

    expect(store.config.database).toBe('MySQL')
  })
})
```

---

## Composable 测试

```typescript
import { describe, it, expect, vi } from 'vitest'

describe('useDebounce', () => {
  it('should debounce function calls', async () => {
    vi.useFakeTimers()

    const callback = vi.fn()
    const debounced = useDebounce(callback, 300)

    debounced()
    debounced()
    debounced()

    expect(callback).not.toHaveBeenCalled()

    vi.advanceTimersByTime(300)
    expect(callback).toHaveBeenCalledTimes(1)

    vi.useRealTimers()
  })
})
```

---

## 组件测试

```typescript
import { describe, it, expect, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import ConfigForm from '@/components/ConfigForm.vue'

describe('ConfigForm', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('should render database options', () => {
    const wrapper = mount(ConfigForm)

    const select = wrapper.find('[data-testid="database-select"]')
    expect(select.exists()).toBe(true)
  })

  it('should update store when database changes', async () => {
    const wrapper = mount(ConfigForm)
    const store = useConfigStore()

    await wrapper.find('select').setValue('MySQL')

    expect(store.config.database).toBe('MySQL')
  })
})
```

---

## E2E 测试 (Playwright)

```typescript
import { test, expect } from '@playwright/test'

test.describe('Scaffold Generation', () => {
  test('should generate project with config', async ({ page }) => {
    await page.goto('/')

    await page.fill('[name="projectName"]', 'MyProject')
    await page.selectOption('[name="database"]', 'MySQL')

    const downloadPromise = page.waitForEvent('download')
    await page.click('button:has-text("生成项目")')

    const download = await downloadPromise
    expect(download.suggestedFilename()).toBe('MyProject.zip')
  })

  test('should apply preset and update preview', async ({ page }) => {
    await page.goto('/')

    await page.selectOption('[data-testid="preset-select"]', 'full-stack')

    await expect(page.locator('[name="database"]')).toHaveValue('MySQL')
  })
})
```

---

## 覆盖率要求

| 类型 | 最低覆盖率 |
|------|-----------|
| Stores | 90% |
| Composables | 85% |
| Components | 70% |
| Utils | 95% |

---

## 测试隔离

```typescript
beforeEach(() => {
  setActivePinia(createPinia())  // 重置 Store
  vi.clearAllMocks()             // 清除 Mock
})
```

---

## 测试目录结构

```
src/apps/web-configurator/
├── tests/
│   ├── stores/
│   │   └── config.spec.ts
│   ├── composables/
│   │   └── useGenerator.spec.ts
│   └── components/
│       └── ConfigForm.spec.ts
└── e2e/
    ├── configurator.spec.ts
    └── preset-preview.spec.ts
```
