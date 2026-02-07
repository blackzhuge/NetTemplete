import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { ScaffoldConfig, FileTreeNode, ScaffoldPreset, PreviewFileResponse } from '@/types'
import { getPresets, previewFile } from '@/api/generator'

export const useConfigStore = defineStore('config', () => {
  const config = ref<ScaffoldConfig>({
    projectName: 'MyProject',
    namespace: 'MyProject',
    database: 'SQLite',
    cache: 'None',
    enableSwagger: true,
    enableJwtAuth: true,
    routerMode: 'Hash',
    enableMockData: false
  })

  const loading = ref(false)
  const error = ref<string | null>(null)

  // 新增状态: 预设相关
  const presets = ref<ScaffoldPreset[]>([])
  const selectedPresetId = ref<string | null>(null)
  const selectedFile = ref<FileTreeNode | null>(null)
  const previewContent = ref<PreviewFileResponse | null>(null)
  const previewLoading = ref(false)

  // 防抖定时器
  let previewDebounceTimer: ReturnType<typeof setTimeout> | null = null

  const fileTree = computed<FileTreeNode[]>(() => {
    const root: FileTreeNode[] = [
      {
        name: 'src',
        path: 'src',
        isDirectory: true,
        children: [
          {
            name: `${config.value.projectName}.Api`,
            path: `src/${config.value.projectName}.Api`,
            isDirectory: true,
            children: [
              { name: 'Program.cs', path: `src/${config.value.projectName}.Api/Program.cs`, isDirectory: false },
              { name: 'appsettings.json', path: `src/${config.value.projectName}.Api/appsettings.json`, isDirectory: false },
              {
                name: 'Extensions',
                path: `src/${config.value.projectName}.Api/Extensions`,
                isDirectory: true,
                children: getExtensionFiles()
              }
            ]
          },
          {
            name: `${config.value.projectName}.Web`,
            path: `src/${config.value.projectName}.Web`,
            isDirectory: true,
            children: [
              { name: 'package.json', path: `src/${config.value.projectName}.Web/package.json`, isDirectory: false },
              { name: 'vite.config.ts', path: `src/${config.value.projectName}.Web/vite.config.ts`, isDirectory: false },
              { name: 'index.html', path: `src/${config.value.projectName}.Web/index.html`, isDirectory: false }
            ]
          }
        ]
      }
    ]
    return root
  })

  function getExtensionFiles(): FileTreeNode[] {
    const files: FileTreeNode[] = [
      { name: 'SqlSugarSetup.cs', path: 'Extensions/SqlSugarSetup.cs', isDirectory: false }
    ]

    if (config.value.enableJwtAuth) {
      files.push({ name: 'JwtSetup.cs', path: 'Extensions/JwtSetup.cs', isDirectory: false })
    }

    if (config.value.enableSwagger) {
      files.push({ name: 'SwaggerSetup.cs', path: 'Extensions/SwaggerSetup.cs', isDirectory: false })
    }

    if (config.value.cache === 'MemoryCache') {
      files.push({ name: 'MemoryCacheSetup.cs', path: 'Extensions/MemoryCacheSetup.cs', isDirectory: false })
    } else if (config.value.cache === 'Redis') {
      files.push({ name: 'RedisSetup.cs', path: 'Extensions/RedisSetup.cs', isDirectory: false })
    }

    return files
  }

  function updateConfig(partial: Partial<ScaffoldConfig>) {
    config.value = { ...config.value, ...partial }
  }

  function setLoading(value: boolean) {
    loading.value = value
  }

  function setError(message: string | null) {
    error.value = message
  }

  // 新增 Actions: 预设相关
  async function fetchPresets() {
    try {
      presets.value = await getPresets()
      // 自动选择默认预设
      const defaultPreset = presets.value.find(p => p.isDefault)
      if (defaultPreset && !selectedPresetId.value) {
        applyPreset(defaultPreset.id)
      }
    } catch (e) {
      console.error('Failed to fetch presets:', e)
    }
  }

  function applyPreset(presetId: string) {
    const preset = presets.value.find(p => p.id === presetId)
    if (!preset) return

    selectedPresetId.value = presetId
    // 将预设配置转换为扁平结构
    config.value = {
      projectName: preset.config.basic.projectName,
      namespace: preset.config.basic.namespace,
      database: preset.config.backend.database as ScaffoldConfig['database'],
      cache: preset.config.backend.cache as ScaffoldConfig['cache'],
      enableSwagger: preset.config.backend.swagger,
      enableJwtAuth: preset.config.backend.jwtAuth,
      routerMode: (preset.config.frontend.routerMode.charAt(0).toUpperCase() +
                   preset.config.frontend.routerMode.slice(1)) as ScaffoldConfig['routerMode'],
      enableMockData: preset.config.frontend.mockData
    }
  }

  function selectFile(node: FileTreeNode) {
    if (node.isDirectory) return
    selectedFile.value = node
    fetchPreviewDebounced()
  }

  function fetchPreviewDebounced() {
    if (previewDebounceTimer) {
      clearTimeout(previewDebounceTimer)
    }
    previewDebounceTimer = setTimeout(() => {
      fetchPreview()
    }, 300)
  }

  async function fetchPreview() {
    if (!selectedFile.value) return

    previewLoading.value = true
    try {
      previewContent.value = await previewFile(config.value, selectedFile.value.path)
    } catch (e) {
      console.error('Failed to fetch preview:', e)
      previewContent.value = null
    } finally {
      previewLoading.value = false
    }
  }

  return {
    config,
    loading,
    error,
    fileTree,
    updateConfig,
    setLoading,
    setError,
    // 新增导出
    presets,
    selectedPresetId,
    selectedFile,
    previewContent,
    previewLoading,
    fetchPresets,
    applyPreset,
    selectFile,
    fetchPreview
  }
})
