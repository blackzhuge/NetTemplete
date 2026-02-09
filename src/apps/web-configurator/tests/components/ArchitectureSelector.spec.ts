import { describe, it, expect, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { defineComponent, h } from 'vue'
import ArchitectureSelector from '@/components/ArchitectureSelector.vue'

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

describe('ArchitectureSelector', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  const mountComponent = () => {
    return mount(ArchitectureSelector, {
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

  it('renders architecture options', () => {
    const wrapper = mountComponent()
    expect(wrapper.find('.el-form-item').exists()).toBe(true)
    expect(wrapper.text()).toContain('架构风格')
  })

  it('displays all 4 architecture options', () => {
    const wrapper = mountComponent()
    const options = wrapper.findAll('.el-radio-button')
    expect(options.length).toBe(4)
  })

  it('displays correct architecture labels', () => {
    const wrapper = mountComponent()
    const text = wrapper.text()
    expect(text).toContain('Simple')
    expect(text).toContain('Clean Architecture')
    expect(text).toContain('Vertical Slice')
    expect(text).toContain('Modular Monolith')
  })

  it('displays architecture descriptions', () => {
    const wrapper = mountComponent()
    const text = wrapper.text()
    expect(text).toContain('单项目结构')
    expect(text).toContain('四层分离')
  })
})
