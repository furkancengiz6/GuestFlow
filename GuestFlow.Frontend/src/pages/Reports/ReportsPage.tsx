import { Box, Typography } from '@mui/material'

const ReportsPage = () => {
  return (
    <Box p={3}>
      <Typography variant="h5" gutterBottom>
        Raporlar
      </Typography>
      <Typography color="text.secondary">Bu sayfa yalnızca Admin rolü içindir.</Typography>
    </Box>
  )
}

export default ReportsPage

