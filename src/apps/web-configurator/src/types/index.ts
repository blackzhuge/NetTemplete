import type { PackageReference } from './packages'

export type DatabaseProvider = 'SQLite' | 'MySQL' | 'SQLServer'
export type CacheProvider = 'None' | 'MemoryCache' | 'Redis'
export type RouterMode = 'Hash' | 'History'
export type ArchitectureStyle = 'Simple' | 'CleanArchitecture' | 'VerticalSlice' | 'ModularMonolith'
export type OrmProvider = 'SqlSugar' | 'EFCore' | 'Dapper' | 'FreeSql'
export type UiLibrary = 'ElementPlus' | 'AntDesignVue' | 'NaiveUI' | 'TailwindHeadless' | 'ShadcnVue' | 'MateChat'

export interface ScaffoldConfig {
  projectName: string
  namespace: string
  architecture: ArchitectureStyle
  orm: OrmProvider
  database: DatabaseProvider
  cache: CacheProvider
  enableSwagger: boolean
  enableJwtAuth: boolean
  uiLibrary: UiLibrary
  routerMode: RouterMode
  enableMockData: boolean
  nugetPackages?: PackageReference[]
  npmPackages?: PackageReference[]
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

export interface ScaffoldPreset {
  id: string
  name: string
  description: string
  isDefault: boolean
  tags: string[]
  config: {
    basic: {
      projectName: string
      namespace: string
    }
    backend: {
      architecture: ArchitectureStyle
      orm: OrmProvider
      database: DatabaseProvider
      cache: CacheProvider
      swagger: boolean
      jwtAuth: boolean
    }
    frontend: {
      uiLibrary: UiLibrary
      routerMode: RouterMode
      mockData: boolean
    }
  }
}

export interface PreviewFileResponse {
  content: string
  language: string
  outputPath: string
}
