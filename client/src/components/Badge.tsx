import React from 'react'

export default function Badge({ color = 'gray', children }: { color?: 'gray' | 'blue' | 'green' | 'red'; children: React.ReactNode }) {
  const colors: Record<string, string> = {
    gray: 'bg-gray-100 text-gray-800',
    blue: 'bg-blue-100 text-blue-800',
    green: 'bg-green-100 text-green-800',
    red: 'bg-red-100 text-red-800',
  }
  return (
    <span className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${colors[color]}`}>{children}</span>
  )
}
