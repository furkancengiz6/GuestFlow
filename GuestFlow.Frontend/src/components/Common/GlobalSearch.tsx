import { useState, useEffect, useRef } from 'react'
import {
  Dialog,
  DialogContent,
  TextField,
  List,
  ListItem,
  ListItemButton,
  ListItemText,
  ListItemIcon,
  Box,
  Typography,
  Chip,
  InputAdornment,
  Paper,
} from '@mui/material'
import {
  Search as SearchIcon,
  Person as PersonIcon,
  DirectionsCar as TransferIcon,
  Tour as TourIcon,
  Receipt as InvoiceIcon,
  Hotel as HotelIcon,
  Restaurant as RestaurantIcon,
} from '@mui/icons-material'
import { useNavigate } from 'react-router-dom'

interface SearchResult {
  id: string
  type: 'guest' | 'transfer' | 'tour' | 'invoice' | 'hotel' | 'restaurant'
  title: string
  subtitle?: string
  url: string
}

interface GlobalSearchProps {
  open: boolean
  onClose: () => void
}

export const GlobalSearch = ({ open, onClose }: GlobalSearchProps) => {
  const [query, setQuery] = useState('')
  const [results, setResults] = useState<SearchResult[]>([])
  const [loading, setLoading] = useState(false)
  const inputRef = useRef<HTMLInputElement>(null)
  const navigate = useNavigate()

  useEffect(() => {
    if (open && inputRef.current) {
      inputRef.current.focus()
    }
  }, [open])

  useEffect(() => {
    if (!query.trim()) {
      setResults([])
      return
    }

    setLoading(true)
    // Simulate search - replace with actual API call
    const searchResults: SearchResult[] = [
      // Mock results - replace with actual search API
      {
        id: '1',
        type: 'guest',
        title: 'John Doe',
        subtitle: 'john@example.com',
        url: '/guests/1',
      },
    ]

    setTimeout(() => {
      setResults(searchResults)
      setLoading(false)
    }, 300)
  }, [query])

  const handleResultClick = (result: SearchResult) => {
    navigate(result.url)
    onClose()
    setQuery('')
  }

  const getIcon = (type: SearchResult['type']) => {
    switch (type) {
      case 'guest':
        return <PersonIcon />
      case 'transfer':
        return <TransferIcon />
      case 'tour':
        return <TourIcon />
      case 'invoice':
        return <InvoiceIcon />
      case 'hotel':
        return <HotelIcon />
      case 'restaurant':
        return <RestaurantIcon />
      default:
        return <SearchIcon />
    }
  }

  const getTypeLabel = (type: SearchResult['type']) => {
    switch (type) {
      case 'guest':
        return 'Misafir'
      case 'transfer':
        return 'Transfer'
      case 'tour':
        return 'Tur'
      case 'invoice':
        return 'Fatura'
      case 'hotel':
        return 'Otel'
      case 'restaurant':
        return 'Restoran'
      default:
        return ''
    }
  }

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="sm"
      fullWidth
      PaperProps={{
        sx: {
          maxHeight: '80vh',
        },
      }}
    >
      <DialogContent sx={{ p: 0 }}>
        <Box sx={{ p: 2, borderBottom: 1, borderColor: 'divider' }}>
          <TextField
            inputRef={inputRef}
            fullWidth
            placeholder="Ara... (Ctrl+K)"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon />
                </InputAdornment>
              ),
            }}
            data-global-search
            autoFocus
          />
        </Box>

        {loading && (
          <Box sx={{ p: 2, textAlign: 'center' }}>
            <Typography variant="body2" color="text.secondary">
              Aranıyor...
            </Typography>
          </Box>
        )}

        {!loading && query && results.length === 0 && (
          <Box sx={{ p: 2, textAlign: 'center' }}>
            <Typography variant="body2" color="text.secondary">
              Sonuç bulunamadı
            </Typography>
          </Box>
        )}

        {!loading && results.length > 0 && (
          <List sx={{ maxHeight: '60vh', overflow: 'auto' }}>
            {results.map((result) => (
              <ListItem key={result.id} disablePadding>
                <ListItemButton onClick={() => handleResultClick(result)}>
                  <ListItemIcon>{getIcon(result.type)}</ListItemIcon>
                  <ListItemText
                    primary={result.title}
                    secondary={
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 0.5 }}>
                        {result.subtitle && (
                          <Typography variant="caption" color="text.secondary">
                            {result.subtitle}
                          </Typography>
                        )}
                        <Chip label={getTypeLabel(result.type)} size="small" variant="outlined" />
                      </Box>
                    }
                  />
                </ListItemButton>
              </ListItem>
            ))}
          </List>
        )}

        {!query && (
          <Box sx={{ p: 2 }}>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              Hızlı kısayollar:
            </Typography>
            <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mt: 1 }}>
              <Chip label="Ctrl+K" size="small" variant="outlined" />
              <Chip label="Ctrl+N" size="small" variant="outlined" />
              <Chip label="Ctrl+S" size="small" variant="outlined" />
              <Chip label="Esc" size="small" variant="outlined" />
            </Box>
          </Box>
        )}
      </DialogContent>
    </Dialog>
  )
}

export default GlobalSearch

