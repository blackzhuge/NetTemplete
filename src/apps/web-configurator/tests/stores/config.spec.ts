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
  })

  it('should update config partially', () => {
    const store = useConfigStore()

    store.updateConfig({ projectName: 'NewProject' })

    expect(store.config.projectName).toBe('NewProject')
    expect(store.config.namespace).toBe('MyProject')
  })

  it('should generate correct file tree based on config', () => {
    const store = useConfigStore()
    store.updateConfig({ projectName: 'TestApp' })

    const tree = store.fileTree
    expect(tree).toHaveLength(1)
    expect(tree[0].name).toBe('src')
    expect(tree[0].children).toBeDefined()

    const apiProject = tree[0].children?.find(c => c.name === 'TestApp.Api')
    expect(apiProject).toBeDefined()
  })

  it('should include JWT files when jwtAuth is enabled', () => {
    const store = useConfigStore()
    store.updateConfig({ enableJwtAuth: true })

    const tree = store.fileTree
    const apiProject = tree[0].children?.find(c => c.name.endsWith('.Api'))
    const extensions = apiProject?.children?.find(c => c.name === 'Extensions')

    expect(extensions?.children?.some(f => f.name === 'JwtSetup.cs')).toBe(true)
  })

  it('should exclude JWT files when jwtAuth is disabled', () => {
    const store = useConfigStore()
    store.updateConfig({ enableJwtAuth: false })

    const tree = store.fileTree
    const apiProject = tree[0].children?.find(c => c.name.endsWith('.Api'))
    const extensions = apiProject?.children?.find(c => c.name === 'Extensions')

    expect(extensions?.children?.some(f => f.name === 'JwtSetup.cs')).toBe(false)
  })

  it('should include Redis files when cache is Redis', () => {
    const store = useConfigStore()
    store.updateConfig({ cache: 'Redis' })

    const tree = store.fileTree
    const apiProject = tree[0].children?.find(c => c.name.endsWith('.Api'))
    const extensions = apiProject?.children?.find(c => c.name === 'Extensions')

    expect(extensions?.children?.some(f => f.name === 'RedisSetup.cs')).toBe(true)
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
})
