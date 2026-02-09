import { defineStore } from 'pinia'
import { ref, computed, watch } from 'vue'
import type { ScaffoldConfig, FileTreeNode, ScaffoldPreset, PreviewFileResponse } from '@/types'
import type { PackageReference } from '@/types/packages'
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

  // 包管理状态
  const nugetPackages = ref<PackageReference[]>([])
  const npmPackages = ref<PackageReference[]>([])

  // Preview Drawer 状态
  const showPreviewDrawer = ref(false)

  // 系统包列表（用于冲突检测）
  const systemNugetPackages = computed(() => [
    'Serilog.AspNetCore',
    'SqlSugarCore',
    ...(config.value.enableJwtAuth ? ['Microsoft.AspNetCore.Authentication.JwtBearer'] : []),
    ...(config.value.cache === 'Redis' ? ['StackExchange.Redis'] : []),
    ...(config.value.enableSwagger ? ['Swashbuckle.AspNetCore'] : [])
  ])

  const systemNpmPackages = computed(() => [
    'vue',
    'vue-router',
    'pinia',
    'axios',
    'element-plus'
  ])

  // 防抖定时器
  let previewDebounceTimer: ReturnType<typeof setTimeout> | null = null

  const fileTree = computed<FileTreeNode[]>(() => {
    const projectName = config.value.projectName
    const root: FileTreeNode[] = [
      {
        name: 'src',
        path: 'src',
        isDirectory: true,
        children: [
          {
            name: `${projectName}.Api`,
            path: `src/${projectName}.Api`,
            isDirectory: true,
            children: [
              { name: 'Program.cs', path: `src/${projectName}.Api/Program.cs`, isDirectory: false },
              { name: 'appsettings.json', path: `src/${projectName}.Api/appsettings.json`, isDirectory: false },
              {
                name: 'Extensions',
                path: `src/${projectName}.Api/Extensions`,
                isDirectory: true,
                children: getExtensionFiles()
              },
              // Options 目录 - 根据配置条件显示
              ...(config.value.enableJwtAuth ? [{
                name: 'Options',
                path: `src/${projectName}.Api/Options`,
                isDirectory: true,
                children: [
                  { name: 'JwtOptions.cs', path: `src/${projectName}.Api/Options/JwtOptions.cs`, isDirectory: false }
                ]
              }] : [])
            ]
          },
          {
            name: `${projectName}.Web`,
            path: `src/${projectName}.Web`,
            isDirectory: true,
            children: [
              { name: 'package.json', path: `src/${projectName}.Web/package.json`, isDirectory: false },
              { name: 'vite.config.ts', path: `src/${projectName}.Web/vite.config.ts`, isDirectory: false },
              { name: 'tsconfig.json', path: `src/${projectName}.Web/tsconfig.json`, isDirectory: false },
              { name: 'index.html', path: `src/${projectName}.Web/index.html`, isDirectory: false },
              {
                name: 'src',
                path: `src/${projectName}.Web/src`,
                isDirectory: true,
                children: [
                  { name: 'main.ts', path: `src/${projectName}.Web/src/main.ts`, isDirectory: false },
                  { name: 'App.vue', path: `src/${projectName}.Web/src/App.vue`, isDirectory: false },
                  {
                    name: 'router',
                    path: `src/${projectName}.Web/src/router`,
                    isDirectory: true,
                    children: [
                      { name: 'index.ts', path: `src/${projectName}.Web/src/router/index.ts`, isDirectory: false }
                    ]
                  },
                  {
                    name: 'stores',
                    path: `src/${projectName}.Web/src/stores`,
                    isDirectory: true,
                    children: [
                      { name: 'index.ts', path: `src/${projectName}.Web/src/stores/index.ts`, isDirectory: false }
                    ]
                  },
                  {
                    name: 'api',
                    path: `src/${projectName}.Web/src/api`,
                    isDirectory: true,
                    children: [
                      { name: 'index.ts', path: `src/${projectName}.Web/src/api/index.ts`, isDirectory: false }
                    ]
                  },
                  {
                    name: 'views',
                    path: `src/${projectName}.Web/src/views`,
                    isDirectory: true,
                    children: [
                      { name: 'HomeView.vue', path: `src/${projectName}.Web/src/views/HomeView.vue`, isDirectory: false }
                    ]
                  }
                ]
              }
            ]
          }
        ]
      }
    ]
    return root
  })

  function getExtensionFiles(): FileTreeNode[] {
    const basePath = `src/${config.value.projectName}.Api/Extensions`
    const files: FileTreeNode[] = [
      { name: 'SqlSugarSetup.cs', path: `${basePath}/SqlSugarSetup.cs`, isDirectory: false }
    ]

    if (config.value.enableJwtAuth) {
      files.push({ name: 'JwtSetup.cs', path: `${basePath}/JwtSetup.cs`, isDirectory: false })
    }

    if (config.value.enableSwagger) {
      files.push({ name: 'SwaggerSetup.cs', path: `${basePath}/SwaggerSetup.cs`, isDirectory: false })
    }

    if (config.value.cache === 'MemoryCache') {
      files.push({ name: 'MemoryCacheSetup.cs', path: `${basePath}/MemoryCacheSetup.cs`, isDirectory: false })
    } else if (config.value.cache === 'Redis') {
      files.push({ name: 'RedisSetup.cs', path: `${basePath}/RedisSetup.cs`, isDirectory: false })
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

  // 当 config 变化时，如果有选中文件，需要更新文件路径并刷新预览
  function onConfigChange() {
    if (selectedFile.value) {
      const fileName = selectedFile.value.name

      // 在新的 fileTree 中查找同名文件
      const newNode = findFileInTree(fileTree.value, fileName)
      if (newNode) {
        selectedFile.value = newNode
        fetchPreviewDebounced()
      } else {
        // 文件不存在了（如关闭了某个功能），清除选择
        selectedFile.value = null
        previewContent.value = null
      }
    }
  }

  function findFileInTree(nodes: FileTreeNode[], fileName: string): FileTreeNode | null {
    for (const node of nodes) {
      if (!node.isDirectory && node.name === fileName) {
        return node
      }
      if (node.children) {
        const found = findFileInTree(node.children, fileName)
        if (found) return found
      }
    }
    return null
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

  // 包管理 Actions
  function addNugetPackage(pkg: PackageReference): boolean {
    const nameLower = pkg.name.toLowerCase()
    // 冲突检测：检查系统包
    if (systemNugetPackages.value.some(p => p.toLowerCase() === nameLower)) {
      return false
    }
    // 检查是否已存在
    if (nugetPackages.value.some(p => p.name.toLowerCase() === nameLower)) {
      return false
    }
    nugetPackages.value.push(pkg)
    return true
  }

  function removeNugetPackage(packageName: string) {
    const index = nugetPackages.value.findIndex(
      p => p.name.toLowerCase() === packageName.toLowerCase()
    )
    if (index !== -1) {
      nugetPackages.value.splice(index, 1)
    }
  }

  function addNpmPackage(pkg: PackageReference): boolean {
    const nameLower = pkg.name.toLowerCase()
    // 冲突检测：检查系统包
    if (systemNpmPackages.value.some(p => p.toLowerCase() === nameLower)) {
      return false
    }
    // 检查是否已存在
    if (npmPackages.value.some(p => p.name.toLowerCase() === nameLower)) {
      return false
    }
    npmPackages.value.push(pkg)
    return true
  }

  function removeNpmPackage(packageName: string) {
    const index = npmPackages.value.findIndex(
      p => p.name.toLowerCase() === packageName.toLowerCase()
    )
    if (index !== -1) {
      npmPackages.value.splice(index, 1)
    }
  }

  // Preview Drawer Actions
  function openPreview() {
    showPreviewDrawer.value = true
  }

  function closePreview() {
    showPreviewDrawer.value = false
  }

  // 监听 config 变化，自动刷新预览
  watch(
    config,
    () => {
      onConfigChange()
    },
    { deep: true }
  )

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
    fetchPreview,
    // 包管理导出
    nugetPackages,
    npmPackages,
    systemNugetPackages,
    systemNpmPackages,
    addNugetPackage,
    removeNugetPackage,
    addNpmPackage,
    removeNpmPackage,
    // Preview Drawer
    showPreviewDrawer,
    openPreview,
    closePreview
  }
})
