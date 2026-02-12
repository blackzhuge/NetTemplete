import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import type { FileTreeNode } from '@/types'

const { getPresetsMock, previewFileMock, getPreviewTreeMock } = vi.hoisted(() => ({
  getPresetsMock: vi.fn(),
  previewFileMock: vi.fn(),
  getPreviewTreeMock: vi.fn()
}))

vi.mock('@/api/generator', () => ({
  getPresets: getPresetsMock,
  previewFile: previewFileMock,
  getPreviewTree: getPreviewTreeMock
}))

import { useConfigStore } from '@/stores/config'

describe('useConfigStore preview payload', () => {
  beforeEach(() => {
    vi.useFakeTimers()
    setActivePinia(createPinia())

    getPresetsMock.mockReset()
    previewFileMock.mockReset()
    getPreviewTreeMock.mockReset()

    getPresetsMock.mockResolvedValue([])
    previewFileMock.mockResolvedValue({
      content: '<Project />',
      language: 'xml',
      outputPath: 'src/MyApp.Api/MyApp.Api.csproj'
    })
    getPreviewTreeMock.mockResolvedValue({ tree: [] })
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('should include custom packages in preview-file and preview-tree requests', async () => {
    getPreviewTreeMock.mockResolvedValue({
      tree: [
        {
          name: 'src',
          path: 'src',
          isDirectory: true,
          children: [
            {
              name: 'MyApp.Api',
              path: 'src/MyApp.Api',
              isDirectory: true,
              children: [
                {
                  name: 'MyApp.Api.csproj',
                  path: 'src/MyApp.Api/MyApp.Api.csproj',
                  isDirectory: false
                }
              ]
            }
          ]
        }
      ]
    })

    const store = useConfigStore()
    const selectedFile: FileTreeNode = {
      name: 'MyApp.Api.csproj',
      path: 'src/MyApp.Api/MyApp.Api.csproj',
      isDirectory: false
    }

    store.selectFile(selectedFile)
    store.nugetPackages.push({ name: 'FluentValidation', version: '11.11.0' })

    await vi.advanceTimersByTimeAsync(400)
    await Promise.resolve()

    const latestPreviewCall = previewFileMock.mock.calls.at(-1)
    expect(latestPreviewCall).toBeTruthy()
    expect(latestPreviewCall![0].nugetPackages).toEqual([
      { name: 'FluentValidation', version: '11.11.0' }
    ])
    expect(latestPreviewCall![1]).toBe('src/MyApp.Api/MyApp.Api.csproj')

    const hasMergedPackagesInTreeRequest = getPreviewTreeMock.mock.calls.some(
      ([requestConfig]) =>
        Array.isArray(requestConfig?.nugetPackages) &&
        requestConfig.nugetPackages.some((pkg: { name: string }) => pkg.name === 'FluentValidation')
    )

    expect(hasMergedPackagesInTreeRequest).toBe(true)
  })

  it('should keep selected file by full path when file names are duplicated', async () => {
    getPreviewTreeMock.mockResolvedValue({
      tree: [
        {
          name: 'src',
          path: 'src',
          isDirectory: true,
          children: [
            {
              name: 'MyApp.Web',
              path: 'src/MyApp.Web',
              isDirectory: true,
              children: [
                {
                  name: 'src',
                  path: 'src/MyApp.Web/src',
                  isDirectory: true,
                  children: [
                    {
                      name: 'router',
                      path: 'src/MyApp.Web/src/router',
                      isDirectory: true,
                      children: [
                        {
                          name: 'index.ts',
                          path: 'src/MyApp.Web/src/router/index.ts',
                          isDirectory: false
                        }
                      ]
                    },
                    {
                      name: 'api',
                      path: 'src/MyApp.Web/src/api',
                      isDirectory: true,
                      children: [
                        {
                          name: 'index.ts',
                          path: 'src/MyApp.Web/src/api/index.ts',
                          isDirectory: false
                        }
                      ]
                    }
                  ]
                }
              ]
            }
          ]
        }
      ]
    })

    const store = useConfigStore()
    const routerIndex: FileTreeNode = {
      name: 'index.ts',
      path: 'src/MyApp.Web/src/router/index.ts',
      isDirectory: false
    }

    store.selectFile(routerIndex)
    store.updateConfig({ routerMode: 'History' })

    await vi.advanceTimersByTimeAsync(500)
    await Promise.resolve()

    const latestPreviewCall = previewFileMock.mock.calls.at(-1)
    expect(latestPreviewCall).toBeTruthy()
    expect(latestPreviewCall![1]).toBe('src/MyApp.Web/src/router/index.ts')
  })
})
