import { describe, it, expect, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { defineComponent, h } from 'vue'
import UiLibrarySelector from '@/components/UiLibrarySelector.vue'

const stubs = {
  'el-form-item': defineComponent({
    props: ['label'],
    setup(props, { slots }) {
      return () => h('div', { class: 'el-form-item' }, [
        h('label', props.label),
        slots.default?.()
      ])
    }
  }),
  'el-radio-group': defineComponent({
    props: ['modelValue'],
    emits: ['update:modelValue'],
    setup(props, { emit, slots }) {
      return () => h('div', {
        class: 'el-radio-group',
        'data-value': props.modelValue,
        onClick: (e: Event) => {
          const target = e.target as HTMLElement
          const value = target.getAttribute('data-value')
          if (value) emit('update:modelValue', value)
        }
      }, slots.default?.())
    }
  }),
  'el-radio-button': defineComponent({
    props: ['value'],
    setup(props, { slots }) {
      return () => h('div', {
        class: 'el-radio-button',
        'data-value': props.value
      }, slots.default?.())
    }
  })
}

describe('UiLibrarySelector', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  const mountComponent = () => {
    return mount(UiLibrarySelector, {
      global: {
        stubs,
        provide: {
          [Symbol.for('vee-validate:form')]: {
            register: () => {},
            unregister: () => {}
          }
        }
      }
    })
  }

  it('renders UI library options', () => {
    const wrapper = mountComponent()
    expect(wrapper.find('.el-form-item').exists()).toBe(true)
    expect(wrapper.text()).toContain('UI 组件库')
  })

  it('displays all 6 UI library options', () => {
    const wrapper = mountComponent()
    const options = wrapper.findAll('.el-radio-button')
    expect(options.length).toBe(6)
  })

  it('displays correct UI library labels', () => {
    const wrapper = mountComponent()
    const text = wrapper.text()
    expect(text).toContain('Element Plus')
    expect(text).toContain('Ant Design Vue')
    expect(text).toContain('Naive UI')
    expect(text).toContain('Tailwind + Headless')
    expect(text).toContain('shadcn-vue')
    expect(text).toContain('MateChat')
  })

  it('displays UI library descriptions', () => {
    const wrapper = mountComponent()
    const text = wrapper.text()
    expect(text).toContain('Vue 3 主流组件库')
    expect(text).toContain('企业级 UI 设计')
    expect(text).toContain('AI 对话专用 UI')
  })
})
