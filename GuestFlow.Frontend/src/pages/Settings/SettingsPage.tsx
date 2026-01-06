import { useState } from 'react'
import {
  Box,
  Typography,
  Paper,
  Tabs,
  Tab,
  Grid,
  Divider,
  Alert,
} from '@mui/material'
import {
  Settings as SettingsIcon,
  Email as EmailIcon,
  AttachMoney as CurrencyIcon,
  Security as SecurityIcon,
  Description as PdfIcon,
  Storage as StorageIcon,
} from '@mui/icons-material'
import ContentState from '../../components/Feedback/ContentState'

interface TabPanelProps {
  children?: React.ReactNode
  index: number
  value: number
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`settings-tabpanel-${index}`}
      aria-labelledby={`settings-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  )
}

const SettingsPage = () => {
  const [tabValue, setTabValue] = useState(0)
  // Settings functionality will be implemented later
  const isLoading = false
  const error = null


  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue)
  }


  if (isLoading) {
    return <ContentState state="loading" skeletonLines={6} />
  }

  if (error) {
    return (
      <ContentState
        state="error"
        title="Ayarlar yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
        actionLabel="Tekrar dene"
        onAction={() => {}}
      />
    )
  }

  return (
    <Box p={3}>
      <Typography variant="h4" sx={{ fontWeight: 600, mb: 3 }}>
        Ayarlar
      </Typography>

      <Paper>
        <Tabs value={tabValue} onChange={handleTabChange} variant="scrollable" scrollButtons="auto">
          <Tab icon={<SettingsIcon />} iconPosition="start" label="Genel" />
          <Tab icon={<EmailIcon />} iconPosition="start" label="E-posta" />
          <Tab icon={<CurrencyIcon />} iconPosition="start" label="Para Birimi" />
          <Tab icon={<PdfIcon />} iconPosition="start" label="PDF" />
          <Tab icon={<StorageIcon />} iconPosition="start" label="Dosya" />
          <Tab icon={<SecurityIcon />} iconPosition="start" label="Güvenlik" />
        </Tabs>

        <TabPanel value={tabValue} index={0}>
          <Typography variant="h6" gutterBottom>
            Genel Ayarlar
          </Typography>
          <Divider sx={{ mb: 3 }} />
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <Alert severity="info" sx={{ mb: 2 }}>
                Genel ayarlar yakında eklenecek.
              </Alert>
            </Grid>
          </Grid>
        </TabPanel>

        <TabPanel value={tabValue} index={1}>
          <Typography variant="h6" gutterBottom>
            E-posta Ayarları
          </Typography>
          <Divider sx={{ mb: 3 }} />
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <Alert severity="info" sx={{ mb: 2 }}>
                E-posta ayarları yakında eklenecek.
              </Alert>
            </Grid>
          </Grid>
        </TabPanel>

        <TabPanel value={tabValue} index={2}>
          <Typography variant="h6" gutterBottom>
            Para Birimi Ayarları
          </Typography>
          <Divider sx={{ mb: 3 }} />
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <Alert severity="info" sx={{ mb: 2 }}>
                Para birimi ayarları yakında eklenecek.
              </Alert>
            </Grid>
          </Grid>
        </TabPanel>

        <TabPanel value={tabValue} index={3}>
          <Typography variant="h6" gutterBottom>
            PDF Ayarları
          </Typography>
          <Divider sx={{ mb: 3 }} />
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <Alert severity="info" sx={{ mb: 2 }}>
                PDF ayarları yakında eklenecek.
              </Alert>
            </Grid>
          </Grid>
        </TabPanel>

        <TabPanel value={tabValue} index={4}>
          <Typography variant="h6" gutterBottom>
            Dosya Ayarları
          </Typography>
          <Divider sx={{ mb: 3 }} />
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <Alert severity="info" sx={{ mb: 2 }}>
                Dosya ayarları yakında eklenecek.
              </Alert>
            </Grid>
          </Grid>
        </TabPanel>

        <TabPanel value={tabValue} index={5}>
          <Typography variant="h6" gutterBottom>
            Güvenlik Ayarları
          </Typography>
          <Divider sx={{ mb: 3 }} />
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <Alert severity="info" sx={{ mb: 2 }}>
                Güvenlik ayarları yakında eklenecek.
              </Alert>
            </Grid>
          </Grid>
        </TabPanel>
      </Paper>
    </Box>
  )
}

export default SettingsPage
