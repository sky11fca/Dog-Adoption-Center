import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { api } from '../api/api'

export default function AdminPage() {
  const { token, isAuthenticated } = useAuth()
  const navigate = useNavigate()
  const [users, setUsers] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [editingId, setEditingId] = useState(null)
  const [editForm, setEditForm] = useState({ username: '', email: '' })

  useEffect(() => {
    if (!isAuthenticated) {
      navigate('/login')
      return
    }
    fetchUsers()
  }, [isAuthenticated])

  const fetchUsers = async () => {
    setLoading(true)
    try {
      const data = await api.getUsers(token)
      setUsers(data)
    } catch {
      setError('Failed to load users.')
    } finally {
      setLoading(false)
    }
  }

  const handleDelete = async (id) => {
    if (!confirm('Delete this user?')) return
    try {
      await api.deleteUser(id, token)
      setUsers(users.filter(u => u.id !== id))
    } catch {
      setError('Failed to delete user.')
    }
  }

  const startEdit = (user) => {
    setEditingId(user.id)
    setEditForm({ username: user.username, email: user.email })
  }

  const handleUpdate = async (id) => {
    try {
      const updated = await api.updateUser(id, editForm, token)
      setUsers(users.map(u => u.id === id ? updated : u))
      setEditingId(null)
    } catch {
      setError('Failed to update user.')
    }
  }

  return (
    <div className="min-h-screen bg-amber-50 px-6 py-10">
      <div className="max-w-4xl mx-auto">
        <h1 className="text-2xl font-bold text-amber-800 mb-6">User Management</h1>
        {error && (
          <p className="text-red-500 mb-4 text-sm">{error}</p>
        )}
        {loading ? (
          <p className="text-gray-500">Loading...</p>
        ) : (
          <div className="bg-white rounded-2xl shadow overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-amber-100 text-amber-800">
                <tr>
                  <th className="px-4 py-3 text-left">Username</th>
                  <th className="px-4 py-3 text-left">Email</th>
                  <th className="px-4 py-3 text-left hidden md:table-cell">Created</th>
                  <th className="px-4 py-3 text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {users.map((user, i) => (
                  <tr key={user.id} className={i % 2 === 0 ? 'bg-white' : 'bg-amber-50'}>
                    <td className="px-4 py-3">
                      {editingId === user.id ? (
                        <input
                          value={editForm.username}
                          onChange={e => setEditForm({ ...editForm, username: e.target.value })}
                          className="border border-amber-300 rounded px-2 py-1 w-full focus:outline-none focus:ring-1 focus:ring-amber-400"
                        />
                      ) : (
                        <span className="font-medium">{user.username}</span>
                      )}
                    </td>
                    <td className="px-4 py-3">
                      {editingId === user.id ? (
                        <input
                          value={editForm.email}
                          onChange={e => setEditForm({ ...editForm, email: e.target.value })}
                          className="border border-amber-300 rounded px-2 py-1 w-full focus:outline-none focus:ring-1 focus:ring-amber-400"
                        />
                      ) : (
                        <span className="text-gray-600">{user.email}</span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-gray-400 hidden md:table-cell">
                      {new Date(user.createdAt).toLocaleDateString()}
                    </td>
                    <td className="px-4 py-3 text-right space-x-3">
                      {editingId === user.id ? (
                        <>
                          <button
                            onClick={() => handleUpdate(user.id)}
                            className="text-green-600 hover:text-green-800 font-medium transition"
                          >
                            Save
                          </button>
                          <button
                            onClick={() => setEditingId(null)}
                            className="text-gray-400 hover:text-gray-600 transition"
                          >
                            Cancel
                          </button>
                        </>
                      ) : (
                        <>
                          <button
                            onClick={() => startEdit(user)}
                            className="text-amber-600 hover:text-amber-800 font-medium transition"
                          >
                            Edit
                          </button>
                          <button
                            onClick={() => handleDelete(user.id)}
                            className="text-red-500 hover:text-red-700 font-medium transition"
                          >
                            Delete
                          </button>
                        </>
                      )}
                    </td>
                  </tr>
                ))}
                {users.length === 0 && (
                  <tr>
                    <td colSpan={4} className="px-4 py-8 text-center text-gray-400">
                      No users found.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  )
}
