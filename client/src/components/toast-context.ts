import { createContext } from 'react'

export type Toast = { id: number; message: string; type?: 'success' | 'error' | 'info' }
export type ToastCtxType = { toast: (message: string, type?: Toast['type']) => void }

export const ToastCtx = createContext<ToastCtxType | null>(null)
