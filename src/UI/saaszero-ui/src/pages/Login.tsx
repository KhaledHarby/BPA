import * as React from 'react'
import { Button, Container, Stack, TextField, Typography } from '@mui/material'
import { useLoginMutation } from '../services/tenantApi'
import { useDispatch } from 'react-redux'
import { setAuth } from '../store'
import { useNavigate } from 'react-router-dom'

export default function Login() {
  const [userName, setUserName] = React.useState('admin')
  const [password, setPassword] = React.useState('Admin123!@#')
  const [tenantId, setTenantId] = React.useState(import.meta.env.VITE_DEFAULT_TENANT_ID || '')
  const [login, { isLoading }] = useLoginMutation()
  const dispatch = useDispatch()
  const navigate = useNavigate()

  const onSubmit = async () => {
    const res = await login({ userName, password, tenantId }).unwrap()
    dispatch(setAuth({ token: res.token, tenantId }))
    navigate('/')
  }

  return (
    <Container maxWidth="xs">
      <Typography variant="h4" sx={{ my: 2 }}>Login</Typography>
      <Stack spacing={2}>
        <TextField label="Tenant Id" value={tenantId} onChange={(e) => setTenantId(e.target.value)} />
        <TextField label="User Name" value={userName} onChange={(e) => setUserName(e.target.value)} />
        <TextField label="Password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
        <Button variant="contained" onClick={onSubmit} disabled={isLoading}>Login</Button>
      </Stack>
    </Container>
  )
}