import { configureStore, createSlice } from '@reduxjs/toolkit'
import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react'

type AuthState = { token: string | null; tenantId: string | null }

const initialAuthState: AuthState = { token: null, tenantId: null }

const authSlice = createSlice({
  name: 'auth',
  initialState: initialAuthState,
  reducers: {
    setAuth(state, action: { payload: { token: string; tenantId: string } }) {
      state.token = action.payload.token
      state.tenantId = action.payload.tenantId
    },
    clearAuth(state) {
      state.token = null
      state.tenantId = null
    },
  },
})

export const { setAuth, clearAuth } = authSlice.actions

export const baseApi = createApi({
  reducerPath: 'api',
  baseQuery: fetchBaseQuery({
    baseUrl: import.meta.env.VITE_API_BASE_URL,
    prepareHeaders: (headers, { getState }) => {
      const state = getState() as { auth: AuthState }
      if (state.auth.token) headers.set('Authorization', `Bearer ${state.auth.token}`)
      if (state.auth.tenantId) headers.set('X-Tenant-Id', state.auth.tenantId)
      return headers
    },
  }),
  endpoints: () => ({}),
})

export const store = configureStore({
  reducer: {
    auth: authSlice.reducer,
    [baseApi.reducerPath]: baseApi.reducer,
  },
  middleware: (gDM) => gDM().concat(baseApi.middleware),
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch