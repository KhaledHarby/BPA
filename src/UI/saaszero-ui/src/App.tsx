import { AppBar, Box, Button, Container, Toolbar, Typography } from '@mui/material'
import { Link, Outlet } from 'react-router-dom'

export default function App() {
  return (
    <Box>
      <AppBar position="static">
        <Toolbar>
          <Typography variant="h6" sx={{ flexGrow: 1 }}>SaaSZero</Typography>
          <Button component={Link} to="/tenants" color="inherit">Tenants</Button>
          <Button component={Link} to="/login" color="inherit">Login</Button>
        </Toolbar>
      </AppBar>
      <Container sx={{ my: 2 }}>
        <Outlet />
      </Container>
    </Box>
  )
}
