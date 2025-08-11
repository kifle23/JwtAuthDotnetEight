import { useEffect, useMemo, useState, Suspense, lazy } from 'react'
import { api } from '../lib/api'
import * as signalR from '@microsoft/signalr'
import { DragDropContext, Droppable, Draggable, type DropResult } from '@hello-pangea/dnd'
import type { Task } from '../lib/types'
import Badge from '../components/Badge'
import Button from '../components/Button'
import Select from '../components/Select'
import ConfirmDialog from '../components/ConfirmDialog'
import { useToast } from '../components/useToast'

const CreateTaskModal = lazy(() => import('../features/tasks/CreateTaskModal'))
const EditTaskModal = lazy(() => import('../features/tasks/EditTaskModal'))

const columns: Array<{ key: Task['status']; title: string }> = [
  { key: 'TODO', title: 'Todo' },
  { key: 'IN_PROGRESS', title: 'In Progress' },
  { key: 'DONE', title: 'Done' },
]

export default function Board() {
  const { toast } = useToast()
  const [tasks, setTasks] = useState<Task[]>([])
  const [openCreate, setOpenCreate] = useState(false)
  const [editing, setEditing] = useState<null | Task>(null)
  const [confirm, setConfirm] = useState<{ open: boolean; id?: number }>({ open: false })
  const [statusFilter, setStatusFilter] = useState<'ALL' | Task['status']>('ALL')
  const [assigneeFilter, setAssigneeFilter] = useState<number | ''>('')
  const [search, setSearch] = useState('')
  const [users, setUsers] = useState<Array<{ id: number; username: string }>>([])
  const grouped = useMemo(() => ({
    TODO: tasks.filter(t => t.status === 'TODO'),
    IN_PROGRESS: tasks.filter(t => t.status === 'IN_PROGRESS'),
    DONE: tasks.filter(t => t.status === 'DONE'),
  }), [tasks])

  useEffect(() => {
    api.get('/api/users').then(r => setUsers(r.data)).catch(() => setUsers([]))
  }, [])

  useEffect(() => {
    const controller = new AbortController()
    const timer = setTimeout(() => {
  const params: Record<string, string | number> = {}
  if (statusFilter !== 'ALL') params.status = statusFilter
  if (assigneeFilter !== '') params.assignee = assigneeFilter
  if (search.trim()) params.search = search.trim()
      api.get('/api/tasks', { params, signal: controller.signal }).then(r => setTasks(r.data)).catch(() => {})
    }, 250)
    return () => { clearTimeout(timer); controller.abort() }
  }, [statusFilter, assigneeFilter, search])

  const connection = useMemo(() => {
    const hubUrl = import.meta.env?.VITE_SIGNALR_HUB ?? '/hub/tasks'
    const base = import.meta.env?.VITE_API_BASE ?? ''
    return new signalR.HubConnectionBuilder()
      .withUrl(base + hubUrl, {
        accessTokenFactory: () => localStorage.getItem('token') || '',
        transport: signalR.HttpTransportType.WebSockets,
        skipNegotiation: true,
      })
      .configureLogging(signalR.LogLevel.Warning)
      .withAutomaticReconnect()
      .build()
  }, [])

  useEffect(() => {
    let disposed = false
    
    const onCreated = (task: Task) => setTasks(prev => {
      const exists = prev.some(t => t.id === task.id)
      return exists ? prev.map(t => t.id === task.id ? task : t) : [...prev, task]
    })
    const onUpdated = (task: Task) => setTasks(prev => prev.map(t => t.id === task.id ? task : t))
    const onDeleted = (data: unknown) => {
      const id = typeof data === 'number' ? data : (typeof data === 'object' && data !== null ? (data as { id?: number }).id : undefined)
      if (typeof id === 'number') {
        setTasks(prev => prev.filter(t => t.id !== id))
      }
    }

    connection.on('task:created', onCreated)
    connection.on('task:updated', onUpdated)
    connection.on('task:deleted', onDeleted)

    const startTimer = window.setTimeout(() => {
      connection.start().catch((err: unknown) => {
        if (!disposed && !(err instanceof Error && /before stop\(\) was called/i.test(err.message))) {
          console.warn('SignalR start error:', err)
        }
      })
    }, 0)

    return () => {
      disposed = true
      if (startTimer) { clearTimeout(startTimer) }
      connection.off('task:created', onCreated)
      connection.off('task:updated', onUpdated)
      connection.off('task:deleted', onDeleted)
      const state = connection.state
      if (state === signalR.HubConnectionState.Connecting || state === signalR.HubConnectionState.Connected || state === signalR.HubConnectionState.Reconnecting) {
        connection.stop().catch((e) => { console.warn('SignalR stop error:', e) })
      }
    }
  }, [connection])

  async function onDragEnd(result: DropResult) {
    const { source, destination, draggableId } = result
    if (!destination) return
    const srcCol = source.droppableId as Task['status']
    const destCol = destination.droppableId as Task['status']
    if (srcCol === destCol) return
    const id = parseInt(draggableId)
    const task = tasks.find(t => t.id === id)
    if (!task) return

    const optimistic: Task = { ...task, status: destCol }
    setTasks(prev => prev.map(t => t.id === id ? optimistic : t))
  try {
      const res = await api.put(`/api/tasks/${id}`, { status: destCol })
      setTasks(prev => prev.map(t => t.id === id ? res.data : t))
  } catch {
      setTasks(prev => prev.map(t => t.id === id ? task : t))
    }
  }

  async function handleDelete(e: React.MouseEvent, id: number) {
    e.preventDefault();
    e.stopPropagation();
    setConfirm({ open: true, id })
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="mx-auto max-w-7xl px-4 py-6">
        <div className="mb-6 flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
          <h1 className="text-2xl font-semibold">Tasks</h1>
          <div className="flex flex-1 items-center gap-3 md:justify-end">
            <input
              placeholder="Search..."
              value={search}
              onChange={e => setSearch(e.target.value)}
              className="w-full max-w-xs rounded-md border border-gray-300 px-3 py-2 outline-none focus:ring-2 focus:ring-blue-500"
            />
            <Select
              label={undefined}
              value={statusFilter}
              onChange={e => setStatusFilter(e.target.value as 'ALL' | Task['status'])}
              options={[
                { label: 'All', value: 'ALL' },
                { label: 'Todo', value: 'TODO' },
                { label: 'In Progress', value: 'IN_PROGRESS' },
                { label: 'Done', value: 'DONE' },
              ]}
              className="max-w-[160px]"
            />
            <Select
              label={undefined}
              value={assigneeFilter}
              onChange={e => setAssigneeFilter(e.target.value === '' ? '' : Number(e.target.value))}
              options={[{ label: 'Assignee: Any', value: '' }, ...users.map(u => ({ label: u.username, value: u.id }))]}
              className="max-w-[180px]"
            />
            <Button onMouseEnter={() => { import('../features/tasks/CreateTaskModal') }} onClick={() => setOpenCreate(true)}>+ Create Task</Button>
          </div>
        </div>
        <DragDropContext onDragEnd={onDragEnd}>
          <div className="grid gap-4 md:grid-cols-3">
            {columns.map(col => (
              <Droppable key={col.key} droppableId={col.key}>
                {(provided) => (
                  <div ref={provided.innerRef} {...provided.droppableProps} className="rounded-lg border bg-white p-3 shadow-sm min-h-[500px]">
                    <div className="mb-3 flex items-center justify-between">
                      <div className="font-medium">{col.title}</div>
                      <Badge color={col.key === 'DONE' ? 'green' : col.key === 'IN_PROGRESS' ? 'blue' : 'gray'}>
                        {grouped[col.key].length}
                      </Badge>
                    </div>
                    {grouped[col.key].length === 0 && (
                      <div className="text-sm text-gray-500">No tasks.</div>
                    )}
                    {grouped[col.key].map((t, idx) => (
                      <Draggable key={t.id} draggableId={String(t.id)} index={idx}>
                        {(prov) => (
                          <div ref={prov.innerRef} {...prov.draggableProps} {...prov.dragHandleProps}
                            className="mb-2 rounded border bg-gray-50 p-3">
                            <div className="font-semibold">{t.title}</div>
                            {t.description && <div className="mt-1 text-sm text-gray-600">{t.description}</div>}
                            <div className="mt-2 flex items-center justify-end gap-3">
                              <button onMouseEnter={() => { import('../features/tasks/EditTaskModal') }} onClick={(e) => { e.preventDefault(); e.stopPropagation(); setEditing(t) }} className="text-xs text-blue-600 hover:underline">Edit</button>
                              <button onClick={(e) => handleDelete(e, t.id)} className="text-xs text-red-600 hover:underline">Delete</button>
                            </div>
                          </div>
                        )}
                      </Draggable>
                    ))}
                    {provided.placeholder}
                  </div>
                )}
              </Droppable>
            ))}
          </div>
        </DragDropContext>
      </div>
      <Suspense fallback={null}>
        <CreateTaskModal
          open={openCreate}
          onClose={() => setOpenCreate(false)}
          onCreated={(t) => setTasks((prev) => {
            const exists = prev.some(x => x.id === t.id)
            return exists ? prev.map(x => x.id === t.id ? t : x) : [...prev, t]
          })}
        />
      </Suspense>
    {editing && (
        <Suspense fallback={null}>
          <EditTaskModal
            open={!!editing}
            task={editing}
            onClose={() => setEditing(null)}
            onUpdated={(ut) => setTasks(prev => prev.map(x => x.id === ut.id ? ut : x))}
          />
        </Suspense>
      )}
      <ConfirmDialog
        open={confirm.open}
        title="Delete task?"
        message="This cannot be undone."
        onCancel={() => setConfirm({ open: false })}
        onConfirm={async () => {
          if (!confirm.id) return
          try {
            await api.delete(`/api/tasks/${confirm.id}`)
            setTasks(prev => prev.filter(t => t.id !== confirm.id))
            toast('Task deleted', 'success')
          } catch {
            toast('Failed to delete task', 'error')
          } finally {
            setConfirm({ open: false })
          }
        }}
      />
    </div>
  )
}
