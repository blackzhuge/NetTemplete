import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { defineComponent, h } from 'vue'
import PackageSelector from '@/components/PackageSelector.vue'
import type { PackageReference } from '@/types/packages'
import * as packagesApi from '@/api/packages'

// Mock Element Plus
vi.mock('element-plus', async () => {
  return {
    ElMessage: {
      warning: vi.fn(),
      error: vi.fn(),
      success: vi.fn()
    }
  }
})

// Mock packages API
vi.mock('@/api/packages', () => ({
  searchPackages: vi.fn(),
  getPackageVersions: vi.fn()
}))

// Simple stubs for Element Plus components
const stubs = {
  'el-input': defineComponent({
    props: ['modelValue', 'placeholder', 'clearable'],
    emits: ['update:modelValue', 'input'],
    setup(props, { emit, slots }) {
      return () => h('div', { class: 'el-input' }, [
        h('input', {
          value: props.modelValue,
          placeholder: props.placeholder,
          onInput: (e: Event) => {
            const value = (e.target as HTMLInputElement).value
            emit('update:modelValue', value)
            emit('input', value)
          }
        }),
        slots.append?.()
      ])
    }
  }),
  'el-button': defineComponent({
    props: ['type', 'size'],
    setup(_, { slots }) {
      return () => h('button', { class: 'el-button' }, slots.default?.())
    }
  }),
  'el-select': defineComponent({
    props: ['modelValue', 'loading', 'size'],
    emits: ['update:modelValue'],
    setup(props, { emit, slots }) {
      return () => h('select', {
        class: 'el-select',
        value: props.modelValue,
        onChange: (e: Event) => emit('update:modelValue', (e.target as HTMLSelectElement).value)
      }, slots.default?.())
    }
  }),
  'el-option': defineComponent({
    props: ['value', 'label'],
    setup(props) {
      return () => h('option', { value: props.value }, props.label)
    }
  }),
  'el-tag': defineComponent({
    props: {
      closable: { type: Boolean, default: false }
    },
    emits: ['close'],
    setup(props, { emit, slots, attrs }) {
      // Handle closable as boolean attribute (no value = true)
      const isClosable = props.closable || attrs.closable !== undefined
      return () => h('span', { class: 'el-tag' }, [
        slots.default?.(),
        isClosable ? h('button', { class: 'close-btn', onClick: () => emit('close') }, '×') : null
      ])
    }
  }),
  'el-popover': defineComponent({
    props: ['visible', 'placement', 'width', 'trigger'],
    setup(_, { slots }) {
      return () => h('div', { class: 'el-popover' }, [slots.reference?.(), slots.default?.()])
    }
  }),
  'el-icon': defineComponent({
    setup(_, { slots }) {
      return () => h('span', { class: 'el-icon' }, slots.default?.())
    }
  }),
  'arrow-down': { template: '<span>▼</span>' },
  'loading': { template: '<span>⏳</span>' }
}

