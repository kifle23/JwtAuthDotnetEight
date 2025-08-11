import React from 'react'

type Option = { label: string; value: string | number }

type Props = React.SelectHTMLAttributes<HTMLSelectElement> & {
  label?: string
  hint?: string
  error?: string
  options: Option[]
}

const Select = React.forwardRef<HTMLSelectElement, Props>(
  ({ label, hint, error, options, className = '', ...rest }, ref) => (
    <label className="block">
      {label && <div className="mb-1 text-sm font-medium text-gray-700">{label}</div>}
      <select
        ref={ref}
        className={`w-full rounded-md border px-3 py-2 outline-none transition focus:ring-2 focus:ring-blue-500 ${error ? 'border-red-500' : 'border-gray-300'} ${className}`}
        {...rest}
      >
        {options.map((o) => (
          <option key={o.value} value={o.value}>
            {o.label}
          </option>
        ))}
      </select>
      {hint && !error && <div className="mt-1 text-xs text-gray-500">{hint}</div>}
      {error && <div className="mt-1 text-xs text-red-600">{error}</div>}
    </label>
  )
)

export default Select
