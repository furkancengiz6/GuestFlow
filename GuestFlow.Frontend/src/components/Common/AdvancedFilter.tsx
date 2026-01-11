import { useState, useEffect } from 'react'
import {
  Box,
  Paper,
  Typography,
  TextField,
  Button,
  Chip,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Switch,
  FormControlLabel,
  IconButton,
} from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import {
  ExpandMore as ExpandMoreIcon,
  FilterList as FilterIcon,
  Clear as ClearIcon,
  Search as SearchIcon,
} from '@mui/icons-material'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns'
import { tr } from 'date-fns/locale'

export interface FilterField {
  key: string
  label: string
  type: 'text' | 'number' | 'date' | 'select' | 'boolean' | 'range'
  options?: Array<{ value: string | number; label: string }>
  placeholder?: string
}

export interface AdvancedFilterProps {
  fields: FilterField[]
  onFilterChange: (filters: Record<string, any>) => void
  initialFilters?: Record<string, any>
  showSearch?: boolean
  searchPlaceholder?: string
}

/**
 * Advanced filtering component with multiple filter types
 */
export const AdvancedFilter = ({
  fields,
  onFilterChange,
  initialFilters = {},
  showSearch = true,
  searchPlaceholder = 'Ara...',
}: AdvancedFilterProps) => {
  const [filters, setFilters] = useState<Record<string, any>>(initialFilters)
  const [searchQuery, setSearchQuery] = useState('')
  const [expanded, setExpanded] = useState(false)

  useEffect(() => {
    onFilterChange({ ...filters, search: searchQuery })
  }, [filters, searchQuery, onFilterChange])

  const handleFilterChange = (key: string, value: any) => {
    setFilters((prev) => ({
      ...prev,
      [key]: value || undefined,
    }))
  }

  const handleClearFilters = () => {
    setFilters({})
    setSearchQuery('')
    onFilterChange({})
  }

  const activeFilterCount = Object.keys(filters).filter((key) => filters[key] !== undefined && filters[key] !== '').length

  const renderFilterField = (field: FilterField) => {
    switch (field.type) {
      case 'text':
        return (
          <TextField
            fullWidth
            label={field.label}
            value={filters[field.key] || ''}
            onChange={(e) => handleFilterChange(field.key, e.target.value)}
            placeholder={field.placeholder}
            size="small"
          />
        )

      case 'number':
        return (
          <TextField
            fullWidth
            type="number"
            label={field.label}
            value={filters[field.key] || ''}
            onChange={(e) => handleFilterChange(field.key, e.target.value ? Number(e.target.value) : undefined)}
            placeholder={field.placeholder}
            size="small"
          />
        )

      case 'date':
        return (
          <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={tr}>
            <DatePicker
              label={field.label}
              value={filters[field.key] || null}
              onChange={(date: Date | null) => handleFilterChange(field.key, date)}
              slotProps={{ textField: { size: 'small', fullWidth: true } }}
            />
          </LocalizationProvider>
        )

      case 'select':
        return (
          <FormControl fullWidth size="small">
            <InputLabel>{field.label}</InputLabel>
            <Select
              value={filters[field.key] || ''}
              label={field.label}
              onChange={(e) => handleFilterChange(field.key, e.target.value)}
            >
              <MenuItem value="">
                <em>Tümü</em>
              </MenuItem>
              {field.options?.map((option) => (
                <MenuItem key={option.value} value={option.value}>
                  {option.label}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        )

      case 'boolean':
        return (
          <FormControlLabel
            control={
              <Switch
                checked={filters[field.key] || false}
                onChange={(e) => handleFilterChange(field.key, e.target.checked)}
              />
            }
            label={field.label}
          />
        )

      case 'range':
        return (
          <Grid container spacing={1}>
            <Grid item xs={6}>
              <TextField
                fullWidth
                type="number"
                label={`${field.label} (Min)`}
                value={filters[`${field.key}_min`] || ''}
                onChange={(e) => handleFilterChange(`${field.key}_min`, e.target.value ? Number(e.target.value) : undefined)}
                size="small"
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                type="number"
                label={`${field.label} (Max)`}
                value={filters[`${field.key}_max`] || ''}
                onChange={(e) => handleFilterChange(`${field.key}_max`, e.target.value ? Number(e.target.value) : undefined)}
                size="small"
              />
            </Grid>
          </Grid>
        )

      default:
        return null
    }
  }

  return (
    <Paper sx={{ p: 2, mb: 2 }}>
      {showSearch && (
        <Box sx={{ mb: 2 }}>
          <TextField
            fullWidth
            placeholder={searchPlaceholder}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            InputProps={{
              startAdornment: <SearchIcon sx={{ mr: 1, color: 'text.secondary' }} />,
              endAdornment: searchQuery && (
                <IconButton size="small" onClick={() => setSearchQuery('')}>
                  <ClearIcon fontSize="small" />
                </IconButton>
              ),
            }}
            size="small"
            data-search
          />
        </Box>
      )}

      <Accordion expanded={expanded} onChange={(_, isExpanded) => setExpanded(isExpanded)}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, width: '100%' }}>
            <FilterIcon />
            <Typography>Gelişmiş Filtreler</Typography>
            {activeFilterCount > 0 && (
              <Chip label={activeFilterCount} size="small" color="primary" />
            )}
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={2}>
            {fields.map((field) => (
              <Grid item xs={12} sm={6} md={4} key={field.key}>
                {renderFilterField(field)}
              </Grid>
            ))}
          </Grid>
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1, mt: 2 }}>
            <Button
              variant="outlined"
              startIcon={<ClearIcon />}
              onClick={handleClearFilters}
              disabled={activeFilterCount === 0 && !searchQuery}
            >
              Filtreleri Temizle
            </Button>
          </Box>
        </AccordionDetails>
      </Accordion>

      {activeFilterCount > 0 && (
        <Box sx={{ display: 'flex', gap: 1, mt: 2, flexWrap: 'wrap' }}>
          {Object.entries(filters).map(([key, value]) => {
            if (value === undefined || value === '') return null
            const field = fields.find((f) => f.key === key)
            const label = field?.label || key
            return (
              <Chip
                key={key}
                label={`${label}: ${value}`}
                onDelete={() => handleFilterChange(key, undefined)}
                size="small"
              />
            )
          })}
        </Box>
      )}
    </Paper>
  )
}

export default AdvancedFilter

