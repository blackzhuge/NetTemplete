import { describe, it, expect, vi, beforeEach } from 'vitest'
import type { ScaffoldConfig } from '@/types'

const { postMock, getMock } = vi.hoisted(() => ({
  postMock: vi.fn(),
  getMock: vi.fn()
}))

vi.mock('axios', () => {
  const create = vi.fn(() => ({
    post: postMock,
    get: getMock,
    interceptors: {
      response: {
        use: vi.fn()
      }
    }
  }))

  return {
    default: { create },
    create,
    AxiosError: class AxiosError extends Error {}
  }
})

vi.mock('element-plus', () => ({
  ElMessage: {
    error: vi.fn()
  }
}))

import { previewFile, getPreviewTree } from '@/api/generator'

describe('generator api payload mapping', () => {
  const baseConfig: ScaffoldConfig = {
    projectName: 'MyApp',
    namespace: 'MyApp',
    architecture: 'CleanArchitecture',
    orm: 'EFCore',
    database: 'SQLite',
    cache: 'None',
    enableSwagger: true,
    enableJwtAuth: true,
    uiLibrary: 'ShadcnVue',
    routerMode: 'History',
    enableMockData: false,
    nugetPackages: [],
    npmPackages: []
  }

  beforeEach(() => {
    postMock.mockReset()
    getMock.mockReset()
  })

  it('should send full backend/frontend fields for preview-file', async () => {
    postMock.mockResolvedValue({ data: { outputPath: 'x', content: '', language: 'xml' } })

    await previewFile(baseConfig, 'src/MyApp.Api/MyApp.Api.csproj')

    expect(postMock).toHaveBeenCalledTimes(1)
    const [url, payload] = postMock.mock.calls[0]

    expect(url).toBe('/v1/scaffolds/preview-file')
    expect(payload.config.backend).toMatchObject({
      architecture: 'CleanArchitecture',
      orm: 'EFCore',
      database: 'SQLite',
      cache: 'None',
      swagger: true,
      jwtAuth: true
    })
    expect(payload.config.frontend).toMatchObject({
      uiLibrary: 'ShadcnVue',
      routerMode: 'History',
      mockData: false
    })
  })

  it('should send full backend/frontend fields for preview-tree', async () => {
    postMock.mockResolvedValue({ data: { tree: [] } })

    await getPreviewTree(baseConfig)

    expect(postMock).toHaveBeenCalledTimes(1)
    const [url, payload] = postMock.mock.calls[0]

    expect(url).toBe('/v1/scaffolds/preview-tree')
    expect(payload.config.backend.orm).toBe('EFCore')
    expect(payload.config.backend.architecture).toBe('CleanArchitecture')
    expect(payload.config.frontend.uiLibrary).toBe('ShadcnVue')
    expect(payload.config.frontend.routerMode).toBe('History')
  })
})
