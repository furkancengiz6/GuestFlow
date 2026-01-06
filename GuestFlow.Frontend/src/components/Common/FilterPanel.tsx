import {
  Box,
  Paper,
  Grid,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Button,
  InputAdornment,
  IconButton,
  Collapse,
} from '@mui/material'
import SearchIcon from '@mui/icons-material/Search'
import ClearIcon from '@mui/icons-material/Clear'
import FilterListIcon from '@mui/icons-material/FilterList'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import { useState, ReactNode } from 'react'

export interface FilterOption {
  label: string
  value: string | number
}

export interface FilterField {
  id: string
  label: string
  type: 'text' | 'select' | 'date' | 'number'
  options?: FilterOption[]
  placeholder?: string
}

interface FilterPanelProps {
  fields: FilterField[]
  values: Record<string, any>
  onChange: (field: string, value: any) => void
  onClear?: () => void
  searchPlaceholder?: string
  showSearch?: boolean
  collapsible?: boolean
  defaultCollapsed?: boolean
  actions?: ReactNode
}

/**
 * Reusable filter panel component
 */
export const FilterPanel = ({
  fields,
  values,
  onChange,
  onClear,
  searchPlaceholder = 'Ara...',
  showSearch = true,
  collapsible = true,
  defaultCollapsed = true,
  actions,
}: FilterPanelProps) => {
  const [collapsed, setCollapsed] = useState(defaultCollapsed)

  const handleSearchChange = (value: string) => {
    onChange('searchTerm', value)
  }

  const searchValue = values.searchTerm || ''

  return (
    <Paper sx={{ p: 2, mb: 2 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
        <Button
          variant="outlined"
          startIcon={<FilterListIcon />}
          endIcon={collapsed ? <ExpandMoreIcon /> : <ExpandLessIcon />}
          onClick={() => collapsible && setCollapsed(!collapsed)}
          sx={{ mr: 1 }}
        >
          Filtreler
        </Button>
        {actions}
      </Box>

      <Collapse in={!collapsed}>
        <Grid container spacing={2} alignItems="center">
          {showSearch && (
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                size="small"
                label={searchPlaceholder}
                value={searchValue}
                onChange={(e) => handleSearchChange(e.target.value)}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <SearchIcon />
                    </InputAdornment>
                  ),
                  endAdornment: searchValue && (
                    <InputAdornment position="end">
                      <IconButton
                        size="small"
                        onClick={() => handleSearchChange('')}
                        edge="end"
                      >
                        <ClearIcon />
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
          )}

          {fields.map((field) => (
            <Grid item xs={12} md={field.type === 'text' ? 4 : 3} key={field.id}>
              {field.type === 'select' ? (
                <FormControl fullWidth size="small">
                  <InputLabel>{field.label}</InputLabel>
                  <Select
                    value={values[field.id] || ''}
                    label={field.label}
                    onChange={(e) => onChange(field.id, e.target.value)}
                  >
                    <MenuItem value="">Tümü</MenuItem>
                    {field.options?.map((option) => (
                      <MenuItem key={option.value} value={option.value}>
                        {option.label}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              ) : (
                <TextField
                  fullWidth
                  size="small"
                  label={field.label}
                  type={field.type}
                  value={values[field.id] || ''}
                  onChange={(e) => onChange(field.id, e.target.value)}
                  placeholder={field.placeholder}
                  InputLabelProps={
                    field.type === 'date' ? { shrink: true } : undefined
                  }
                />
              )}
            </Grid>
          ))}

          {onClear && (
            <Grid item xs={12} md="auto">
              <Button
                variant="outlined"
                size="small"
                onClick={onClear}
                startIcon={<ClearIcon />}
              >
                Temizle
              </Button>
            </Grid>
          )}
        </Grid>
      </Collapse>
    </Paper>
  )
}

export default FilterPanel

