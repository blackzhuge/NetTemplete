import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { ScaffoldConfig, FileTreeNode } from '@/types'

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

  return {
    config,
    loading,
    error,
    fileTree,
    updateConfig,
    setLoading,
    setError
  }
})
