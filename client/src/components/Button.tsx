import React from 'react'

type Props = React.ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: 'primary' | 'secondary' | 'ghost'
  size?: 'sm' | 'md'
  loading?: boolean
}

export default function Button({
  variant = 'primary',
  size = 'md',
  className = '',
  children,
  loading = false,
  ...rest
}: Props) {
  const base = 'inline-flex items-center justify-center rounded-md font-medium transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2'
  const sizes = {
    sm: 'px-3 py-1.5 text-sm',
    md: 'px-4 py-2',
  }[size]
  const variants = {
    primary: 'bg-blue-600 text-white hover:bg-blue-700 focus:ring-blue-500',
    secondary: 'bg-gray-100 text-gray-900 hover:bg-gray-200 focus:ring-gray-400',
    ghost: 'bg-transparent text-blue-600 hover:bg-blue-50 focus:ring-blue-300',
  }[variant]
  return (
    <button className={`${base} ${sizes} ${variants} ${className}`} disabled={loading || rest.disabled} {...rest}>
      {loading ? '...' : children}
    </button>
  )
}
