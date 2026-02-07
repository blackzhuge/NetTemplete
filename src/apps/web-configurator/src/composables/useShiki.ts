import { ref, shallowRef } from 'vue'
import type { Highlighter, BundledLanguage } from 'shiki'

const highlighter = shallowRef<Highlighter | null>(null)
const loading = ref(false)
const loadPromise = ref<Promise<Highlighter> | null>(null)

const SUPPORTED_LANGUAGES: BundledLanguage[] = [
  'csharp',
  'typescript',
  'vue',
  'json',
  'html',
  'xml',
  'css',
  'scss',
  'markdown'
]

export function useShiki() {
  async function ensureLoaded(): Promise<Highlighter> {
    if (highlighter.value) {
      return highlighter.value
    }

    if (loadPromise.value) {
      return loadPromise.value
    }

    loading.value = true
    loadPromise.value = (async () => {
      const { createHighlighter } = await import('shiki')
      const hl = await createHighlighter({
        themes: ['github-light', 'github-dark'],
        langs: SUPPORTED_LANGUAGES
      })
      highlighter.value = hl
      loading.value = false
      return hl
    })()

    return loadPromise.value
  }

  async function highlight(code: string, language: string): Promise<string> {
    const hl = await ensureLoaded()
    const lang = SUPPORTED_LANGUAGES.includes(language as BundledLanguage)
      ? (language as BundledLanguage)
      : 'text'

    return hl.codeToHtml(code, {
      lang,
      theme: 'github-light'
    })
  }

  return {
    highlight,
    loading
  }
}
