import { BrowserRouter, Routes, Route, Navigate, Link } from 'react-router-dom'
import { Suspense, useEffect, useRef, lazy } from 'react'
const Login = lazy(() => import('./pages/Login'))
const Register = lazy(() => import('./pages/Register'))
const Board = lazy(() => import('./pages/Board'))
import './App.css'
import { useAppDispatch, useAppSelector } from './store'
import { logout } from './store/authSlice'
import ToastProvider from './components/ToastProvider'

function RequireAuth({ children }: { children: React.ReactElement }) {
  const token = localStorage.getItem('token')
  return token ? children : <Navigate to="/login" replace />
}

export default function App() {
  const { username } = useAppSelector(s => s.auth)
  const dispatch = useAppDispatch()
  const exp = useAppSelector(s => s.auth.exp)
  const timerRef = useRef<number | null>(null)
  function onLogout() {
    dispatch(logout())
  }

  useEffect(() => {
    if (timerRef.current) {
      window.clearTimeout(timerRef.current)
      timerRef.current = null
    }
    if (exp && exp * 1000 > Date.now()) {
      const ms = exp * 1000 - Date.now()
      timerRef.current = window.setTimeout(() => dispatch(logout()), ms)
    }
    return () => {
      if (timerRef.current) {
        window.clearTimeout(timerRef.current)
        timerRef.current = null
      }
    }
  }, [exp, dispatch])

  return (
    <BrowserRouter>
      <ToastProvider>
      <div className="border-b bg-white">
        <div className="mx-auto flex max-w-7xl items-center gap-3 px-4 py-3">
          <Link to="/" className="font-semibold">Task Manager</Link>
          <div className="ml-auto flex items-center gap-2">
            {username ? (
              <>
                <span className="text-sm text-gray-600">{username}</span>
                <button onClick={onLogout} className="text-sm text-blue-600">Logout</button>
              </>
            ) : (
              <>
                <Link to="/login" className="text-sm">Login</Link>
                <Link to="/register" className="text-sm">Register</Link>
              </>
            )}
          </div>
        </div>
      </div>
      <Suspense fallback={<div className="p-4 text-sm text-gray-500">Loading…</div>}>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/" element={<RequireAuth><Board /></RequireAuth>} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </Suspense>
      </ToastProvider>
    </BrowserRouter>
  )
}
