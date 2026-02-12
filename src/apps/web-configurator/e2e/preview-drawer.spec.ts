import { test, expect } from '@playwright/test'

test.describe('Preview Drawer', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/v1/scaffolds/preview-file', async route => {
      const body = route.request().postDataJSON() as { outputPath: string }
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          content: `// Generated file: ${body.outputPath}\nnamespace MyApp;\n\nvar builder = WebApplication.CreateBuilder(args);\nbuilder.Services.AddControllers();`,
          language: 'csharp',
          outputPath: body.outputPath
        })
      })
    })

    await page.goto('/')
  })

  test('should open and close drawer', async ({ page }) => {
    await page.getByRole('button', { name: '预览' }).click()
    const drawer = page.locator('.preview-drawer')
    await expect(drawer).toBeVisible()

    await page.getByRole('button', { name: '关闭此对话框' }).click()
    await expect(drawer).not.toBeVisible()
  })

  test('should show explorer and code panels', async ({ page }) => {
    await page.getByRole('button', { name: '预览' }).click()
    await expect(page.locator('.explorer-panel .panel-header')).toContainText('Explorer')
    await expect(page.locator('.code-panel .panel-header')).toContainText('Code')
  })

  test('should show file tree in explorer panel', async ({ page }) => {
    await page.getByRole('button', { name: '预览' }).click()
    const tree = page.locator('.preview-drawer .el-tree')
    await expect(tree).toBeVisible()
    await expect(tree).toContainText('MyApp')
  })

  test('should render code preview when file selected', async ({ page }) => {
    await page.getByRole('button', { name: '预览' }).click()
    await page.locator('.el-tree-node__content').filter({ hasText: 'Program.cs' }).first().click()
    await expect(page.locator('.code-preview-header')).toContainText('Program.cs')
    await expect(page.locator('.code-preview')).toContainText('builder.Services')
  })

  test('should update header when selecting different file', async ({ page }) => {
    await page.getByRole('button', { name: '预览' }).click()
    const appsettingsNode = page.locator('.el-tree-node__content').filter({ hasText: 'appsettings.json' }).first()
    const programNode = page.locator('.el-tree-node__content').filter({ hasText: 'Program.cs' }).first()

    await appsettingsNode.click()
    await expect(page.locator('.code-preview-header')).toContainText('appsettings.json')

    await programNode.click()
    await expect(page.locator('.code-preview-header')).toContainText('Program.cs')
  })
})
