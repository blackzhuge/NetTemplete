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

  describe('分栏布局', () => {
    it('should render split layout with explorer and code panels', async () => {
      const store = useConfigStore()
      store.openPreview()

      const wrapper = mountComponent()
      await nextTick()

      expect(wrapper.find('.preview-split').exists()).toBe(true)
      expect(wrapper.find('.explorer-panel').exists()).toBe(true)
      expect(wrapper.find('.code-panel').exists()).toBe(true)
    })

    it('should show both FileTreeView and CodePreview simultaneously', async () => {
      const store = useConfigStore()
      store.openPreview()

      const wrapper = mountComponent()
      await nextTick()

      expect(wrapper.find('.file-tree-view').exists()).toBe(true)
      expect(wrapper.find('.code-preview').exists()).toBe(true)
    })

    it('should have panel headers for explorer and code', async () => {
      const store = useConfigStore()
      store.openPreview()

      const wrapper = mountComponent()
      await nextTick()

      const headers = wrapper.findAll('.panel-header')
      expect(headers).toHaveLength(2)
      expect(headers[0].text()).toBe('Explorer')
      expect(headers[1].text()).toBe('Code')
    })
  })

  describe('组件渲染', () => {
    it('should render FileTreeView in explorer panel', async () => {
      const store = useConfigStore()
      store.openPreview()

      const wrapper = mountComponent()
      await nextTick()

      const explorerPanel = wrapper.find('.explorer-panel')
      expect(explorerPanel.find('.file-tree-view').exists()).toBe(true)
    })

    it('should render CodePreview in code panel', async () => {
      const store = useConfigStore()
      store.openPreview()

      const wrapper = mountComponent()
      await nextTick()

      const codePanel = wrapper.find('.code-panel')
      expect(codePanel.find('.code-preview').exists()).toBe(true)
    })
  })
})
