import { describe, it, expect, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { defineComponent, h } from 'vue'
import OrmSelector from '@/components/OrmSelector.vue'

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

describe('OrmSelector', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  const mountComponent = () => {
    return mount(OrmSelector, {
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

  it('renders ORM options', () => {
    const wrapper = mountComponent()
    expect(wrapper.find('.el-form-item').exists()).toBe(true)
    expect(wrapper.text()).toContain('ORM 框架')
  })

  it('displays all 4 ORM options', () => {
    const wrapper = mountComponent()
    const options = wrapper.findAll('.el-radio-button')
    expect(options.length).toBe(4)
  })

  it('displays correct ORM labels', () => {
    const wrapper = mountComponent()
    const text = wrapper.text()
    expect(text).toContain('SqlSugar')
    expect(text).toContain('EF Core')
    expect(text).toContain('Dapper')
    expect(text).toContain('FreeSql')
  })

  it('displays ORM descriptions', () => {
    const wrapper = mountComponent()
    const text = wrapper.text()
    expect(text).toContain('国产高性能 ORM')
    expect(text).toContain('微软官方 ORM')
  })
})
