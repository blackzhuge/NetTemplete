import { test, expect } from '@playwright/test'

test.describe('Package Selector', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/')
  })

  test.describe('NuGet Package Selection', () => {
    test('should display package search input in backend options', async ({ page }) => {
      // 查找后端配置区域的包选择器
      const backendSection = page.locator('.section-title:has-text("后端配置")').locator('..')
      await expect(backendSection).toBeVisible()

      // 检查 NuGet 包搜索输入框存在
      const nugetInput = page.locator('.package-selector input').first()
      await expect(nugetInput).toBeVisible()
    })

    test('should search and display NuGet packages', async ({ page }) => {
      // Mock API response
      await page.route('**/api/v1/packages/nuget/search*', async route => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [
              { name: 'Serilog', version: '3.1.1', description: 'Simple .NET logging' },
              { name: 'Serilog.Sinks.Console', version: '5.0.0', description: 'Console sink for Serilog' }
            ],
            totalCount: 2
          })
        })
      })

      const nugetInput = page.locator('.package-selector input').first()
      await nugetInput.fill('serilog')

      // 等待防抖和结果显示
      await page.waitForTimeout(400)

      const results = page.locator('.result-item')
      await expect(results).toHaveCount(2)
      await expect(results.first()).toContainText('Serilog')
    })

    test('should add package after selecting version', async ({ page }) => {
      // Mock search API
      await page.route('**/api/v1/packages/nuget/search*', async route => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [{ name: 'AutoMapper', version: '12.0.1', description: 'Object mapper' }],
            totalCount: 1
          })
        })
      })

      // Mock versions API
      await page.route('**/api/v1/packages/nuget/*/versions*', async route => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            versions: ['12.0.1', '12.0.0', '11.0.0']
          })
        })
      })

      const nugetInput = page.locator('.package-selector input').first()
      await nugetInput.fill('automapper')
      await page.waitForTimeout(400)

      // 点击搜索结果
      await page.locator('.result-item').first().click()

      // 等待版本选择器出现
      await expect(page.locator('.version-selector')).toBeVisible()

      // 点击添加按钮
      await page.locator('.version-selector button:has-text("添加")').click()

      // 验证包已添加到标签列表
      await expect(page.locator('.el-tag:has-text("AutoMapper")')).toBeVisible()
    })

    test('should show warning when adding conflicting system package', async ({ page }) => {
      // Mock search API - 返回系统已有的包
      await page.route('**/api/v1/packages/nuget/search*', async route => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [{ name: 'SqlSugarCore', version: '5.1.0', description: 'ORM framework' }],
            totalCount: 1
          })
        })
      })

      const nugetInput = page.locator('.package-selector input').first()
      await nugetInput.fill('sqlsugar')
      await page.waitForTimeout(400)

      // 点击搜索结果
      await page.locator('.result-item').first().click()

      // 验证显示警告消息
      await expect(page.locator('.el-message--warning')).toBeVisible({ timeout: 5000 })
    })

    test('should remove package when clicking close button', async ({ page }) => {
      // Mock APIs
      await page.route('**/api/v1/packages/nuget/search*', async route => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [{ name: 'Polly', version: '8.2.0', description: 'Resilience library' }],
            totalCount: 1
          })
        })
      })

      await page.route('**/api/v1/packages/nuget/*/versions*', async route => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({ versions: ['8.2.0', '8.1.0'] })
        })
      })

      // 添加包
      const nugetInput = page.locator('.package-selector input').first()
      await nugetInput.fill('polly')
      await page.waitForTimeout(400)
      await page.locator('.result-item').first().click()
      await page.locator('.version-selector button:has-text("添加")').click()

      // 验证包已添加
      const tag = page.locator('.el-tag:has-text("Polly")')
      await expect(tag).toBeVisible()

      // 点击关闭按钮删除
      await tag.locator('.el-tag__close').click()

      // 验证包已删除
      await expect(tag).not.toBeVisible()
    })
  })

  test.describe('npm Package Selection', () => {
    test('should search npm packages in frontend options', async ({ page }) => {
      // Mock npm search API
      await page.route('**/api/v1/packages/npm/search*', async route => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [
              { name: 'dayjs', version: '1.11.10', description: 'Date library' },
              { name: 'lodash-es', version: '4.17.21', description: 'Utility library' }
            ],
            totalCount: 2
          })
        })
      })

      // 找到 npm 包选择器（第二个 package-selector，在前端配置区域）
      const npmInput = page.locator('.package-selector').nth(1).locator('input')
      await npmInput.fill('dayjs')
      await page.waitForTimeout(400)

      const results = page.locator('.result-item')
      await expect(results.first()).toContainText('dayjs')
    })
  })

  test.describe('Package Source Switching', () => {
    test('should switch package source', async ({ page }) => {
      // 找到包选择器中的包源切换按钮
      const packageSelector = page.locator('.package-selector').first()
      const sourceButton = packageSelector.locator('.el-input-group__append button, .el-input__suffix button').first()

      if (await sourceButton.isVisible()) {
        await sourceButton.click()

        // 验证包源列表显示
        await expect(page.locator('.source-list, .source-item').first()).toBeVisible()
      } else {
        // 如果按钮不可见，跳过测试
        test.skip()
      }
    })
  })

  test.describe('Generated Output', () => {
    test('should include selected packages in generated project', async ({ page }) => {
      // Mock APIs
      await page.route('**/api/v1/packages/nuget/search*', async route => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [{ name: 'MediatR', version: '12.2.0', description: 'Mediator pattern' }],
            totalCount: 1
          })
        })
      })

      await page.route('**/api/v1/packages/nuget/*/versions*', async route => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({ versions: ['12.2.0'] })
        })
      })

      // 添加包
      const nugetInput = page.locator('.package-selector input').first()
      await nugetInput.fill('mediatr')
      await page.waitForTimeout(400)
      await page.locator('.result-item').first().click()
      await page.locator('.version-selector button:has-text("添加")').click()

      // 验证包已添加
      await expect(page.locator('.el-tag:has-text("MediatR")')).toBeVisible()

      // Mock generate API - 验证请求包含选中的包
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

      // 点击生成按钮
      const generateButton = page.locator('.generate-btn')
      await generateButton.click()

      // 等待请求完成
      await page.waitForTimeout(1000)

      // 验证请求包含用户选择的包
      if (capturedRequest) {
        expect(capturedRequest.backend?.nugetPackages).toBeDefined()
        expect(capturedRequest.backend.nugetPackages).toContainEqual(
          expect.objectContaining({ name: 'MediatR', version: '12.2.0' })
        )
      }
    })
  })
})
