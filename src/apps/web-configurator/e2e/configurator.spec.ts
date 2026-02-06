import { test, expect } from '@playwright/test'

test.describe('Scaffold Generator UI', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/')
  })

  test('should display page header', async ({ page }) => {
    await expect(page.locator('h1')).toContainText('Scaffold Generator')
    await expect(page.locator('.page-header p')).toContainText('快速生成')
  })

  test('should show config form with default values', async ({ page }) => {
    const projectNameInput = page.locator('input[placeholder*="项目名称"], input').first()
    await expect(projectNameInput).toBeVisible()
  })

  test('should show file tree preview panel', async ({ page }) => {
    await expect(page.locator('.right-panel')).toBeVisible()
  })

  test('should have generate button', async ({ page }) => {
    const generateButton = page.locator('button:has-text("生成项目")')
    await expect(generateButton).toBeVisible()
  })

  test('should update file tree when config changes', async ({ page }) => {
    const projectNameInput = page.locator('input').first()
    await projectNameInput.fill('CustomProject')

    await page.waitForTimeout(500)
    await expect(page.locator('.right-panel')).toContainText('CustomProject')
  })
})

test.describe('Form Validation', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/')
  })

  test('should disable generate button when form is invalid', async ({ page }) => {
    const projectNameInput = page.locator('input').first()
    await projectNameInput.clear()

    const generateButton = page.locator('button:has-text("生成")')
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

    const generateButton = page.locator('button:has-text("生成项目")')
    await generateButton.click()

    await expect(page.locator('button:has-text("生成中")')).toBeVisible()
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

    const generateButton = page.locator('button:has-text("生成项目")')
    await generateButton.click()

    const download = await downloadPromise
    expect(download.suggestedFilename()).toContain('.zip')
  })
})
