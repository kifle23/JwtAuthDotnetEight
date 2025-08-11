import React from 'react'

type Props = React.InputHTMLAttributes<HTMLInputElement> & {
  label?: string
  hint?: string
  error?: string
}

const Input = React.forwardRef<HTMLInputElement, Props>(
  ({ label, hint, error, className = '', ...rest }, ref) => {
  const autoComplete = rest.autoComplete ?? (rest.type === 'password' ? 'new-password' : 'off')
    return (
      <label className="block">
        {label && <div className="mb-1 text-sm font-medium text-gray-700">{label}</div>}
        <input
          ref={ref}
          className={`w-full rounded-md border px-3 py-2 outline-none transition focus:ring-2 focus:ring-blue-500 ${error ? 'border-red-500' : 'border-gray-300'} ${className}`}
      autoComplete={autoComplete}
      {...rest}
        />
        {hint && !error && <div className="mt-1 text-xs text-gray-500">{hint}</div>}
        {error && <div className="mt-1 text-xs text-red-600">{error}</div>}
      </label>
    )
  }
)

export default Input
