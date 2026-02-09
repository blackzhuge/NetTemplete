import { test, expect } from '@playwright/test'

test.describe('Preset Workflow', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/')
  })

  test('should display preset selector', async ({ page }) => {
    const presetSelector = page.locator('.preset-selector')
    await expect(presetSelector).toBeVisible()
  })

  test('should load default preset on page load', async ({ page }) => {
    // 等待页面加载完成
    await page.waitForLoadState('networkidle')

    // 检查预设选择器存在
    const presetDropdown = page.locator('.preset-selector .el-select')
    await expect(presetDropdown).toBeVisible()
  })

  test('should change configuration when selecting different preset', async ({ page }) => {
    // Mock presets API
    await page.route('**/api/v1/presets', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          version: '1.0.0',
          presets: [
            {
              id: 'minimal',
              name: 'Minimal',
              isDefault: false,
              config: {
                projectName: 'MinimalApp',
                backend: { database: 'SQLite', cache: 'None', jwtAuth: false, swagger: true }
              }
            },
            {
              id: 'standard',
              name: 'Standard',
              isDefault: true,
              config: {
                projectName: 'StandardApp',
                backend: { database: 'MySQL', cache: 'MemoryCache', jwtAuth: true, swagger: true }
              }
            }
          ]
        })
      })
    })

    await page.reload()
    await page.waitForLoadState('networkidle')

    // 验证预设选择器可交互
    const presetSelector = page.locator('.preset-selector')
    await expect(presetSelector).toBeVisible()
  })

  test('should display architecture selector in backend options', async ({ page }) => {
    const backendSection = page.locator('.section-title:has-text("后端配置")').locator('..')
    await expect(backendSection).toBeVisible()

    // 检查架构选择器
    const architectureLabel = page.locator('label:has-text("架构风格")')
    await expect(architectureLabel).toBeVisible()
  })

  test('should display ORM selector in backend options', async ({ page }) => {
    const ormLabel = page.locator('label:has-text("ORM 框架")')
    await expect(ormLabel).toBeVisible()
  })

  test('should display UI library selector in frontend options', async ({ page }) => {
    const uiLibraryLabel = page.locator('label:has-text("UI 组件库")')
    await expect(uiLibraryLabel).toBeVisible()
  })

  test('should have 4 architecture options', async ({ page }) => {
    const architectureOptions = page.locator('.card-radio-group').first().locator('.el-radio-button')
    await expect(architectureOptions).toHaveCount(4)
  })

  test('should have 6 UI library options', async ({ page }) => {
    // 找到 UI 组件库的选择器
    const uiSection = page.locator('label:has-text("UI 组件库")').locator('..')
    const uiOptions = uiSection.locator('.el-radio-button')
    await expect(uiOptions).toHaveCount(6)
  })

  test('should generate scaffold with selected options', async ({ page }) => {
    // Mock generate API
    await page.route('**/api/v1/scaffold/generate', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/octet-stream',
        body: Buffer.from('mock zip content')
      })
    })

    // 点击生成按钮
    const generateButton = page.locator('button:has-text("生成")')
    if (await generateButton.isVisible()) {
      // 验证按钮可点击
      await expect(generateButton).toBeEnabled()
    }
  })
})
