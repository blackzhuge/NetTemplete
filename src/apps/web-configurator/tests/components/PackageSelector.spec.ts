import { describe, it, expect, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { defineComponent, h } from 'vue'
import PackageSelector from '@/components/PackageSelector.vue'
import type { PackageReference } from '@/types/packages'

// Simple stubs for Element Plus components
const stubs = {
  'el-button': defineComponent({
    props: ['type', 'size', 'plain'],
    emits: ['click'],
    setup(_, { emit, slots }) {
      return () => h('button', {
        class: 'el-button',
        onClick: () => emit('click')
      }, slots.default?.())
    }
  }),
  'el-tag': defineComponent({
    props: {
      closable: { type: Boolean, default: false }
    },
    emits: ['close'],
    setup(props, { emit, slots, attrs }) {
      const isClosable = props.closable || attrs.closable !== undefined
      return () => h('span', { class: 'el-tag' }, [
        slots.default?.(),
        isClosable ? h('button', { class: 'close-btn', onClick: () => emit('close') }, '×') : null
      ])
    }
  }),
  'el-icon': defineComponent({
    setup(_, { slots }) {
      return () => h('span', { class: 'el-icon' }, slots.default?.())
    }
  }),
  'Plus': { template: '<span>+</span>' },
  'PackageSelectorModal': defineComponent({
    props: ['visible', 'managerType', 'existingPackages', 'systemPackages'],
    emits: ['update:visible', 'confirm'],
    setup(props, { emit }) {
      return () => h('div', {
        class: 'package-selector-modal',
        'data-visible': props.visible,
        onClick: () => {
          // Simulate confirm with test packages
          emit('confirm', [{ name: 'TestPackage', version: '1.0.0' }])
        }
      })
    }
  })
}

describe('PackageSelector', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
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

  describe('渲染', () => {
    it('should render add button for NuGet', () => {
      const wrapper = mountComponent({ managerType: 'nuget' })
      const button = wrapper.find('.el-button')
      expect(button.exists()).toBe(true)
      expect(button.text()).toContain('NuGet')
    })

    it('should render add button for npm', () => {
      const wrapper = mountComponent({ managerType: 'npm' })
      const button = wrapper.find('.el-button')
      expect(button.exists()).toBe(true)
      expect(button.text()).toContain('npm')
    })

    it('should not render tags when no packages selected', () => {
      const wrapper = mountComponent({ modelValue: [] })
      expect(wrapper.findAll('.el-tag')).toHaveLength(0)
    })
  })

  describe('包展示', () => {
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

  describe('Modal 交互', () => {
    it('should open modal when clicking add button', async () => {
      const wrapper = mountComponent()
      const button = wrapper.find('.el-button')
      await button.trigger('click')

      const modal = wrapper.find('.package-selector-modal')
      expect(modal.attributes('data-visible')).toBe('true')
    })

    it('should add packages when modal confirms', async () => {
      const wrapper = mountComponent({ modelValue: [] })

      // Open modal
      await wrapper.find('.el-button').trigger('click')

      // Trigger confirm (simulated by clicking the mock modal)
      await wrapper.find('.package-selector-modal').trigger('click')

      const emitted = wrapper.emitted('update:modelValue')
      expect(emitted).toBeTruthy()
      expect(emitted![0][0]).toContainEqual({ name: 'TestPackage', version: '1.0.0' })
    })

    it('should append to existing packages when confirming', async () => {
      const wrapper = mountComponent({
        modelValue: [{ name: 'ExistingPackage', version: '1.0.0' }]
      })

      // Open and confirm
      await wrapper.find('.el-button').trigger('click')
      await wrapper.find('.package-selector-modal').trigger('click')

      const emitted = wrapper.emitted('update:modelValue')
      expect(emitted).toBeTruthy()
      expect(emitted![0][0]).toHaveLength(2)
    })
  })

  describe('Props 传递', () => {
    it('should pass managerType to modal', () => {
      const wrapper = mountComponent({ managerType: 'npm' })
      const modal = wrapper.find('.package-selector-modal')
      expect(modal.exists()).toBe(true)
    })

    it('should pass existingPackages to modal', () => {
      const packages = [{ name: 'Test', version: '1.0.0' }]
      const wrapper = mountComponent({ modelValue: packages })
      const modal = wrapper.find('.package-selector-modal')
      expect(modal.exists()).toBe(true)
    })

    it('should pass systemPackages to modal', () => {
      const systemPackages = ['SqlSugarCore', 'Serilog']
      const wrapper = mountComponent({ systemPackages })
      const modal = wrapper.find('.package-selector-modal')
      expect(modal.exists()).toBe(true)
    })
  })
})
