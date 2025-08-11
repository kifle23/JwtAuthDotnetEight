export type TaskStatus = 'TODO' | 'IN_PROGRESS' | 'DONE'
export type TaskPriority = 'LOW' | 'MEDIUM' | 'HIGH'
export type Task = {
  id: number
  title: string
  description?: string
  status: TaskStatus
  priority?: TaskPriority
  assigneeId?: number
  updatedAt?: string
}

export type User = {
  id: number
  username: string
  email?: string
}
