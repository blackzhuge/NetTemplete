import { test, expect } from '@playwright/test'

test.describe('Preset Selection', () => {
  test.beforeEach(async ({ page }) => {
    // Mock presets API
    await page.route('**/api/v1/scaffolds/presets', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          presets: [
            {
              id: 'minimal',
              name: 'Minimal',
              description: '最小化配置，仅包含核心功能',
              isDefault: false,
              tags: ['lightweight', 'quick-start'],
              config: {
                basic: { projectName: 'MinimalApp', namespace: 'MinimalApp' },
                backend: { database: 'SQLite', cache: 'None', swagger: true, jwtAuth: false },
                frontend: { routerMode: 'hash', mockData: false }
              }
            },
            {
              id: 'standard',
              name: 'Standard',
              description: '标准配置，适合大多数项目',
              isDefault: true,
              tags: ['recommended'],
              config: {
                basic: { projectName: 'StandardApp', namespace: 'StandardApp' },
                backend: { database: 'SQLite', cache: 'MemoryCache', swagger: true, jwtAuth: true },
                frontend: { routerMode: 'hash', mockData: false }
              }
            },
            {
              id: 'enterprise',
              name: 'Enterprise',
              description: '企业级配置，包含完整功能',
              isDefault: false,
              tags: ['full-featured', 'production'],
              config: {
                basic: { projectName: 'EnterpriseApp', namespace: 'EnterpriseApp' },
                backend: { database: 'MySQL', cache: 'Redis', swagger: true, jwtAuth: true },
                frontend: { routerMode: 'history', mockData: false }
              }
            }
          ]
        })
      })
    })

    await page.goto('/')
  })

  test('should display preset selector', async ({ page }) => {
    const presetSelector = page.locator('.preset-selector')
    await expect(presetSelector).toBeVisible()
  })

  test('should auto-fill form when selecting preset', async ({ page }) => {
    // Wait for presets to load
    await page.waitForTimeout(500)

    // Open preset dropdown
    const presetSelect = page.locator('.preset-selector .el-select')
    await presetSelect.click()

    // Select Enterprise preset
    await page.locator('.el-select-dropdown__item:has-text("Enterprise")').click()

    // Wait for form update
    await page.waitForTimeout(300)

    // Verify form is updated with Enterprise config
    const fileTree = page.locator('.center-panel').first()
    await expect(fileTree).toContainText('EnterpriseApp')
  })

  test('should update file tree when switching presets', async ({ page }) => {
    await page.waitForTimeout(500)

    // Select Minimal preset
    const presetSelect = page.locator('.preset-selector .el-select')
    await presetSelect.click()
    await page.locator('.el-select-dropdown__item:has-text("Minimal")').click()

    await page.waitForTimeout(300)

    // File tree should show MinimalApp
    await expect(page.locator('.el-tree').first()).toContainText('MinimalApp')
  })
})

test.describe('Code Preview', () => {
  test.beforeEach(async ({ page }) => {
    // Mock presets API
    await page.route('**/api/v1/scaffolds/presets', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ presets: [] })
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

  test('should show code preview panel', async ({ page }) => {
    const codePreview = page.locator('.code-preview').first()
    await expect(codePreview).toBeVisible()
  })

  test('should show preview when clicking file in tree', async ({ page }) => {
    // Wait for page to load
    await page.waitForTimeout(500)

    // Click on a file in the tree (Program.cs)
    const fileNode = page.locator('.el-tree-node:has-text("Program.cs")').first()
    await fileNode.click()

    // Wait for preview to load
    await page.waitForTimeout(500)

    // Code preview should show content
    const codePreview = page.locator('.code-preview').first()
    await expect(codePreview).toContainText('namespace')
  })

  test('should update preview when selecting different file', async ({ page }) => {
    await page.waitForTimeout(500)

    // Click on appsettings.json first (unique name)
    const jsonNode = page.locator('.el-tree-node__content:has-text("appsettings.json")')
    await jsonNode.click()
    await page.waitForTimeout(500)

    // Verify appsettings.json is shown in header
    const codePreviewHeader = page.locator('.code-preview-header')
    await expect(codePreviewHeader).toContainText('appsettings.json')

    // Click on package.json (another unique name)
    const packageNode = page.locator('.el-tree-node__content:has-text("package.json")')
    await packageNode.click()
    await page.waitForTimeout(500)

    // Header should update to show package.json
    await expect(codePreviewHeader).toContainText('package.json')
  })

  test('should show empty state when no file selected', async ({ page }) => {
    await page.waitForTimeout(500)

    // Code preview should show empty state message
    const emptyState = page.locator('.code-preview .empty-state')
    await expect(emptyState).toBeVisible()
  })
})

test.describe('Preset and Preview Integration', () => {
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
                basic: { projectName: 'StandardApp', namespace: 'StandardApp' },
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
      const projectName = body.config?.basic?.projectName || 'MyProject'

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          content: `namespace ${projectName};\n\npublic class Program { }`,
          language: 'csharp',
          outputPath: body.outputPath
        })
      })
    })

    await page.goto('/')
  })

  test('should update preview content when preset changes', async ({ page }) => {
    await page.waitForTimeout(500)

    // Click on a file
    const fileNode = page.locator('.el-tree-node:has-text("Program.cs")').first()
    await fileNode.click()
    await page.waitForTimeout(500)

    // Preview should show StandardApp (from preset)
    const codePreview = page.locator('.code-preview').first()
    await expect(codePreview).toContainText('StandardApp')
  })
})
