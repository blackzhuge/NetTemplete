import { test, expect } from '@playwright/test'
import { exec } from 'child_process'
import { promisify } from 'util'
import * as fs from 'fs'
import * as path from 'path'
import * as os from 'os'
import AdmZip from 'adm-zip'

const execAsync = promisify(exec)

test.describe('Generated Project Runnability', () => {
  const tempDir = path.join(os.tmpdir(), 'scaffold-test-' + Date.now())

  test.beforeAll(async () => {
    fs.mkdirSync(tempDir, { recursive: true })
  })

  test.afterAll(async () => {
    fs.rmSync(tempDir, { recursive: true, force: true })
  })

  test('generated backend project should restore and build', async ({ request }) => {
    const response = await request.post('/api/v1/scaffolds/generate-zip', {
      data: {
        basic: { projectName: 'TestApp', namespace: 'TestApp' },
        backend: { database: 'SQLite', cache: 'None', swagger: true, jwtAuth: false },
        frontend: { routerMode: 'hash', mockData: false }
      }
    })

    expect(response.ok()).toBeTruthy()

    const buffer = await response.body()
    const zipPath = path.join(tempDir, 'TestApp.zip')
    fs.writeFileSync(zipPath, buffer)

    const zip = new AdmZip(zipPath)
    zip.extractAllTo(tempDir, true)

    const backendPath = path.join(tempDir, 'backend')
    if (fs.existsSync(backendPath)) {
      const { stderr } = await execAsync('dotnet restore', { cwd: backendPath })
      expect(stderr).not.toContain('error')

      const buildResult = await execAsync('dotnet build --no-restore', { cwd: backendPath })
      expect(buildResult.stderr).not.toContain('error')
    }
  })

  test('generated frontend project should install and build', async ({ request }) => {
    const response = await request.post('/api/v1/scaffolds/generate-zip', {
      data: {
        basic: { projectName: 'TestApp2', namespace: 'TestApp2' },
        backend: { database: 'SQLite', cache: 'None', swagger: true, jwtAuth: false },
        frontend: { routerMode: 'hash', mockData: false }
      }
    })

    expect(response.ok()).toBeTruthy()

    const buffer = await response.body()
    const zipPath = path.join(tempDir, 'TestApp2.zip')
    fs.writeFileSync(zipPath, buffer)

    const zip = new AdmZip(zipPath)
    zip.extractAllTo(tempDir, true)

    const frontendPath = path.join(tempDir, 'frontend')
    if (fs.existsSync(frontendPath)) {
      await execAsync('pnpm install', { cwd: frontendPath })
      const buildResult = await execAsync('pnpm build', { cwd: frontendPath })
      expect(buildResult.stderr).not.toContain('error')
    }
  })
})
