import { createSlice, type PayloadAction } from '@reduxjs/toolkit'

export type AuthState = {
  token: string | null
  username: string | null
  exp: number | null
}

const initialState: AuthState = {
  token: localStorage.getItem('token'),
  username: localStorage.getItem('username'),
  exp: null,
}

function decodeJwt(token: string): { exp?: number; [k: string]: unknown } | null {
  try {
    const payload = token.split('.')[1]
    const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'))
    return JSON.parse(json)
  } catch {
    return null
  }
}

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    loginSuccess(state, action: PayloadAction<{ token: string; username: string }>) {
      const { token, username } = action.payload
      state.token = token
      state.username = username
      localStorage.setItem('token', token)
      localStorage.setItem('username', username)
      const payload = decodeJwt(token)
      state.exp = payload?.exp ?? null
    },
    logout(state) {
      state.token = null
      state.username = null
      state.exp = null
      localStorage.removeItem('token')
      localStorage.removeItem('username')
      window.location.href = '/login'
    },
  },
})

export const { loginSuccess, logout } = authSlice.actions
export default authSlice.reducer
