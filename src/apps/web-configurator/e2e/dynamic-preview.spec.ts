import { test, expect } from '@playwright/test'

test.describe('Dynamic Preview', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/v1/packages/nuget/search**', async route => {
      const url = new URL(route.request().url())
      const query = (url.searchParams.get('q') || '').toLowerCase()
      const items = query.includes('automapper')
        ? [{ name: 'AutoMapper', version: '12.0.1', description: 'Object mapper', downloadCount: 100000000 }]
        : []

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ items, totalCount: items.length })
      })
    })

    await page.route('**/api/v1/packages/nuget/*/versions**', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ versions: ['12.0.1'] })
      })
    })

    await page.goto('/')
  })

  test('should update file tree when architecture changes', async ({ page }) => {
    await page.getByText('Clean Architecture', { exact: false }).first().click()
    await page.waitForTimeout(500)
    await page.getByRole('button', { name: '预览' }).click()
    await expect(page.locator('.preview-drawer')).toContainText('MyApp.Domain')
  })

  test('should show ORM-specific files when ORM changes', async ({ page }) => {
    await page.getByText('EF Core', { exact: false }).first().click()
    await page.waitForTimeout(500)
    await page.getByRole('button', { name: '预览' }).click()
    await expect(page.locator('.preview-drawer')).toContainText('EFCoreSetup.cs')
  })

  test('should show UI library config files when UI library changes', async ({ page }) => {
    await page.getByText('shadcn-vue', { exact: false }).first().click()
    await page.waitForTimeout(500)
    await page.getByRole('button', { name: '预览' }).click()
    await expect(page.locator('.preview-drawer')).toContainText('tailwind.config.js')
  })

  test('should show NuGet packages in .csproj preview', async ({ page }) => {
    await page.getByRole('button', { name: '添加NuGet依赖' }).click()
    const searchInput = page.getByRole('textbox', { name: '搜索 NuGet 包...' })
    await searchInput.fill('AutoMapper')
    await page.waitForTimeout(800)
    await page.locator('.result-item').first().click({ force: true })
    await page.getByRole('button', { name: /添加 \(1\)/ }).click({ force: true })

    await page.getByRole('button', { name: '预览' }).click()
    await page.locator('.el-tree-node__content').filter({ hasText: 'MyApp.Api.csproj' }).first().click()
    await page.waitForTimeout(300)
    await expect(page.locator('.code-preview')).toContainText('AutoMapper')
  })
})
