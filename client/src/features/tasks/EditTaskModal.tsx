import { useEffect, useMemo, useState } from 'react'
import Modal from '../../components/Modal'
import Button from '../../components/Button'
import Input from '../../components/Input'
import Select from '../../components/Select'
import { api } from '../../lib/api'
import { useToast } from '../../components/useToast'
import type { Task, TaskPriority, TaskStatus, User } from '../../lib/types'

interface Props {
  open: boolean
  onClose: () => void
  task: Task
  onUpdated: (task: Task) => void
}

export default function EditTaskModal({ open, onClose, task, onUpdated }: Props) {
  const [title, setTitle] = useState(task.title)
  const [description, setDescription] = useState(task.description || '')
  const [priority, setPriority] = useState<TaskPriority>(task.priority || 'MEDIUM')
  const [status, setStatus] = useState<TaskStatus>(task.status)
  const [assigneeId, setAssigneeId] = useState<number | ''>(task.assigneeId ?? '')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [users, setUsers] = useState<User[]>([])
  const { toast } = useToast()

  useEffect(() => {
    if (!open) return
    api.get<User[]>('/api/users').then(r => setUsers(r.data)).catch(() => setUsers([]))
  }, [open])

  useEffect(() => {
    setTitle(task.title)
    setDescription(task.description || '')
    setPriority(task.priority || 'MEDIUM')
    setStatus(task.status)
    setAssigneeId(task.assigneeId ?? '')
  }, [task])

  const priorityOptions = useMemo(() => [
    { label: 'Low', value: 'LOW' },
    { label: 'Medium', value: 'MEDIUM' },
    { label: 'High', value: 'HIGH' },
  ], [])

  const statusOptions = useMemo(() => [
    { label: 'Todo', value: 'TODO' },
    { label: 'In Progress', value: 'IN_PROGRESS' },
    { label: 'Done', value: 'DONE' },
  ], [])

  const assigneeOptions = useMemo(() => [
    { label: 'Unassigned', value: '' },
    ...users.map(u => ({ label: u.username, value: u.id })),
  ], [users])

  const canSubmit = title.trim().length > 2 && !loading

  async function handleSave() {
    if (!canSubmit) return
    setLoading(true)
    setError(null)
    try {
      const payload = {
        title: title.trim(),
        description: description.trim() || undefined,
        priority,
        status,
        assigneeId: assigneeId === '' ? undefined : Number(assigneeId),
      }
  const res = await api.put<Task>(`/api/tasks/${task.id}`, payload)
      onUpdated(res.data)
  toast('Task updated', 'success')
      onClose()
    } catch (e: unknown) {
      type ErrResp = { response?: { data?: { message?: string } } }
      const message = (e as ErrResp)?.response?.data?.message
      setError(message || 'Failed to update task')
  toast(message || 'Failed to update task', 'error')
    } finally {
      setLoading(false)
    }
  }

  return (
    <Modal open={open} onClose={() => !loading && onClose()}>
      <div className="space-y-4">
        <h3 className="text-lg font-semibold">Edit Task</h3>
        {error && (
          <div className="rounded-md border border-red-200 bg-red-50 p-2 text-sm text-red-700">{error}</div>
        )}
        <Input label="Title" value={title} onChange={(e) => setTitle(e.target.value)} autoComplete="off" />
        <label className="block">
          <div className="mb-1 text-sm font-medium text-gray-700">Description</div>
          <textarea
            className="h-24 w-full rounded-md border border-gray-300 px-3 py-2 outline-none transition focus:ring-2 focus:ring-blue-500"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            autoComplete="off"
          />
        </label>
        <div className="grid grid-cols-3 gap-3">
          <Select label="Priority" value={priority} onChange={(e) => setPriority(e.target.value as TaskPriority)} options={priorityOptions} />
          <Select label="Status" value={status} onChange={(e) => setStatus(e.target.value as TaskStatus)} options={statusOptions} />
          <Select label="Assignee" value={assigneeId} onChange={(e) => setAssigneeId(e.target.value === '' ? '' : Number(e.target.value))} options={assigneeOptions} />
        </div>
        <div className="flex justify-end gap-2 pt-2">
          <Button variant="secondary" onClick={onClose} disabled={loading}>Cancel</Button>
          <Button onClick={handleSave} disabled={!canSubmit || loading}>Save</Button>
        </div>
      </div>
    </Modal>
  )
}
