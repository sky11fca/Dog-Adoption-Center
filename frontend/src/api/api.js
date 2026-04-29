async function request(method, baseUrl, path, body, token) {
  const headers = { 'Content-Type': 'application/json' }
  if (token) headers['Authorization'] = `Bearer ${token}`

  const res = await fetch(`${baseUrl}${path}`, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined,
  })

  if (!res.ok) throw new Error(`HTTP ${res.status}`)
  if (res.status === 204 || res.status === 202) return null
  return res.json()
}

const user = (method, path, body, token) => request(method, '/api', path, body, token)
const reviews = (method, path, body, token) => request(method, '/api/reviews', path, body, token)
const analytics = (method, path, body, token) => request(method, '/api/analytics', path, body, token)

export const api = {
  login: (email, password) =>
    user('POST', '/login', { email, password }),
  register: (username, email, password) =>
    user('POST', '/users', { username, email, password }),
  getUsers: (token) =>
    user('GET', '/users', null, token),
  getUser: (id, token) =>
    user('GET', `/users/${id}`, null, token),
  updateUser: (id, data, token) =>
    user('PUT', `/users/${id}`, data, token),
  deleteUser: (id, token) =>
    user('DELETE', `/users/${id}`, null, token),

  getReviewsByShelter: (shelterId) =>
    reviews('GET', `/shelter/${shelterId}`),
  getShelterSummary: (shelterId) =>
    reviews('GET', `/shelter/${shelterId}/summary`),
  createReview: (data) =>
    reviews('POST', '/', data),
  deleteReview: (id) =>
    reviews('DELETE', `/${id}`),

  getMetrics: () =>
    analytics('GET', '/metrics'),
  getTrends: (from, to) =>
    analytics('GET', `/trends?from=${from}&to=${to}`),
  trackEvent: (event) =>
    analytics('POST', '/events', event),
}
