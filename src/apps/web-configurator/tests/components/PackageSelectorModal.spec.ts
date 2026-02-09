import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { defineComponent, h } from 'vue'
import PackageSelectorModal from '@/components/PackageSelectorModal.vue'
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
  'el-dialog': defineComponent({
    props: ['modelValue', 'title', 'width'],
    emits: ['update:modelValue', 'close'],
    setup(props, { slots }) {
      return () => props.modelValue
        ? h('div', { class: 'el-dialog' }, [
            h('div', { class: 'el-dialog__header' }, props.title),
            h('div', { class: 'el-dialog__body' }, slots.default?.()),
            h('div', { class: 'el-dialog__footer' }, slots.footer?.())
          ])
        : null
    }
  }),
  'el-input': defineComponent({
    props: ['modelValue', 'placeholder', 'clearable', 'prefixIcon'],
    emits: ['update:modelValue', 'input'],
    setup(props, { emit }) {
      return () => h('input', {
        class: 'el-input',
        value: props.modelValue,
        placeholder: props.placeholder,
        onInput: (e: Event) => {
          const value = (e.target as HTMLInputElement).value
          emit('update:modelValue', value)
          emit('input', value)
        }
      })
    }
  }),
  'el-select': defineComponent({
    props: ['modelValue', 'loading', 'size', 'valueKey'],
    emits: ['update:modelValue'],
    setup(props, { emit, slots }) {
      return () => h('select', {
        class: 'el-select',
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
  'el-button': defineComponent({
    props: ['type', 'size', 'disabled', 'plain', 'text'],
    emits: ['click'],
    setup(props, { emit, slots }) {
      return () => h('button', {
        class: 'el-button',
        disabled: props.disabled,
        onClick: () => emit('click')
      }, slots.default?.())
    }
  }),
  'el-tag': defineComponent({
    props: ['closable'],
    emits: ['close'],
    setup(props, { emit, slots }) {
      return () => h('span', { class: 'el-tag' }, [
        slots.default?.(),
        props.closable ? h('button', { class: 'close-btn', onClick: () => emit('close') }, '×') : null
      ])
    }
  }),
  'el-icon': defineComponent({
    setup(_, { slots }) {
      return () => h('span', { class: 'el-icon' }, slots.default?.())
    }
  }),
  'Search': { template: '<span>🔍</span>' },
  'Plus': { template: '<span>+</span>' },
  'Check': { template: '<span>✓</span>' },
  'Download': { template: '<span>⬇</span>' },
  'Loading': { template: '<span>⏳</span>' }
}

describe('PackageSelectorModal', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
    vi.clearAllMocks()
  })

  const mountComponent = (props: Partial<{
    visible: boolean
    managerType: 'nuget' | 'npm'
    existingPackages: Array<{ name: string; version: string }>
    systemPackages: string[]
  }> = {}) => {
    return mount(PackageSelectorModal, {
      props: {
        visible: true,
        managerType: 'nuget',
        existingPackages: [],
        systemPackages: [],
        ...props
      },
      global: {
        stubs
      }
    })
  }

  describe('搜索功能', () => {
    it('should trigger search after debounce', async () => {
      const mockSearch = vi.mocked(packagesApi.searchPackages)
      mockSearch.mockResolvedValue({ items: [], totalCount: 0 })

      const wrapper = mountComponent()
      const input = wrapper.find('.el-input')

      await input.setValue('serilog')
      expect(mockSearch).not.toHaveBeenCalled()

      vi.advanceTimersByTime(300)
      await flushPromises()
      expect(mockSearch).toHaveBeenCalledWith('nuget', 'serilog', undefined)
    })

    it('should display search results', async () => {
      const mockSearch = vi.mocked(packagesApi.searchPackages)
      mockSearch.mockResolvedValue({
        items: [
          { name: 'Serilog', version: '3.1.1', description: 'Logging', downloadCount: 100000000 },
          { name: 'Serilog.Sinks.Console', version: '5.0.0', description: 'Console sink', downloadCount: 50000000 }
        ],
        totalCount: 2
      })

      const wrapper = mountComponent()
      const input = wrapper.find('.el-input')

      await input.setValue('serilog')
      vi.advanceTimersByTime(300)
      await flushPromises()

      const results = wrapper.findAll('.result-item')
      expect(results).toHaveLength(2)
    })
  })

  describe('排序功能', () => {
    it('should sort by downloads by default', async () => {
      const mockSearch = vi.mocked(packagesApi.searchPackages)
      mockSearch.mockResolvedValue({
        items: [
          { name: 'PackageA', version: '1.0.0', description: 'A', downloadCount: 1000 },
          { name: 'PackageB', version: '1.0.0', description: 'B', downloadCount: 5000 }
        ],
        totalCount: 2
      })

      const wrapper = mountComponent()
      const input = wrapper.find('.el-input')

      await input.setValue('package')
      vi.advanceTimersByTime(300)
      await flushPromises()

      const names = wrapper.findAll('.pkg-name')
      expect(names[0].text()).toBe('PackageB') // Higher downloads first
      expect(names[1].text()).toBe('PackageA')
    })
  })

  describe('包选择', () => {
    it('should add package to selection on click', async () => {
      const mockSearch = vi.mocked(packagesApi.searchPackages)
      const mockVersions = vi.mocked(packagesApi.getPackageVersions)
      mockSearch.mockResolvedValue({
        items: [{ name: 'Serilog', version: '3.1.1', description: 'Logging' }],
        totalCount: 1
      })
      mockVersions.mockResolvedValue(['3.1.1', '3.1.0', '3.0.0'])

      const wrapper = mountComponent()
      const input = wrapper.find('.el-input')

      await input.setValue('serilog')
      vi.advanceTimersByTime(300)
      await flushPromises()

      const resultItem = wrapper.find('.result-item')
      await resultItem.trigger('click')
      await flushPromises()

      expect(wrapper.find('.selected-section').exists()).toBe(true)
      expect(wrapper.find('.selected-item').text()).toContain('Serilog')
    })

    it('should reject system packages', async () => {
      const { ElMessage } = await import('element-plus')
      const mockSearch = vi.mocked(packagesApi.searchPackages)
      mockSearch.mockResolvedValue({
        items: [{ name: 'SqlSugarCore', version: '5.1.0', description: 'ORM' }],
        totalCount: 1
      })

      const wrapper = mountComponent({
        systemPackages: ['SqlSugarCore']
      })

      const input = wrapper.find('.el-input')
      await input.setValue('sqlsugar')
      vi.advanceTimersByTime(300)
      await flushPromises()

      const resultItem = wrapper.find('.result-item')
      await resultItem.trigger('click')

      expect(ElMessage.warning).toHaveBeenCalled()
    })
  })

  describe('确认和取消', () => {
    it('should emit confirm with selected packages', async () => {
      const mockSearch = vi.mocked(packagesApi.searchPackages)
      const mockVersions = vi.mocked(packagesApi.getPackageVersions)
      mockSearch.mockResolvedValue({
        items: [{ name: 'Serilog', version: '3.1.1', description: 'Logging' }],
        totalCount: 1
      })
      mockVersions.mockResolvedValue(['3.1.1'])

      const wrapper = mountComponent()
      const input = wrapper.find('.el-input')

      await input.setValue('serilog')
      vi.advanceTimersByTime(300)
      await flushPromises()

      await wrapper.find('.result-item').trigger('click')
      await flushPromises()

      // Find and click confirm button
      const buttons = wrapper.findAll('.el-button')
      const confirmBtn = buttons.find(b => b.text().includes('添加'))
      await confirmBtn?.trigger('click')

      const emitted = wrapper.emitted('confirm')
      expect(emitted).toBeTruthy()
      expect(emitted![0][0]).toHaveLength(1)
    })
  })

  describe('npm 支持', () => {
    it('should search npm packages when managerType is npm', async () => {
      const mockSearch = vi.mocked(packagesApi.searchPackages)
      mockSearch.mockResolvedValue({ items: [], totalCount: 0 })

      const wrapper = mountComponent({ managerType: 'npm' })
      const input = wrapper.find('.el-input')

      await input.setValue('axios')
      vi.advanceTimersByTime(300)
      await flushPromises()

      expect(mockSearch).toHaveBeenCalledWith('npm', 'axios', undefined)
    })
  })
})
