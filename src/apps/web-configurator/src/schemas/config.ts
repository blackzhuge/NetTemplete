import { z } from 'zod'
import { toTypedSchema } from '@vee-validate/zod'

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
  database: z.enum(['SQLite', 'MySQL', 'SQLServer']),
  cache: z.enum(['None', 'MemoryCache', 'Redis']),
  enableSwagger: z.boolean(),
  enableJwtAuth: z.boolean(),
  routerMode: z.enum(['Hash', 'History']),
  enableMockData: z.boolean()
})

// VeeValidate typed schema
export const validationSchema = toTypedSchema(scaffoldConfigSchema)

// Type inference from schema
export type ScaffoldConfigSchema = z.infer<typeof scaffoldConfigSchema>