describe('PackageSelector', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
    vi.clearAllMocks()
  })

  const mountComponent = (props: Partial<{
    managerType: 'nuget' | 'npm'
    modelValue: PackageReference[]
    systemPackages: string[]
  }> = {}) => {
    return mount(PackageSelector, {
      props: {
        managerType: 'nuget',
        modelValue: [],
        systemPackages: [],
        ...props
      },
      global: {
        stubs
      }
    })
  }

  describe('搜索功能', () => {
    it('should debounce search by 300ms', async () => {
      const mockSearch = vi.mocked(packagesApi.searchPackages)
      mockSearch.mockResolvedValue({ items: [], totalCount: 0 })

      const wrapper = mountComponent()
      const input = wrapper.find('input')

      await input.setValue('serilog')
      expect(mockSearch).not.toHaveBeenCalled()

      vi.advanceTimersByTime(299)
      expect(mockSearch).not.toHaveBeenCalled()

      vi.advanceTimersByTime(1)
      await flushPromises()
      expect(mockSearch).toHaveBeenCalledWith('nuget', 'serilog', undefined)
    })

    it('should clear results when query is empty', async () => {
      const mockSearch = vi.mocked(packagesApi.searchPackages)
      mockSearch.mockResolvedValue({
        items: [{ name: 'Serilog', version: '3.1.1', description: 'Logging' }],
        totalCount: 1
      })

      const wrapper = mountComponent()
      const input = wrapper.find('input')

      await input.setValue('serilog')
      vi.advanceTimersByTime(300)
      await flushPromises()

      expect(wrapper.findAll('.result-item')).toHaveLength(1)

      await input.setValue('')
      await flushPromises()
      expect(wrapper.findAll('.result-item')).toHaveLength(0)
    })

    it('should display search results', async () => {
      const mockSearch = vi.mocked(packagesApi.searchPackages)
      mockSearch.mockResolvedValue({
        items: [
          { name: 'Serilog', version: '3.1.1', description: 'Logging framework' },
          { name: 'Serilog.Sinks.Console', version: '5.0.0', description: 'Console sink' }
        ],
        totalCount: 2
      })

      const wrapper = mountComponent()
      const input = wrapper.find('input')

      await input.setValue('serilog')
      vi.advanceTimersByTime(300)
      await flushPromises()

      const results = wrapper.findAll('.result-item')
      expect(results).toHaveLength(2)
      expect(wrapper.find('.pkg-name').text()).toBe('Serilog')
    })
  })

  describe('冲突检测', () => {
    it('should reject packages that conflict with system packages', async () => {
      const { ElMessage } = await import('element-plus')
      const mockSearch = vi.mocked(packagesApi.searchPackages)
      mockSearch.mockResolvedValue({
        items: [{ name: 'SqlSugarCore', version: '5.1.0', description: 'ORM' }],
        totalCount: 1
      })

      const wrapper = mountComponent({
        systemPackages: ['SqlSugarCore']
      })

      const input = wrapper.find('input')
      await input.setValue('sqlsugar')
      vi.advanceTimersByTime(300)
      await flushPromises()

      const resultItem = wrapper.find('.result-item')
      await resultItem.trigger('click')

      expect(ElMessage.warning).toHaveBeenCalledWith(
        expect.stringContaining('SqlSugarCore')
      )
    })

    it('should reject duplicate packages (case insensitive)', async () => {
      const { ElMessage } = await import('element-plus')
      const mockSearch = vi.mocked(packagesApi.searchPackages)
      mockSearch.mockResolvedValue({
        items: [{ name: 'Newtonsoft.Json', version: '13.0.3', description: 'JSON' }],
        totalCount: 1
      })

      const wrapper = mountComponent({
        modelValue: [{ name: 'newtonsoft.json', version: '13.0.0' }]
      })

      const input = wrapper.find('input')
      await input.setValue('newtonsoft')
      vi.advanceTimersByTime(300)
      await flushPromises()

      const resultItem = wrapper.find('.result-item')
      await resultItem.trigger('click')

      expect(ElMessage.warning).toHaveBeenCalledWith(
        expect.stringContaining('已添加')
      )
    })
  })

  describe('包管理', () => {
    it('should display selected packages as tags', () => {
      const wrapper = mountComponent({
        modelValue: [
          { name: 'Serilog', version: '3.1.1' },
          { name: 'AutoMapper', version: '12.0.1' }
        ]
      })

      const tags = wrapper.findAll('.el-tag')
      expect(tags).toHaveLength(2)
      expect(tags[0].text()).toContain('Serilog@3.1.1')
      expect(tags[1].text()).toContain('AutoMapper@12.0.1')
    })

    it('should emit update when removing package', async () => {
      const wrapper = mountComponent({
        modelValue: [
          { name: 'Serilog', version: '3.1.1' },
          { name: 'AutoMapper', version: '12.0.1' }
        ]
      })

      const closeButtons = wrapper.findAll('.close-btn')
      expect(closeButtons.length).toBeGreaterThan(0)
      await closeButtons[0].trigger('click')

      const emitted = wrapper.emitted('update:modelValue')
      expect(emitted).toBeTruthy()
      expect(emitted![0][0]).toHaveLength(1)
    })
  })

  describe('版本选择', () => {
    it('should load versions when package is selected', async () => {
      const mockSearch = vi.mocked(packagesApi.searchPackages)
      const mockVersions = vi.mocked(packagesApi.getPackageVersions)

      mockSearch.mockResolvedValue({
        items: [{ name: 'Serilog', version: '3.1.1', description: 'Logging' }],
        totalCount: 1
      })
      mockVersions.mockResolvedValue(['3.1.1', '3.1.0', '3.0.0'])

      const wrapper = mountComponent()

      const input = wrapper.find('input')
      await input.setValue('serilog')
      vi.advanceTimersByTime(300)
      await flushPromises()

      const resultItem = wrapper.find('.result-item')
      await resultItem.trigger('click')
      await flushPromises()

      expect(mockVersions).toHaveBeenCalledWith('nuget', 'Serilog', undefined)
      expect(wrapper.find('.version-selector').exists()).toBe(true)
    })
  })

  describe('npm 支持', () => {
    it('should search npm packages when managerType is npm', async () => {
      const mockSearch = vi.mocked(packagesApi.searchPackages)
      mockSearch.mockResolvedValue({ items: [], totalCount: 0 })

      const wrapper = mountComponent({ managerType: 'npm' })
      const input = wrapper.find('input')

      await input.setValue('axios')
      vi.advanceTimersByTime(300)
      await flushPromises()

      expect(mockSearch).toHaveBeenCalledWith('npm', 'axios', undefined)
    })
  })
})
