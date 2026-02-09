import { describe, it, expect, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { defineComponent, h, nextTick } from 'vue'
import PreviewDrawer from '@/components/PreviewDrawer.vue'
import { useConfigStore } from '@/stores/config'

// Simple stubs for Element Plus components
const stubs = {
  'el-drawer': defineComponent({
    props: ['modelValue', 'direction', 'size', 'title'],
    emits: ['update:modelValue', 'close'],
    setup(props, { emit, slots }) {
      return () => props.modelValue
        ? h('div', { class: 'el-drawer' }, [
            h('div', { class: 'el-drawer__header' }, props.title),
            h('div', { class: 'el-drawer__body' }, slots.default?.()),
            h('button', { class: 'close-btn', onClick: () => emit('close') }, 'Close')
          ])
        : null
    }
  }),
  'el-tabs': defineComponent({
    props: ['modelValue'],
    emits: ['update:modelValue'],
    setup(props, { emit, slots }) {
      return () => h('div', { class: 'el-tabs', 'data-active': props.modelValue }, [
        h('div', { class: 'el-tabs__header' }, [
          h('button', {
            class: 'tab-explorer',
            onClick: () => emit('update:modelValue', 'explorer')
          }, 'Explorer'),
          h('button', {
            class: 'tab-code',
            onClick: () => emit('update:modelValue', 'code')
          }, 'Code')
        ]),
        slots.default?.()
      ])
    }
  }),
  'el-tab-pane': defineComponent({
    props: ['label', 'name'],
    setup(_, { slots }) {
      return () => h('div', { class: 'el-tab-pane' }, slots.default?.())
    }
  }),
  'FileTreeView': defineComponent({
    props: ['theme'],
    setup() {
      return () => h('div', { class: 'file-tree-view' }, 'FileTreeView')
    }
  }),
  'CodePreview': defineComponent({
    setup() {
      return () => h('div', { class: 'code-preview' }, 'CodePreview')
    }
  })
}

describe('PreviewDrawer', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  const mountComponent = () => {
    return mount(PreviewDrawer, {
      global: {
        stubs
      }
    })
  }

  describe('Drawer 状态', () => {
    it('should not render drawer when showPreviewDrawer is false', () => {
      const store = useConfigStore()
      store.showPreviewDrawer = false

      const wrapper = mountComponent()
      expect(wrapper.find('.el-drawer').exists()).toBe(false)
    })

    it('should render drawer when showPreviewDrawer is true', async () => {
      const store = useConfigStore()
      store.openPreview()

      const wrapper = mountComponent()
      await nextTick()

      expect(wrapper.find('.el-drawer').exists()).toBe(true)
    })

    it('should call closePreview when drawer is closed', async () => {
      const store = useConfigStore()
      store.openPreview()

      const wrapper = mountComponent()
      await nextTick()

      const closeBtn = wrapper.find('.close-btn')
      await closeBtn.trigger('click')

      expect(store.showPreviewDrawer).toBe(false)
    })
  })

  describe('Tab 切换', () => {
    it('should default to explorer tab', async () => {
      const store = useConfigStore()
      store.openPreview()

      const wrapper = mountComponent()
      await nextTick()

      const tabs = wrapper.find('.el-tabs')
      expect(tabs.attributes('data-active')).toBe('explorer')
    })

    it('should switch to code tab when clicked', async () => {
      const store = useConfigStore()
      store.openPreview()

      const wrapper = mountComponent()
      await nextTick()

      const codeTab = wrapper.find('.tab-code')
      await codeTab.trigger('click')

      const tabs = wrapper.find('.el-tabs')
      expect(tabs.attributes('data-active')).toBe('code')
    })
  })

  describe('文件选择联动', () => {
    it('should switch to code tab when file is selected', async () => {
      const store = useConfigStore()
      store.openPreview()

      const wrapper = mountComponent()
      await nextTick()

      // Simulate file selection
      store.selectedFile = {
        name: 'Program.cs',
        path: 'src/MyProject.Api/Program.cs',
        isDirectory: false
      }
      await nextTick()

      const tabs = wrapper.find('.el-tabs')
      expect(tabs.attributes('data-active')).toBe('code')
    })

    it('should not switch tab when directory is selected', async () => {
      const store = useConfigStore()
      store.openPreview()

      const wrapper = mountComponent()
      await nextTick()

      // Simulate directory selection (should not trigger tab switch)
      store.selectedFile = {
        name: 'src',
        path: 'src',
        isDirectory: true
      }
      await nextTick()

      const tabs = wrapper.find('.el-tabs')
      expect(tabs.attributes('data-active')).toBe('explorer')
    })
  })

  describe('组件渲染', () => {
    it('should render FileTreeView in explorer pane', async () => {
      const store = useConfigStore()
      store.openPreview()

      const wrapper = mountComponent()
      await nextTick()

      expect(wrapper.find('.file-tree-view').exists()).toBe(true)
    })

    it('should render CodePreview in code pane', async () => {
      const store = useConfigStore()
      store.openPreview()

      const wrapper = mountComponent()
      await nextTick()

      expect(wrapper.find('.code-preview').exists()).toBe(true)
    })
  })
})
