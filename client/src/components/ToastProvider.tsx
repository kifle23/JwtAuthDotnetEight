import React, { useMemo, useState } from 'react'
import { ToastCtx, type Toast } from './toast-context'

export default function ToastProvider({ children }: { children: React.ReactNode }) {
  const [items, setItems] = useState<Toast[]>([])
  const toast = (message: string, type?: Toast['type']) => {
    const id = Date.now() + Math.random()
    setItems(prev => [...prev, { id, message, type }])
    window.setTimeout(() => setItems(prev => prev.filter(t => t.id !== id)), 3000)
  }
  const value = useMemo(() => ({ toast }), [])
  return (
    <ToastCtx.Provider value={value}>
      {children}
      <div className="pointer-events-none fixed inset-x-0 top-2 z-[60] mx-auto flex max-w-md flex-col gap-2 px-2">
        {items.map(t => (
          <div key={t.id} className={`pointer-events-auto rounded-md border p-2 text-sm shadow-sm ${t.type === 'error' ? 'border-red-200 bg-red-50 text-red-700' : t.type === 'success' ? 'border-green-200 bg-green-50 text-green-700' : 'border-gray-200 bg-white text-gray-800'}`}>
            {t.message}
          </div>
        ))}
      </div>
    </ToastCtx.Provider>
  )
}
