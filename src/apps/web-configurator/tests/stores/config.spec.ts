import { describe, it, expect, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useConfigStore } from '@/stores/config'

describe('useConfigStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('should have default config values', () => {
    const store = useConfigStore()

    expect(store.config.projectName).toBe('MyProject')
    expect(store.config.namespace).toBe('MyProject')
    expect(store.config.database).toBe('SQLite')
    expect(store.config.cache).toBe('None')
    expect(store.config.enableSwagger).toBe(true)
    expect(store.config.enableJwtAuth).toBe(true)
    expect(store.config.routerMode).toBe('Hash')
    expect(store.config.enableMockData).toBe(false)
    expect(store.config.backendUnitTestFramework).toBe('None')
    expect(store.config.backendIntegrationTestFramework).toBe('None')
    expect(store.config.frontendUnitTestFramework).toBe('None')
    expect(store.config.frontendE2EFramework).toBe('None')
  })

  it('should update config partially', () => {
    const store = useConfigStore()

    store.updateConfig({ projectName: 'NewProject' })

    expect(store.config.projectName).toBe('NewProject')
    expect(store.config.namespace).toBe('MyProject')
  })

  it('should update test framework fields', () => {
    const store = useConfigStore()

    store.updateConfig({
      backendUnitTestFramework: 'xUnit',
      frontendE2EFramework: 'Playwright'
    })

    expect(store.config.backendUnitTestFramework).toBe('xUnit')
    expect(store.config.frontendE2EFramework).toBe('Playwright')
    expect(store.config.backendIntegrationTestFramework).toBe('None')
  })

  it('should set loading state', () => {
    const store = useConfigStore()

    expect(store.loading).toBe(false)
    store.setLoading(true)
    expect(store.loading).toBe(true)
  })

  it('should set error state', () => {
    const store = useConfigStore()

    expect(store.error).toBeNull()
    store.setError('Test error')
    expect(store.error).toBe('Test error')
  })

  describe('Package Management', () => {
    it('should add nuget package', () => {
      const store = useConfigStore()

      const result = store.addNugetPackage({
        name: 'Serilog',
        version: '3.1.1'
      })

      expect(result).toBe(true)
      expect(store.nugetPackages).toHaveLength(1)
      expect(store.nugetPackages[0].name).toBe('Serilog')
    })

    it('should reject duplicate nuget package (case insensitive)', () => {
      const store = useConfigStore()

      store.addNugetPackage({ name: 'Serilog', version: '3.1.1' })
      const result = store.addNugetPackage({ name: 'serilog', version: '3.0.0' })

      expect(result).toBe(false)
      expect(store.nugetPackages).toHaveLength(1)
    })

    it('should reject nuget package conflicting with system packages', () => {
      const store = useConfigStore()
      store.updateConfig({ enableJwtAuth: true })

      const result = store.addNugetPackage({
        name: 'Microsoft.AspNetCore.Authentication.JwtBearer',
        version: '8.0.0'
      })

      expect(result).toBe(false)
      expect(store.nugetPackages).toHaveLength(0)
    })

    it('should remove nuget package', () => {
      const store = useConfigStore()

      store.addNugetPackage({ name: 'Serilog', version: '3.1.1' })
      store.addNugetPackage({ name: 'AutoMapper', version: '12.0.1' })
      store.removeNugetPackage('Serilog')

      expect(store.nugetPackages).toHaveLength(1)
      expect(store.nugetPackages[0].name).toBe('AutoMapper')
    })

    it('should add npm package', () => {
      const store = useConfigStore()

      const result = store.addNpmPackage({
        name: 'dayjs',
        version: '1.11.10'
      })

      expect(result).toBe(true)
      expect(store.npmPackages).toHaveLength(1)
    })

    it('should reject npm package conflicting with system packages', () => {
      const store = useConfigStore()

      const result = store.addNpmPackage({
        name: 'vue',
        version: '3.4.0'
      })

      expect(result).toBe(false)
      expect(store.npmPackages).toHaveLength(0)
    })

    it('should remove npm package', () => {
      const store = useConfigStore()

      store.addNpmPackage({ name: 'dayjs', version: '1.11.10' })
      store.addNpmPackage({ name: 'lodash-es', version: '4.17.21' })
      store.removeNpmPackage('dayjs')

      expect(store.npmPackages).toHaveLength(1)
      expect(store.npmPackages[0].name).toBe('lodash-es')
    })

    it('should compute system nuget packages based on config', () => {
      const store = useConfigStore()

      // 默认配置
      expect(store.systemNugetPackages).toContain('Serilog.AspNetCore')
      expect(store.systemNugetPackages).toContain('SqlSugarCore')

      // JWT 启用时
      store.updateConfig({ enableJwtAuth: true })
      expect(store.systemNugetPackages).toContain('Microsoft.AspNetCore.Authentication.JwtBearer')

      // Redis 缓存
      store.updateConfig({ cache: 'Redis' })
      expect(store.systemNugetPackages).toContain('StackExchange.Redis')
    })

    it('should compute system npm packages', () => {
      const store = useConfigStore()

      expect(store.systemNpmPackages).toContain('vue')
      expect(store.systemNpmPackages).toContain('vue-router')
      expect(store.systemNpmPackages).toContain('pinia')
      expect(store.systemNpmPackages).toContain('axios')
      expect(store.systemNpmPackages).toContain('element-plus')
    })

    describe('Async File Tree', () => {
      it('should have empty fileTree initially', () => {
        const store = useConfigStore()

        expect(store.fileTree).toEqual([])
      })

      it('should have treeLoading as boolean initially', () => {
        const store = useConfigStore()

        expect(typeof store.treeLoading).toBe('boolean')
      })
    })
  })
})
