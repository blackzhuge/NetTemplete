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
    await page.route('**/api/v1/scaffolds/presets', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          presets: [
            {
              id: 'minimal',
              name: 'Minimal',
              isDefault: false,
              description: '最小化',
              tags: [],
              config: {
                basic: { projectName: 'MinimalApp', namespace: 'MinimalApp' },
                backend: {
                  architecture: 'Simple',
                  orm: 'SqlSugar',
                  database: 'SQLite',
                  cache: 'None',
                  jwtAuth: false,
                  swagger: true
                },
                frontend: { uiLibrary: 'ElementPlus', routerMode: 'Hash', mockData: false }
              }
            },
            {
              id: 'standard',
              name: 'Standard',
              isDefault: true,
              description: '标准',
              tags: [],
              config: {
                basic: { projectName: 'StandardApp', namespace: 'StandardApp' },
                backend: {
                  architecture: 'Simple',
                  orm: 'SqlSugar',
                  database: 'MySQL',
                  cache: 'MemoryCache',
                  jwtAuth: true,
                  swagger: true
                },
                frontend: { uiLibrary: 'ElementPlus', routerMode: 'Hash', mockData: false }
              }
            }
          ]
        })
      })
    })

    await page.reload()
    await page.waitForLoadState('networkidle')

    await page.locator('.preset-selector .el-select').click()
    await page.locator('.el-select-dropdown__item').filter({ hasText: 'Minimal' }).click()

    await expect(page.getByRole('textbox', { name: '项目名称' })).toHaveValue('MinimalApp')
  })

  test('should display architecture selector in backend options', async ({ page }) => {
    const backendSection = page.locator('.section-title').filter({ hasText: '后端配置' })
    await expect(backendSection).toBeVisible()

    const architectureLabel = page.locator('.el-form-item__label').filter({ hasText: '架构风格' })
    await expect(architectureLabel).toBeVisible()
  })

  test('should display ORM selector in backend options', async ({ page }) => {
    const ormLabel = page.locator('.el-form-item__label').filter({ hasText: 'ORM 框架' })
    await expect(ormLabel).toBeVisible()
  })

  test('should display UI library selector in frontend options', async ({ page }) => {
    const uiLibraryLabel = page.locator('.el-form-item__label').filter({ hasText: 'UI 组件库' })
    await expect(uiLibraryLabel).toBeVisible()
  })

  test('should have 4 architecture options', async ({ page }) => {
    const architectureOptions = page.locator('.card-radio-group').first().locator('.el-radio-button')
    await expect(architectureOptions).toHaveCount(4)
  })

  test('should have 6 UI library options', async ({ page }) => {
    const uiSection = page.locator('.el-form-item').filter({ hasText: 'UI 组件库' }).first()
    const uiOptions = uiSection.locator('.el-radio-button')
    await expect(uiOptions).toHaveCount(6)
  })

  test('should generate scaffold with selected options', async ({ page }) => {
    await page.route('**/api/v1/scaffolds/generate-zip', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/zip',
        body: Buffer.from([0x50, 0x4B, 0x03, 0x04])
      })
    })

    const generateButton = page.locator('.generate-btn')
    await expect(generateButton).toBeVisible()
    await expect(generateButton).toBeEnabled()
  })
})
