export type AuthState = {
  token: string | null
  username: string | null
}

export const auth = {
  get(): AuthState {
    return {
      token: localStorage.getItem('token'),
      username: localStorage.getItem('username'),
    }
  },
  set(token: string, username: string) {
    localStorage.setItem('token', token)
    localStorage.setItem('username', username)
  },
  clear() {
    localStorage.removeItem('token')
    localStorage.removeItem('username')
  },
}
