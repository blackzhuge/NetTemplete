export type DatabaseProvider = 'SQLite' | 'MySQL' | 'SQLServer'
export type CacheProvider = 'None' | 'MemoryCache' | 'Redis'
export type RouterMode = 'Hash' | 'History'

export interface ScaffoldConfig {
  projectName: string
  namespace: string
  database: DatabaseProvider
  cache: CacheProvider
  enableSwagger: boolean
  enableJwtAuth: boolean
  routerMode: RouterMode
  enableMockData: boolean
}

export type ErrorCode = 'None' | 'ValidationError' | 'InvalidCombination' | 'TemplateError'

export interface ApiErrorResponse {
  error: string
  errorCode?: ErrorCode
}

export interface GenerationResult {
  success: boolean
  fileName?: string
  fileContent?: Blob
  errorMessage?: string
  errorCode?: ErrorCode
}

export interface FileTreeNode {
  name: string
  path: string
  isDirectory: boolean
  children?: FileTreeNode[]
}
