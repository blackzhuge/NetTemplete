import { test, expect } from '@playwright/test'

test.describe('Package Selector Modal', () => {
  test.beforeEach(async ({ page }) => {
    // Mock presets API
    await page.route('**/api/v1/scaffolds/presets', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          presets: [
            {
              id: 'standard',
              name: 'Standard',
              description: '标准配置',
              isDefault: true,
              tags: [],
              config: {
                basic: { projectName: 'MyProject', namespace: 'MyProject' },
                backend: { database: 'SQLite', cache: 'MemoryCache', swagger: true, jwtAuth: true },
                frontend: { routerMode: 'hash', mockData: false }
              }
            }
          ]
        })
      })
    })

    // Mock NuGet search API
    await page.route('**/api/v1/packages/nuget/search**', async route => {
      const url = new URL(route.request().url())
      const query = url.searchParams.get('q') || ''

      const packages = query.toLowerCase().includes('serilog')
        ? [
            { name: 'Serilog', version: '3.1.1', description: 'Simple .NET logging', downloadCount: 500000000 },
            { name: 'Serilog.Sinks.Console', version: '5.0.0', description: 'Console sink', downloadCount: 200000000 }
          ]
        : []

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ items: packages, totalCount: packages.length })
      })
    })

    // Mock npm search API
    await page.route('**/api/v1/packages/npm/search**', async route => {
      const url = new URL(route.request().url())
      const query = url.searchParams.get('q') || ''

      const packages = query.toLowerCase().includes('axios')
        ? [
            { name: 'axios', version: '1.6.0', description: 'Promise based HTTP client', downloadCount: 40000000, lastUpdated: '2024-01-15' }
          ]
        : []

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ items: packages, totalCount: packages.length })
      })
    })

    // Mock versions API
    await page.route('**/api/v1/packages/*/versions**', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ versions: ['3.1.1', '3.1.0', '3.0.0', '2.0.0'] })
      })
    })

    await page.goto('/')
  })

  async function openNugetModal(page: any) {
    await page.getByRole('button', { name: '添加NuGet依赖' }).click()
    await expect(page.locator('.el-dialog')).toBeVisible()
  }

  test('should open modal when clicking add dependency button', async ({ page }) => {
    const addButton = page.getByRole('button', { name: '添加NuGet依赖' })
    await expect(addButton).toBeVisible()
    await addButton.click()

    const modal = page.locator('.el-dialog')
    await expect(modal).toBeVisible()
    await expect(modal).toContainText('添加 NuGet 依赖')
  })

  test('should search packages and display results', async ({ page }) => {
    await openNugetModal(page)

    const searchInput = page.getByRole('textbox', { name: '搜索 NuGet 包...' })
    await searchInput.fill('serilog')
    await page.waitForTimeout(500)

    const results = page.locator('.result-item')
    await expect(results).toHaveCount(2)
    await expect(page.locator('.result-item').first()).toContainText('Serilog')
  })

  test('should sort results by downloads', async ({ page }) => {
    await openNugetModal(page)

    const searchInput = page.getByRole('textbox', { name: '搜索 NuGet 包...' })
    await searchInput.fill('serilog')
    await page.waitForTimeout(500)

    const firstResult = page.locator('.result-item').first()
    await expect(firstResult).toContainText('Serilog')
    await expect(firstResult).toContainText('500.0M')
  })

  test('should select package and show in selection list', async ({ page }) => {
    await openNugetModal(page)

    const searchInput = page.getByRole('textbox', { name: '搜索 NuGet 包...' })
    await searchInput.fill('serilog')
    await page.waitForTimeout(500)

    await page.locator('.result-item').first().click()
    await page.waitForTimeout(300)

    const selectedSection = page.locator('.selected-section')
    await expect(selectedSection).toBeVisible()
    await expect(selectedSection).toContainText('Serilog')
  })

  test('should confirm and add packages', async ({ page }) => {
    await openNugetModal(page)

    await page.getByRole('textbox', { name: '搜索 NuGet 包...' }).fill('serilog')
    await page.waitForTimeout(500)
    await page.locator('.result-item').first().click({ force: true })
    await page.waitForTimeout(300)

    const confirmButton = page.getByRole('button', { name: /添加 \(1\)/ })
    await confirmButton.click({ force: true })
    await page.waitForTimeout(300)

    await expect(page.locator('.el-dialog')).not.toBeVisible()

    const packageTag = page.locator('.el-tag:has-text("Serilog")')
    await expect(packageTag).toBeVisible()
  })

  test('should cancel and close modal', async ({ page }) => {
    await openNugetModal(page)

    const cancelButton = page.getByRole('button', { name: '取消' }).last()
    await cancelButton.click()
    await page.waitForTimeout(300)

    await expect(page.locator('.el-dialog')).not.toBeVisible()
  })

  test('should remove package from selection', async ({ page }) => {
    await openNugetModal(page)
    await page.getByRole('textbox', { name: '搜索 NuGet 包...' }).fill('serilog')
    await page.waitForTimeout(500)
    await page.locator('.result-item').first().click({ force: true })
    await page.waitForTimeout(300)
    await page.getByRole('button', { name: /添加 \(1\)/ }).click({ force: true })
    await page.waitForTimeout(300)

    const closeButton = page.locator('.el-tag:has-text("Serilog") .el-tag__close')
    await closeButton.click()

    await expect(page.locator('.el-tag:has-text("Serilog")')).not.toBeVisible()
  })
})

test.describe('npm Package Selector', () => {
  test.beforeEach(async ({ page }) => {
    // Mock presets API
    await page.route('**/api/v1/scaffolds/presets', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ presets: [] })
      })
    })

    // Mock npm search API
    await page.route('**/api/v1/packages/npm/search**', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [
            { name: 'axios', version: '1.6.0', description: 'Promise based HTTP client', downloadCount: 40000000 }
          ],
          totalCount: 1
        })
      })
    })

    await page.route('**/api/v1/packages/npm/*/versions**', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ versions: ['1.6.0', '1.5.0', '1.4.0'] })
      })
    })

    await page.goto('/')
  })

  test('should open npm modal when clicking npm button', async ({ page }) => {
    const addButton = page.getByRole('button', { name: '添加npm依赖' })
    await expect(addButton).toBeVisible()
    await addButton.click()

    const modal = page.locator('.el-dialog')
    await expect(modal).toContainText('添加 npm 依赖')
  })
})
