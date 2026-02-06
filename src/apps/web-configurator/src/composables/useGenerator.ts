import { ref } from 'vue'
import { AxiosError } from 'axios'
import { generateScaffold } from '@/api/generator'
import { useConfigStore } from '@/stores/config'
import { ElMessage } from 'element-plus'
import type { ApiErrorResponse } from '@/types'

export function useGenerator() {
  const store = useConfigStore()
  const downloading = ref(false)

  async function generate() {
    if (downloading.value) return

    downloading.value = true
    store.setLoading(true)
    store.setError(null)

    try {
      const blob = await generateScaffold(store.config)
      downloadBlob(blob, `${store.config.projectName}.zip`)
      ElMessage.success('项目生成成功！')
    } catch (err: unknown) {
      let message = '生成失败，请重试'
      if (err instanceof AxiosError) {
        const data = err.response?.data as ApiErrorResponse | undefined
        message = data?.error || message
      }
      store.setError(message)
    } finally {
      downloading.value = false
      store.setLoading(false)
    }
  }

  function downloadBlob(blob: Blob, fileName: string) {
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = fileName
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
  }

  return {
    downloading,
    generate
  }
}
