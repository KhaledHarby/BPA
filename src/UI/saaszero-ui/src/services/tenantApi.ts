import { baseApi } from '../store'

export type TenantDto = { id: string; name: string; displayName?: string; isActive: boolean; culture?: string }
export type CreateTenantRequest = { name: string; displayName?: string; culture?: string; connectionString?: string }
export type UpdateTenantRequest = { displayName?: string; isActive?: boolean; culture?: string; connectionString?: string }

export const tenantApi = baseApi.enhanceEndpoints({ addTagTypes: ['Tenants'] }).injectEndpoints({
  endpoints: (builder) => ({
    getTenants: builder.query<TenantDto[], void>({
      query: () => ({ url: '/api/tenants' }),
      providesTags: ['Tenants'],
    }),
    createTenant: builder.mutation<string, CreateTenantRequest>({
      query: (body) => ({ url: '/api/tenants', method: 'POST', body }),
      invalidatesTags: ['Tenants'],
    }),
    login: builder.mutation<{ token: string; expiresAtUtc: string }, { userName: string; password: string; tenantId: string }>({
      query: (body) => ({ url: '/api/auth/login', method: 'POST', body }),
    }),
  }),
  overrideExisting: false,
})

export const { useGetTenantsQuery, useCreateTenantMutation, useLoginMutation } = tenantApi