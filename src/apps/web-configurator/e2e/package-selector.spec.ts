import { test, expect } from '@playwright/test'

test.describe('Package Selector', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/v1/packages/nuget/search**', async route => {
      const url = new URL(route.request().url())
      const query = (url.searchParams.get('q') || '').toLowerCase()
      const items = query.includes('serilog')
        ? [
            { name: 'Serilog', version: '3.1.1', description: 'Simple .NET logging', downloadCount: 500000000 },
            { name: 'Serilog.Sinks.Console', version: '5.0.0', description: 'Console sink', downloadCount: 200000000 }
          ]
        : query.includes('automapper')
          ? [{ name: 'AutoMapper', version: '12.0.1', description: 'Object mapper', downloadCount: 100000000 }]
          : query.includes('polly')
            ? [{ name: 'Polly', version: '8.2.0', description: 'Resilience library', downloadCount: 50000000 }]
            : []

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ items, totalCount: items.length })
      })
    })

    await page.route('**/api/v1/packages/npm/search**', async route => {
      const url = new URL(route.request().url())
      const query = (url.searchParams.get('q') || '').toLowerCase()
      const items = query.includes('dayjs')
        ? [{ name: 'dayjs', version: '1.11.10', description: 'Date library', downloadCount: 20000000 }]
        : []

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ items, totalCount: items.length })
      })
    })

    await page.route('**/api/v1/packages/*/versions**', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ versions: ['12.0.1', '8.2.0', '5.0.0', '3.1.1', '1.11.10'] })
      })
    })

    await page.goto('/')
  })

  test('should open nuget modal and show search result', async ({ page }) => {
    await page.getByRole('button', { name: '添加NuGet依赖' }).click()
    await page.getByRole('textbox', { name: '搜索 NuGet 包...' }).fill('serilog')
    await page.waitForTimeout(500)

    const results = page.locator('.result-item')
    await expect(results).toHaveCount(2)
    await expect(results.first()).toContainText('Serilog')
  })

  test('should add and remove a nuget package', async ({ page }) => {
    await page.getByRole('button', { name: '添加NuGet依赖' }).click()
    await page.getByRole('textbox', { name: '搜索 NuGet 包...' }).fill('polly')
    await page.waitForTimeout(500)
    await page.locator('.result-item').first().click({ force: true })
    await page.getByRole('button', { name: /添加 \(1\)/ }).click({ force: true })

    const tag = page.locator('.selected-packages').locator('text=Polly').first()
    await expect(tag).toBeVisible()

    await page.locator('.selected-packages .el-tag__close').first().click()
    await expect(page.locator('.selected-packages').locator('text=Polly')).not.toBeVisible()
  })

  test('should show warning for conflicting system package', async ({ page }) => {
    await page.route('**/api/v1/packages/nuget/search**', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [{ name: 'SqlSugarCore', version: '5.1.0', description: 'ORM framework', downloadCount: 300000000 }],
          totalCount: 1
        })
      })
    })

    await page.getByRole('button', { name: '添加NuGet依赖' }).click()
    await page.getByRole('textbox', { name: '搜索 NuGet 包...' }).fill('sqlsugar')
    await page.waitForTimeout(500)
    await page.locator('.result-item').first().click()
    await expect(page.locator('.el-message--warning')).toBeVisible({ timeout: 5000 })
  })

  test('should search npm package in frontend modal', async ({ page }) => {
    await page.getByRole('button', { name: '添加npm依赖' }).click()
    await page.getByRole('textbox', { name: '搜索 npm 包...' }).fill('dayjs')
    await page.waitForTimeout(500)

    await expect(page.locator('.result-item').first()).toContainText('dayjs')
  })

  test('should send selected nuget package when generating', async ({ page }) => {
    let capturedRequest: any = null

    await page.route('**/api/v1/scaffolds/generate-zip', async route => {
      capturedRequest = route.request().postDataJSON()
      await route.fulfill({
        status: 200,
        contentType: 'application/zip',
        headers: { 'Content-Disposition': 'attachment; filename="test.zip"' },
        body: Buffer.from([0x50, 0x4B, 0x03, 0x04])
      })
    })

    await page.getByRole('button', { name: '添加NuGet依赖' }).click()
    await page.getByRole('textbox', { name: '搜索 NuGet 包...' }).fill('automapper')
    await page.waitForTimeout(500)
    await page.locator('.result-item').first().click({ force: true })
    await page.getByRole('button', { name: /添加 \(1\)/ }).click({ force: true })

    await page.locator('.generate-btn').click()
    await page.waitForTimeout(500)

    expect(capturedRequest?.backend?.nugetPackages).toBeDefined()
    expect(capturedRequest.backend.nugetPackages).toContainEqual(
      expect.objectContaining({ name: 'AutoMapper', version: '12.0.1' })
    )
  })
})
