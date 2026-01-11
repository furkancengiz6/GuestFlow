import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  List,
  ListItem,
  ListItemText,
  Box,
  Typography,
  Chip,
  Divider,
} from '@mui/material'
import { Keyboard as KeyboardIcon } from '@mui/icons-material'
import { commonShortcuts } from '../../hooks/useKeyboardShortcuts'

interface KeyboardShortcutsDialogProps {
  open: boolean
  onClose: () => void
  customShortcuts?: Array<{
    key: string
    ctrl?: boolean
    shift?: boolean
    alt?: boolean
    description: string
  }>
}

export const KeyboardShortcutsDialog = ({
  open,
  onClose,
  customShortcuts = [],
}: KeyboardShortcutsDialogProps) => {
  const allShortcuts = [...commonShortcuts, ...customShortcuts]

  const formatKey = (shortcut: any) => {
    const parts: string[] = []
    if (shortcut.ctrl) parts.push('Ctrl')
    if (shortcut.shift) parts.push('Shift')
    if (shortcut.alt) parts.push('Alt')
    if (shortcut.meta) parts.push('Meta')
    if (shortcut.key) parts.push(String(shortcut.key).toUpperCase())
    return parts.join(' + ')
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <KeyboardIcon />
          <Typography variant="h6">Klavye Kısayolları</Typography>
        </Box>
      </DialogTitle>
      <DialogContent>
        <List>
          {allShortcuts.map((shortcut: any, index: number) => (
            <div key={index}>
              <ListItem>
                <ListItemText
                  primary={shortcut.description || 'Kısayol'}
                  secondary={
                    <Box sx={{ mt: 1, display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
                      {formatKey(shortcut)
                        .split(' + ')
                        .map((key, i) => (
                          <Chip key={i} label={key} size="small" variant="outlined" />
                        ))}
                    </Box>
                  }
                  secondaryTypographyProps={{ component: 'div' }}
                />
              </ListItem>
              {index < allShortcuts.length - 1 && <Divider />}
            </div>
          ))}
        </List>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Kapat</Button>
      </DialogActions>
    </Dialog>
  )
}

export default KeyboardShortcutsDialog

