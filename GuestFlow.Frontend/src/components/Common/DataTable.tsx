import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  Paper,
  Box,
  Checkbox,
  IconButton,
  Tooltip,
  Typography,
} from '@mui/material'
import { useState, ReactNode } from 'react'

export interface Column<T> {
  id: string
  label: string
  minWidth?: number
  align?: 'right' | 'left' | 'center'
  format?: (value: any, row: T) => ReactNode
  sortable?: boolean
}

interface DataTableProps<T> {
  columns: Column<T>[]
  data: T[]
  loading?: boolean
  page?: number
  rowsPerPage?: number
  totalCount?: number
  onPageChange?: (page: number) => void
  onRowsPerPageChange?: (rowsPerPage: number) => void
  onRowClick?: (row: T) => void
  actions?: (row: T) => ReactNode
  selectable?: boolean
  selectedRows?: T[]
  onSelectionChange?: (selected: T[]) => void
  getRowId?: (row: T) => string | number
  emptyMessage?: string
}

/**
 * Generic, reusable data table component
 */
export function DataTable<T extends Record<string, any>>({
  columns,
  data,
  loading = false,
  page = 0,
  rowsPerPage = 10,
  totalCount,
  onPageChange,
  onRowsPerPageChange,
  onRowClick,
  actions,
  selectable = false,
  selectedRows = [],
  onSelectionChange,
  getRowId = (row: T) => row.id,
  emptyMessage = 'Veri bulunamadı',
}: DataTableProps<T>) {
  const [selected, setSelected] = useState<(string | number)[]>(
    selectedRows.map(getRowId)
  )

  const handleSelectAllClick = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.checked) {
      const newSelected = data.map(getRowId)
      setSelected(newSelected)
      onSelectionChange?.(data)
      return
    }
    setSelected([])
    onSelectionChange?.([])
  }

  const handleSelectClick = (event: React.MouseEvent<unknown>, row: T) => {
    event.stopPropagation()
    const rowId = getRowId(row)
    const selectedIndex = selected.indexOf(rowId)
    let newSelected: (string | number)[] = []

    if (selectedIndex === -1) {
      newSelected = newSelected.concat(selected, rowId)
    } else if (selectedIndex === 0) {
      newSelected = newSelected.concat(selected.slice(1))
    } else if (selectedIndex === selected.length - 1) {
      newSelected = newSelected.concat(selected.slice(0, -1))
    } else if (selectedIndex > 0) {
      newSelected = newSelected.concat(
        selected.slice(0, selectedIndex),
        selected.slice(selectedIndex + 1)
      )
    }

    setSelected(newSelected)
    const selectedRows = data.filter((row) => newSelected.includes(getRowId(row)))
    onSelectionChange?.(selectedRows)
  }

  const isSelected = (row: T) => selected.indexOf(getRowId(row)) !== -1

  const handleChangePage = (_event: unknown, newPage: number) => {
    onPageChange?.(newPage)
  }

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    onRowsPerPageChange?.(parseInt(event.target.value, 10))
    onPageChange?.(0)
  }

  return (
    <Paper>
      <TableContainer>
        <Table stickyHeader>
          <TableHead>
            <TableRow>
              {selectable && (
                <TableCell padding="checkbox">
                  <Checkbox
                    indeterminate={selected.length > 0 && selected.length < data.length}
                    checked={data.length > 0 && selected.length === data.length}
                    onChange={handleSelectAllClick}
                    inputProps={{
                      'aria-label': 'Tümünü seç',
                    }}
                  />
                </TableCell>
              )}
              {columns.map((column) => (
                <TableCell
                  key={column.id}
                  align={column.align}
                  style={{ minWidth: column.minWidth }}
                >
                  {column.label}
                </TableCell>
              ))}
              {actions && <TableCell align="right">İşlemler</TableCell>}
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={columns.length + (selectable ? 1 : 0) + (actions ? 1 : 0)}>
                  <Box sx={{ py: 4, textAlign: 'center' }}>
                    <Typography color="text.secondary">Yükleniyor...</Typography>
                  </Box>
                </TableCell>
              </TableRow>
            ) : data.length === 0 ? (
              <TableRow>
                <TableCell colSpan={columns.length + (selectable ? 1 : 0) + (actions ? 1 : 0)}>
                  <Box sx={{ py: 4, textAlign: 'center' }}>
                    <Typography color="text.secondary">{emptyMessage}</Typography>
                  </Box>
                </TableCell>
              </TableRow>
            ) : (
              data.map((row) => {
                const isItemSelected = isSelected(row)
                return (
                  <TableRow
                    hover
                    key={getRowId(row)}
                    selected={isItemSelected}
                    onClick={() => onRowClick?.(row)}
                    sx={{ cursor: onRowClick ? 'pointer' : 'default' }}
                  >
                    {selectable && (
                      <TableCell padding="checkbox">
                        <Checkbox
                          checked={isItemSelected}
                          onClick={(e) => handleSelectClick(e, row)}
                          inputProps={{
                            'aria-labelledby': `table-checkbox-${getRowId(row)}`,
                          }}
                        />
                      </TableCell>
                    )}
                    {columns.map((column) => {
                      const value = row[column.id]
                      return (
                        <TableCell key={column.id} align={column.align}>
                          {column.format ? column.format(value, row) : value}
                        </TableCell>
                      )
                    })}
                    {actions && (
                      <TableCell align="right" onClick={(e) => e.stopPropagation()}>
                        {actions(row)}
                      </TableCell>
                    )}
                  </TableRow>
                )
              })
            )}
          </TableBody>
        </Table>
      </TableContainer>
      {totalCount !== undefined && (
        <TablePagination
          component="div"
          count={totalCount}
          page={page}
          onPageChange={handleChangePage}
          rowsPerPage={rowsPerPage}
          onRowsPerPageChange={handleChangeRowsPerPage}
          rowsPerPageOptions={[5, 10, 25, 50, 100]}
          labelRowsPerPage="Sayfa başına satır:"
          labelDisplayedRows={({ from, to, count }) =>
            `${from}-${to} / ${count !== -1 ? count : `~${to}`}`
          }
        />
      )}
    </Paper>
  )
}

export default DataTable

