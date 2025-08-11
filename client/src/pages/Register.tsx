import { useState } from 'react'
import { api } from '../lib/api'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import Input from '../components/Input'

const schema = z.object({
  username: z.string().min(3, 'Username must be at least 3 characters'),
  email: z.string().email('Enter a valid email'),
  password: z.string().min(6, 'Password must be at least 6 characters'),
})

type FormValues = z.infer<typeof schema>

export default function Register() {
  const [message, setMessage] = useState<string | null>(null)
  const { register, handleSubmit, formState: { errors, isSubmitting }, reset } = useForm<FormValues>({
    resolver: zodResolver(schema),
  })

  async function onSubmit(values: FormValues) {
    setMessage(null)
    try {
      await api.post('/api/auth/register', values)
      setMessage('Registered. You can login now.')
      reset()
    } catch (err: unknown) {
      type ErrResp = { response?: { data?: { message?: string } } }
      const message = (err as ErrResp)?.response?.data?.message
      setMessage(message || 'Registration failed')
    }
  }

  return (
    <div className="min-h-screen grid place-items-center bg-gray-50 p-4">
      <form onSubmit={handleSubmit(onSubmit)} className="w-full max-w-sm bg-white p-6 rounded shadow space-y-4" autoComplete="off" autoCapitalize="off" autoCorrect="off">
        <h1 className="text-xl font-semibold">Create account</h1>
        {message && <p className="text-sm">{message}</p>}
        <Input label="Username" error={errors.username?.message} {...register('username')} autoComplete="new-username" />
        <Input label="Email" type="email" error={errors.email?.message} {...register('email')} autoComplete="off" />
        <Input label="Password" type="password" error={errors.password?.message} {...register('password')} autoComplete="new-password" />
        <button disabled={isSubmitting} className="w-full bg-blue-600 text-white py-2 rounded hover:bg-blue-700 disabled:opacity-60">{isSubmitting ? 'Registering...' : 'Register'}</button>
      </form>
    </div>
  )
}
