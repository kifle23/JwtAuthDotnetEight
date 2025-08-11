import Modal from './Modal'
import Button from './Button'

type Props = {
  open: boolean
  title?: string
  message?: string
  confirmText?: string
  cancelText?: string
  onCancel: () => void
  onConfirm: () => void
  busy?: boolean
}

export default function ConfirmDialog({ open, title = 'Are you sure?', message, confirmText = 'Confirm', cancelText = 'Cancel', onCancel, onConfirm, busy = false }: Props) {
  return (
    <Modal open={open} onClose={onCancel} title={title}>
      {message && <p className="text-sm text-gray-700">{message}</p>}
      <div className="mt-4 flex justify-end gap-2">
        <Button variant="secondary" onClick={onCancel} disabled={busy}> {cancelText} </Button>
        <Button className="bg-red-600 text-white hover:bg-red-700" onClick={onConfirm} loading={busy}> {confirmText} </Button>
      </div>
    </Modal>
  )
}
