import { Button, Menu, MenuItem, ListItemIcon, ListItemText } from '@mui/material'
import FileDownloadIcon from '@mui/icons-material/FileDownload'
import TableChartIcon from '@mui/icons-material/TableChart'
import DescriptionIcon from '@mui/icons-material/Description'
import { useState } from 'react'
import { useExport } from '../../hooks/useExport'
import { ExportColumn } from '../../utils/exportUtils'

interface ExportButtonProps<T extends Record<string, any>> {
  data: T[]
  columns: ExportColumn[]
  filename?: string
  label?: string
  variant?: 'text' | 'outlined' | 'contained'
  size?: 'small' | 'medium' | 'large'
}

/**
 * Export button component with CSV and Excel options
 */
export const ExportButton = <T extends Record<string, any>>({
  data,
  columns,
  filename = 'export',
  label = 'Dışa Aktar',
  variant = 'outlined',
  size = 'medium',
}: ExportButtonProps<T>) => {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
  const { exportToCSV, exportToExcel } = useExport()

  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget)
  }

  const handleClose = () => {
    setAnchorEl(null)
  }

  const handleExportCSV = () => {
    exportToCSV(data, columns, `${filename}.csv`)
    handleClose()
  }

  const handleExportExcel = () => {
    exportToExcel(data, columns, `${filename}.xls`)
    handleClose()
  }

  return (
    <>
      <Button
        variant={variant}
        size={size}
        startIcon={<FileDownloadIcon />}
        onClick={handleClick}
      >
        {label}
      </Button>
      <Menu anchorEl={anchorEl} open={Boolean(anchorEl)} onClose={handleClose}>
        <MenuItem onClick={handleExportCSV}>
          <ListItemIcon>
            <DescriptionIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>CSV olarak dışa aktar</ListItemText>
        </MenuItem>
        <MenuItem onClick={handleExportExcel}>
          <ListItemIcon>
            <TableChartIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>Excel olarak dışa aktar</ListItemText>
        </MenuItem>
      </Menu>
    </>
  )
}

export default ExportButton

