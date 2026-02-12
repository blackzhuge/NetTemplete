import { test, expect } from '@playwright/test'

test.describe('Preset and Preview', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/v1/scaffolds/presets', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          presets: [
            {
              id: 'minimal',
              name: 'Minimal',
              description: '最小化配置',
              isDefault: false,
              tags: [],
              config: {
                basic: { projectName: 'MinimalApp', namespace: 'MinimalApp' },
                backend: {
                  architecture: 'Simple',
                  orm: 'SqlSugar',
                  database: 'SQLite',
                  cache: 'None',
                  swagger: true,
                  jwtAuth: false
                },
                frontend: { uiLibrary: 'ElementPlus', routerMode: 'Hash', mockData: false }
              }
            },
            {
              id: 'enterprise',
              name: 'Enterprise',
              description: '企业级配置',
              isDefault: true,
              tags: [],
              config: {
                basic: { projectName: 'EnterpriseApp', namespace: 'EnterpriseApp' },
                backend: {
                  architecture: 'CleanArchitecture',
                  orm: 'EFCore',
                  database: 'MySQL',
                  cache: 'Redis',
                  swagger: true,
                  jwtAuth: true
                },
                frontend: { uiLibrary: 'ShadcnVue', routerMode: 'History', mockData: false }
              }
            }
          ]
        })
      })
    })

    await page.goto('/')
  })

  test('should show preset selector', async ({ page }) => {
    await expect(page.locator('.preset-selector')).toBeVisible()
  })

  test('should apply selected preset to form', async ({ page }) => {
    await page.locator('.preset-selector .el-select').click()
    await page.locator('.el-select-dropdown__item').filter({ hasText: 'Minimal' }).click()

    await expect(page.getByRole('textbox', { name: '项目名称' })).toHaveValue('MinimalApp')
    await expect(page.getByRole('textbox', { name: '命名空间' })).toHaveValue('MinimalApp')
  })

  test('should update preview tree after preset change', async ({ page }) => {
    await page.locator('.preset-selector .el-select').click()
    await page.locator('.el-select-dropdown__item').filter({ hasText: 'Enterprise' }).click()
    await page.waitForTimeout(500)

    await page.getByRole('button', { name: '预览' }).click()
    await expect(page.locator('.preview-drawer')).toContainText('EnterpriseApp.Domain')
    await expect(page.locator('.preview-drawer')).toContainText('EFCoreSetup.cs')
  })

  test('should update code preview when selecting file after preset change', async ({ page }) => {
    await page.locator('.preset-selector .el-select').click()
    await page.locator('.el-select-dropdown__item').filter({ hasText: 'Enterprise' }).click()
    await page.waitForTimeout(500)

    await page.getByRole('button', { name: '预览' }).click()
    await page.locator('.el-tree-node__content').filter({ hasText: 'Program.cs' }).first().click()
    await expect(page.locator('.code-preview')).toContainText('AddEFCore')
  })
})
