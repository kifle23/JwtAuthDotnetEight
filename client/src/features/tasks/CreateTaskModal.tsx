import { useEffect, useMemo, useState } from 'react'
import Modal from '../../components/Modal'
import Button from '../../components/Button'
import Input from '../../components/Input'
import Select from '../../components/Select'
import { api } from '../../lib/api'
import type { AxiosResponse } from 'axios'
import type { Task, TaskPriority, User } from '../../lib/types'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { useToast } from '../../components/useToast'

interface Props {
  open: boolean
  onClose: () => void
  onCreated: (task: Task) => void
}

export default function CreateTaskModal({ open, onClose, onCreated }: Props) {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [users, setUsers] = useState<User[]>([])
  const { toast } = useToast()

  const schema = z.object({
    title: z.string().min(3, 'Title must be at least 3 characters'),
    description: z.string().max(1000).optional().or(z.literal('')),
    priority: z.enum(['LOW', 'MEDIUM', 'HIGH']),
    assigneeId: z.string().refine(v => v === '' || /^\d+$/.test(v), {
      message: 'Assignee must be a number',
    }),
  })

  type FormValues = z.infer<typeof schema>
  const { register, handleSubmit, formState: { errors, isSubmitting }, reset } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { title: '', description: '', priority: 'MEDIUM', assigneeId: '' },
  })

  useEffect(() => {
    if (!open) return
    api
      .get<User[]>('/api/users')
      .then((r: AxiosResponse<User[]>) => setUsers(r.data))
      .catch(() => setUsers([]))
  }, [open])

  const canSubmit = !loading && !isSubmitting

  const priorityOptions = useMemo(
    () => [
      { label: 'Low', value: 'LOW' },
      { label: 'Medium', value: 'MEDIUM' },
      { label: 'High', value: 'HIGH' },
    ],
    []
  )

  const assigneeOptions = useMemo(() => {
    const base: Array<{ label: string; value: string | number }> = [
      { label: 'Unassigned', value: '' },
    ]
    const more: Array<{ label: string; value: string | number }> = users.map((u) => ({
      label: u.username,
      value: u.id,
    }))
    return [...base, ...more]
  }, [users])

  const resetForm = () => { reset(); setError(null) }

  const handleCreate = async (values: FormValues) => {
    if (!canSubmit) return
    setLoading(true)
    setError(null)
    try {
      const payload = {
        title: values.title.trim(),
        description: values.description?.trim() || undefined,
        priority: values.priority as TaskPriority,
        assigneeId: values.assigneeId === '' ? undefined : Number(values.assigneeId),
      }
  const res = await api.post<Task>('/api/tasks', payload)
      onCreated(res.data)
  toast('Task created', 'success')
      resetForm()
      onClose()
    } catch (e: unknown) {
      type ErrResp = { response?: { data?: { message?: string } } }
      const message = (e as ErrResp)?.response?.data?.message
      setError(message || 'Failed to create task')
  toast(message || 'Failed to create task', 'error')
    } finally {
      setLoading(false)
    }
  }

  return (
    <Modal open={open} onClose={() => !loading && onClose()}>
      <div className="space-y-4">
        <h3 className="text-lg font-semibold">Create Task</h3>
        {error && (
          <div className="rounded-md border border-red-200 bg-red-50 p-2 text-sm text-red-700">{error}</div>
        )}
        <form onSubmit={handleSubmit(handleCreate)} className="space-y-4" autoComplete="off" autoCapitalize="off" autoCorrect="off">
          <Input label="Title" placeholder="Task title" error={errors.title?.message} {...register('title')} autoComplete="off" />
        <label className="block">
          <div className="mb-1 text-sm font-medium text-gray-700">Description</div>
          <textarea
            className="h-24 w-full rounded-md border border-gray-300 px-3 py-2 outline-none transition focus:ring-2 focus:ring-blue-500"
            placeholder="Task details"
            {...register('description')}
            autoComplete="off"
          />
        </label>
        <div className="grid grid-cols-2 gap-3">
          <Select label="Priority" error={errors.priority?.message} {...register('priority')} options={priorityOptions} />
          <Select label="Assignee" error={errors.assigneeId?.message} {...register('assigneeId')} options={assigneeOptions} />
        </div>
        <div className="flex justify-end gap-2 pt-2">
          <Button type="button" variant="secondary" onClick={onClose} disabled={loading}>
            Cancel
          </Button>
          <Button type="submit" disabled={!canSubmit || loading}>{isSubmitting ? 'Creating...' : 'Create'}</Button>
        </div>
        </form>
      </div>
    </Modal>
  )
}
