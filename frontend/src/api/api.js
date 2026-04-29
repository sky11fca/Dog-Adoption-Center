const BASE_URL = '/api'

async function request(method, path, body, token) {
  const headers = { 'Content-Type': 'application/json' }
  if (token) headers['Authorization'] = `Bearer ${token}`

  const res = await fetch(`${BASE_URL}${path}`, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined,
  })

  if (!res.ok) throw new Error(`HTTP ${res.status}`)
  return res.json()
}

export const api = {
  login: (email, password) =>
    request('POST', '/login', { email, password }),
  register: (username, email, password) =>
    request('POST', '/users', { username, email, password }),
  getUsers: (token) =>
    request('GET', '/users', null, token),
  getUser: (id, token) =>
    request('GET', `/users/${id}`, null, token),
  updateUser: (id, data, token) =>
    request('PUT', `/users/${id}`, data, token),
  deleteUser: (id, token) =>
    request('DELETE', `/users/${id}`, null, token),
}
