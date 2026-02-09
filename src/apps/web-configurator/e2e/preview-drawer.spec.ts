import { test, expect } from '@playwright/test'

test.describe('Preview Drawer', () => {
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

    // Mock preview API
    await page.route('**/api/v1/scaffolds/preview-file', async route => {
      const request = route.request()
      const body = JSON.parse(request.postData() || '{}')

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          content: `// Generated file: ${body.outputPath}\nnamespace MyProject;\n\npublic class Program\n{\n    public static void Main(string[] args)\n    {\n        Console.WriteLine("Hello, World!");\n    }\n}`,
          language: 'csharp',
          outputPath: body.outputPath
        })
      })
    })

    await page.goto('/')
  })

  test('should open drawer when clicking preview button', async ({ page }) => {
    // Click preview button
    const previewButton = page.locator('button:has-text("预览")')
    await expect(previewButton).toBeVisible()
    await previewButton.click()

    // Drawer should be visible
    const drawer = page.locator('.el-drawer')
    await expect(drawer).toBeVisible()
  })

  test('should show Explorer tab by default', async ({ page }) => {
    // Open drawer
    await page.locator('button:has-text("预览")').click()
    await page.waitForTimeout(300)

    // Explorer tab should be active
    const explorerTab = page.locator('.el-tabs__item:has-text("Explorer")')
    await expect(explorerTab).toHaveClass(/is-active/)
  })

  test('should display file tree in Explorer tab', async ({ page }) => {
    // Open drawer
    await page.locator('button:has-text("预览")').click()
    await page.waitForTimeout(300)

    // File tree should be visible with project structure
    const fileTree = page.locator('.el-tree')
    await expect(fileTree).toBeVisible()
    await expect(fileTree).toContainText('MyProject')
  })

  test('should switch to Code tab when clicking file', async ({ page }) => {
    // Open drawer
    await page.locator('button:has-text("预览")').click()
    await page.waitForTimeout(300)

    // Click on a file in the tree
    const fileNode = page.locator('.el-tree-node:has-text("Program.cs")').first()
    await fileNode.click()
    await page.waitForTimeout(500)

    // Code tab should be active
    const codeTab = page.locator('.el-tabs__item:has-text("Code")')
    await expect(codeTab).toHaveClass(/is-active/)
  })

  test('should show code preview when file is selected', async ({ page }) => {
    // Open drawer
    await page.locator('button:has-text("预览")').click()
    await page.waitForTimeout(300)

    // Click on a file
    const fileNode = page.locator('.el-tree-node:has-text("Program.cs")').first()
    await fileNode.click()
    await page.waitForTimeout(500)

    // Code preview should show content
    const codePreview = page.locator('.code-preview')
    await expect(codePreview).toContainText('namespace')
  })

  test('should close drawer when clicking close button', async ({ page }) => {
    // Open drawer
    await page.locator('button:has-text("预览")').click()
    await page.waitForTimeout(300)

    // Close drawer
    const closeButton = page.locator('.el-drawer__close-btn')
    await closeButton.click()
    await page.waitForTimeout(300)

    // Drawer should not be visible
    const drawer = page.locator('.el-drawer')
    await expect(drawer).not.toBeVisible()
  })

  test('should switch tabs manually', async ({ page }) => {
    // Open drawer
    await page.locator('button:has-text("预览")').click()
    await page.waitForTimeout(300)

    // Click Code tab
    const codeTab = page.locator('.el-tabs__item:has-text("Code")')
    await codeTab.click()
    await expect(codeTab).toHaveClass(/is-active/)

    // Click Explorer tab
    const explorerTab = page.locator('.el-tabs__item:has-text("Explorer")')
    await explorerTab.click()
    await expect(explorerTab).toHaveClass(/is-active/)
  })
})
