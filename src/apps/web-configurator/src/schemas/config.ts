import { z } from 'zod'
import { toTypedSchema } from '@vee-validate/zod'

// Package reference schema
const packageReferenceSchema = z.object({
  name: z.string().min(1, '包名不能为空'),
  version: z.string().regex(/^\d+\.\d+\.\d+$/, '版本号格式不正确 (x.y.z)'),
  source: z.string().url().optional()
})

// Zod schema for scaffold configuration
export const scaffoldConfigSchema = z.object({
  projectName: z
    .string()
    .min(1, '项目名称不能为空')
    .regex(/^[a-zA-Z][a-zA-Z0-9]*$/, '项目名称必须以字母开头，只能包含字母和数字'),
  namespace: z
    .string()
    .min(1, '命名空间不能为空')
    .regex(/^[a-zA-Z][a-zA-Z0-9.]*$/, '命名空间格式不正确'),
  architecture: z.enum(['Simple', 'CleanArchitecture', 'VerticalSlice', 'ModularMonolith']),
  orm: z.enum(['SqlSugar', 'EFCore', 'Dapper', 'FreeSql']),
  database: z.enum(['SQLite', 'MySQL', 'SQLServer']),
  cache: z.enum(['None', 'MemoryCache', 'Redis']),
  enableSwagger: z.boolean(),
  enableJwtAuth: z.boolean(),
  uiLibrary: z.enum(['ElementPlus', 'AntDesignVue', 'NaiveUI', 'TailwindHeadless', 'ShadcnVue', 'MateChat']),
  routerMode: z.enum(['Hash', 'History']),
  enableMockData: z.boolean(),
  nugetPackages: z.array(packageReferenceSchema).optional().default([]),
  npmPackages: z.array(packageReferenceSchema).optional().default([])
})

// VeeValidate typed schema
export const validationSchema = toTypedSchema(scaffoldConfigSchema)

// Type inference from schema
export type ScaffoldConfigSchema = z.infer<typeof scaffoldConfigSchema>
