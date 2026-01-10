import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { ExportButton } from '../../../components/Common/ExportButton'

const exportToCSV = jest.fn()
const exportToExcel = jest.fn()

jest.mock('../../../hooks/useExport', () => ({
  __esModule: true,
  useExport: () => ({
    exportToCSV,
    exportToExcel,
  }),
}))

const mockData = [
  { id: 1, name: 'Test 1', value: 100 },
  { id: 2, name: 'Test 2', value: 200 },
]

const mockColumns = [
  { header: 'ID', key: 'id' },
  { header: 'Name', key: 'name' },
  { header: 'Value', key: 'value' },
]

describe('ExportButton', () => {
  beforeEach(() => {
    jest.clearAllMocks()
  })

  it('should render export button', () => {
    render(<ExportButton data={mockData} columns={mockColumns} />)
    expect(screen.getByText('Dışa Aktar')).toBeInTheDocument()
  })

  it('should open menu on click', async () => {
    const user = userEvent.setup()
    render(<ExportButton data={mockData} columns={mockColumns} />)

    const button = screen.getByText('Dışa Aktar')
    await user.click(button)

    await waitFor(() => {
      expect(screen.getByText('CSV olarak dışa aktar')).toBeInTheDocument()
      expect(screen.getByText('Excel olarak dışa aktar')).toBeInTheDocument()
    })
  })

  it('calls exportToCSV when CSV option is clicked', async () => {
    const user = userEvent.setup()

    render(<ExportButton data={mockData} columns={mockColumns} />)

    const button = screen.getByText('Dışa Aktar')
    await user.click(button)

    const csvOption = screen.getByText('CSV olarak dışa aktar')
    await user.click(csvOption)

    expect(exportToCSV).toHaveBeenCalledTimes(1)
    expect(exportToCSV).toHaveBeenCalledWith(mockData, mockColumns, 'export.csv')
    // menu should close after click
    expect(screen.queryByText('CSV olarak dışa aktar')).not.toBeInTheDocument()
  })

  it('calls exportToExcel when Excel option is clicked', async () => {
    const user = userEvent.setup()

    render(<ExportButton data={mockData} columns={mockColumns} filename="my-file" />)

    await user.click(screen.getByText('Dışa Aktar'))
    await user.click(screen.getByText('Excel olarak dışa aktar'))

    expect(exportToExcel).toHaveBeenCalledTimes(1)
    expect(exportToExcel).toHaveBeenCalledWith(mockData, mockColumns, 'my-file.xls')
  })

  it('should use custom label when provided', () => {
    render(<ExportButton data={mockData} columns={mockColumns} label="Export Data" />)
    expect(screen.getByText('Export Data')).toBeInTheDocument()
  })
})

