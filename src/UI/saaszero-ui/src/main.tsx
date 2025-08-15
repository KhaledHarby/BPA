import React from 'react'
import ReactDOM from 'react-dom/client'
import { Provider } from 'react-redux'
import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import { CssBaseline, ThemeProvider, createTheme } from '@mui/material'
import { store } from './store'
import App from './App'

const router = createBrowserRouter([
  {
    path: '/',
    element: <App />,
    children: [
      { path: 'tenants', lazy: () => import('./pages/Tenants').then(m => ({ Component: m.default })) },
      { path: 'login', lazy: () => import('./pages/Login').then(m => ({ Component: m.default })) },
    ],
  },
])

const theme = createTheme({
  palette: { mode: 'light' },
})

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <Provider store={store}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <RouterProvider router={router} />
      </ThemeProvider>
    </Provider>
  </React.StrictMode>,
)
