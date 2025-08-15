import * as React from 'react'
import { Box, Button, CircularProgress, Container, Stack, TextField, Typography } from '@mui/material'
import { useCreateTenantMutation, useGetTenantsQuery } from '../services/tenantApi'

export default function Tenants() {
  const { data, isLoading, refetch } = useGetTenantsQuery()
  const [createTenant, { isLoading: isCreating }] = useCreateTenantMutation()
  const [name, setName] = React.useState('')

  const onCreate = async () => {
    if (!name) return
    await createTenant({ name }).unwrap()
    setName('')
    refetch()
  }

  if (isLoading) return <CircularProgress />

  return (
    <Container>
      <Typography variant="h4" sx={{ my: 2 }}>Tenants</Typography>
      <Stack direction="row" spacing={2} sx={{ mb: 2 }}>
        <TextField label="Tenant Name" value={name} onChange={(e) => setName(e.target.value)} />
        <Button variant="contained" onClick={onCreate} disabled={isCreating}>Create</Button>
      </Stack>
      <Box>
        {data?.map(t => (
          <Box key={t.id} sx={{ p: 1, borderBottom: '1px solid #eee' }}>
            <Typography>{t.name} {t.displayName ? `(${t.displayName})` : ''}</Typography>
          </Box>
        ))}
      </Box>
    </Container>
  )
}