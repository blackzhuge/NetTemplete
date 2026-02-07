import { test, expect } from '@playwright/test'

test.describe('Scaffold Generator UI', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/')
  })

  test('should display page header', async ({ page }) => {
    await expect(page.locator('h1')).toContainText('Scaffold Generator')
    await expect(page.locator('.subtitle')).toContainText('.NET + Vue')
  })

  test('should show config form with default values', async ({ page }) => {
    const projectNameInput = page.locator('input[placeholder*="项目名称"], input').first()
    await expect(projectNameInput).toBeVisible()
  })

  test('should show file tree preview panel', async ({ page }) => {
    await expect(page.locator('.ide-preview')).toBeVisible()
  })

  test('should have generate button', async ({ page }) => {
    const generateButton = page.locator('button:has-text("生成项目")')
    await expect(generateButton).toBeVisible()
  })

  test('should update file tree when config changes', async ({ page }) => {
    // 使用 role 精确定位项目名称输入框
    const projectNameInput = page.getByRole('textbox', { name: '项目名称' })
    await projectNameInput.fill('CustomProject')

    await page.waitForTimeout(500)
    await expect(page.locator('.ide-preview')).toContainText('CustomProject')
  })
})

test.describe('Form Validation', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/')
  })

  test('should disable generate button when form is invalid', async ({ page }) => {
    // 使用 role 精确定位项目名称输入框
    const projectNameInput = page.getByRole('textbox', { name: '项目名称' })
    await projectNameInput.clear()
    await projectNameInput.blur()

    // Wait for validation to run
    await page.waitForTimeout(300)

    const generateButton = page.locator('.generate-btn')
    await expect(generateButton).toBeDisabled()
  })
})

test.describe('Generate Flow', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/')
  })

  test('should show loading state when generating', async ({ page }) => {
    await page.route('**/api/v1/scaffolds/generate-zip', async route => {
      await new Promise(resolve => setTimeout(resolve, 1000))
      await route.fulfill({
        status: 200,
        contentType: 'application/zip',
        body: Buffer.from([0x50, 0x4B, 0x03, 0x04])
      })
    })

    const generateButton = page.locator('.generate-btn')
    await generateButton.click()

    await expect(page.locator('.generate-btn:has-text("正在生成")')).toBeVisible()
  })

  test('should trigger download on successful generation', async ({ page }) => {
    const downloadPromise = page.waitForEvent('download')

    await page.route('**/api/v1/scaffolds/generate-zip', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/zip',
        headers: {
          'Content-Disposition': 'attachment; filename="TestProject.zip"'
        },
        body: Buffer.from([0x50, 0x4B, 0x03, 0x04])
      })
    })

    const generateButton = page.locator('.generate-btn')
    await generateButton.click()

    const download = await downloadPromise
    expect(download.suggestedFilename()).toContain('.zip')
  })
})
