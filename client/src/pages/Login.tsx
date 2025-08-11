import { useState } from 'react'
import { api } from '../lib/api'
import { useAppDispatch } from '../store'
import { loginSuccess } from '../store/authSlice'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import Input from '../components/Input'

const schema = z.object({
  username: z.string().min(3, 'Username must be at least 3 characters'),
  password: z.string().min(6, 'Password must be at least 6 characters'),
})

type FormValues = z.infer<typeof schema>

export default function Login() {
  const [error, setError] = useState<string | null>(null)
  const dispatch = useAppDispatch()
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
  })

  async function onSubmit(values: FormValues) {
    setError(null)
    try {
      const res = await api.post('/api/auth/login', values)
      const token: string = res.data.token || res.data.Token || res.data
      dispatch(loginSuccess({ token, username: values.username }))
      window.location.href = '/'
    } catch (err: unknown) {
      type ErrResp = { response?: { data?: { message?: string } } }
      const message = (err as ErrResp)?.response?.data?.message
      setError(message || 'Login failed')
    }
  }

  return (
    <div className="min-h-screen grid place-items-center bg-gray-50 p-4">
      <form onSubmit={handleSubmit(onSubmit)} className="w-full max-w-sm bg-white p-6 rounded shadow space-y-4" autoComplete="off" autoCapitalize="off" autoCorrect="off">
        <h1 className="text-xl font-semibold">Sign in</h1>
        {error && <p className="text-sm text-red-600">{error}</p>}
        <Input label="Username" error={errors.username?.message} {...register('username')} autoComplete="off" />
        <Input label="Password" type="password" error={errors.password?.message} {...register('password')} autoComplete="new-password" />
        <button disabled={isSubmitting} className="w-full bg-blue-600 text-white py-2 rounded hover:bg-blue-700 disabled:opacity-60">{isSubmitting ? 'Signing in...' : 'Login'}</button>
      </form>
    </div>
  )
}
