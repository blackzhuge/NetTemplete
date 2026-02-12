import { defineStore } from 'pinia'
import { ref, computed, watch } from 'vue'
import type { ScaffoldConfig, FileTreeNode, ScaffoldPreset, PreviewFileResponse } from '@/types'
import type { PackageReference } from '@/types/packages'
import { getPresets, previewFile, getPreviewTree } from '@/api/generator'

export const useConfigStore = defineStore('config', () => {
  const config = ref<ScaffoldConfig>({
    projectName: 'MyProject',
    namespace: 'MyProject',
    architecture: 'Simple',
    orm: 'SqlSugar',
    database: 'SQLite',
    cache: 'None',
    enableSwagger: true,
    enableJwtAuth: true,
    uiLibrary: 'ElementPlus',
    routerMode: 'Hash',
    enableMockData: false,
    backendUnitTestFramework: 'None',
    backendIntegrationTestFramework: 'None',
    frontendUnitTestFramework: 'None',
    frontendE2EFramework: 'None'
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

  // 文件树状态 - 从后端获取
  const fileTree = ref<FileTreeNode[]>([])
  const treeLoading = ref(false)

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
  let treeDebounceTimer: ReturnType<typeof setTimeout> | null = null

  function buildConfigWithPackages(): ScaffoldConfig {
    return {
      ...config.value,
      nugetPackages: [...nugetPackages.value],
      npmPackages: [...npmPackages.value]
    }
  }

  // 获取文件树
  async function fetchFileTree() {
    treeLoading.value = true
    try {
      const requestConfig = buildConfigWithPackages()
      console.log('[fetchFileTree] Fetching tree for config:', requestConfig.projectName, requestConfig.orm, requestConfig.architecture)
      const response = await getPreviewTree(requestConfig)
      fileTree.value = response.tree
      onConfigChange()
      console.log('[fetchFileTree] Received tree:', response.tree.length, 'nodes')
    } catch (e) {
      console.error('Failed to fetch file tree:', e)
      // 保持当前状态，不做特殊处理
    } finally {
      treeLoading.value = false
    }
  }

  // 防抖调用（使用较短延迟 100ms）
  function fetchFileTreeDebounced() {
    console.log('[fetchFileTreeDebounced] Triggered')
    if (treeDebounceTimer) {
      clearTimeout(treeDebounceTimer)
    }
    treeDebounceTimer = setTimeout(() => {
      fetchFileTree()
    }, 100)
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
      architecture: preset.config.backend.architecture,
      orm: preset.config.backend.orm,
      database: preset.config.backend.database,
      cache: preset.config.backend.cache,
      enableSwagger: preset.config.backend.swagger,
      enableJwtAuth: preset.config.backend.jwtAuth,
      uiLibrary: preset.config.frontend.uiLibrary,
      routerMode: preset.config.frontend.routerMode,
      enableMockData: preset.config.frontend.mockData,
      backendUnitTestFramework: preset.config.backend.unitTestFramework ?? 'None',
      backendIntegrationTestFramework: preset.config.backend.integrationTestFramework ?? 'None',
      frontendUnitTestFramework: preset.config.frontend.unitTestFramework ?? 'None',
      frontendE2EFramework: preset.config.frontend.e2eFramework ?? 'None'
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
      const currentPath = selectedFile.value.path

      // 优先按完整路径匹配，避免 index.ts 等重名文件选错
      let newNode = findFileByPath(fileTree.value, currentPath)

      // 仅在同名文件唯一时回退到文件名匹配
      if (!newNode) {
        const sameNameFiles = findFilesByName(fileTree.value, selectedFile.value.name)
        if (sameNameFiles.length === 1) {
          newNode = sameNameFiles[0]
        }
      }

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

  function findFileByPath(nodes: FileTreeNode[], path: string): FileTreeNode | null {
    for (const node of nodes) {
      if (!node.isDirectory && node.path === path) {
        return node
      }
      if (node.children) {
        const found = findFileByPath(node.children, path)
        if (found) return found
      }
    }
    return null
  }

  function findFilesByName(nodes: FileTreeNode[], fileName: string): FileTreeNode[] {
    const matches: FileTreeNode[] = []

    function walk(treeNodes: FileTreeNode[]) {
      for (const node of treeNodes) {
        if (!node.isDirectory && node.name === fileName) {
          matches.push(node)
        }
        if (node.children) {
          walk(node.children)
        }
      }
    }

    walk(nodes)
    return matches
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
      previewContent.value = await previewFile(buildConfigWithPackages(), selectedFile.value.path)
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

  // 初始化时获取文件树
  fetchFileTree()

  // 监听 config 变化，自动刷新预览
  watch(
    config,
    () => {
      fetchFileTreeDebounced()
    },
    { deep: true }
  )

  watch(
    [nugetPackages, npmPackages],
    () => {
      fetchFileTreeDebounced()
      if (selectedFile.value) {
        fetchPreviewDebounced()
      }
    },
    { deep: true }
  )

  return {
    config,
    loading,
    error,
    fileTree,
    treeLoading,
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
