import { useEffect, useState } from 'react'
import { api } from '../api/api'

const DEMO_SHELTER_ID = '00000000-0000-0000-0000-000000000001'
const DEMO_USER_ID = '00000000-0000-0000-0000-000000000002'

const Stars = ({ value }) => (
  <span className="text-amber-400 text-sm">
    {'★'.repeat(value)}{'☆'.repeat(5 - value)}
  </span>
)

export default function ReviewsPage() {
  const [reviews, setReviews] = useState([])
  const [summary, setSummary] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [form, setForm] = useState({ userName: '', rating: 5, comment: '' })
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    fetchReviews()
  }, [])

  const fetchReviews = async () => {
    setLoading(true)
    try {
      const [data, sum] = await Promise.all([
        api.getReviewsByShelter(DEMO_SHELTER_ID),
        api.getShelterSummary(DEMO_SHELTER_ID),
      ])
      setReviews(data)
      setSummary(sum)
    } catch {
      setError('Failed to load reviews.')
    } finally {
      setLoading(false)
    }
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setSubmitting(true)
    setError('')
    try {
      await api.createReview({
        shelterId: DEMO_SHELTER_ID,
        userId: DEMO_USER_ID,
        userName: form.userName,
        rating: Number(form.rating),
        comment: form.comment,
      })
      setForm({ userName: '', rating: 5, comment: '' })
      await fetchReviews()
    } catch {
      setError('Failed to submit review.')
    } finally {
      setSubmitting(false)
    }
  }

  const handleDelete = async (id) => {
    if (!confirm('Delete this review?')) return
    try {
      await api.deleteReview(id)
      setReviews(reviews.filter(r => r.id !== id))
    } catch {
      setError('Failed to delete review.')
    }
  }

  return (
    <div className="min-h-screen bg-amber-50 px-6 py-10">
      <div className="max-w-3xl mx-auto space-y-8">
        <div>
          <h1 className="text-2xl font-bold text-amber-800">Shelter Reviews</h1>
          {summary && (
            <p className="text-gray-500 text-sm mt-1">
              <Stars value={Math.round(summary.averageRating)} />
              {' '}{summary.averageRating.toFixed(1)} · {summary.totalReviews} review{summary.totalReviews !== 1 ? 's' : ''}
            </p>
          )}
        </div>

        {error && <p className="text-red-500 text-sm">{error}</p>}

        <form onSubmit={handleSubmit} className="bg-white rounded-2xl shadow p-6 space-y-4">
          <h2 className="font-semibold text-amber-800">Leave a Review</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs text-gray-500 mb-1">Your name</label>
              <input
                required
                value={form.userName}
                onChange={e => setForm({ ...form, userName: e.target.value })}
                className="w-full border border-amber-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-amber-400"
                placeholder="John Doe"
              />
            </div>
            <div>
              <label className="block text-xs text-gray-500 mb-1">Rating</label>
              <select
                value={form.rating}
                onChange={e => setForm({ ...form, rating: e.target.value })}
                className="w-full border border-amber-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-amber-400"
              >
                {[5, 4, 3, 2, 1].map(n => (
                  <option key={n} value={n}>{'★'.repeat(n)} ({n})</option>
                ))}
              </select>
            </div>
          </div>
          <div>
            <label className="block text-xs text-gray-500 mb-1">Comment</label>
            <textarea
              required
              value={form.comment}
              onChange={e => setForm({ ...form, comment: e.target.value })}
              rows={3}
              className="w-full border border-amber-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-amber-400 resize-none"
              placeholder="Share your experience..."
            />
          </div>
          <button
            type="submit"
            disabled={submitting}
            className="bg-amber-700 hover:bg-amber-600 disabled:bg-gray-300 text-white text-sm font-semibold px-5 py-2 rounded-lg transition"
          >
            {submitting ? 'Submitting...' : 'Submit Review'}
          </button>
        </form>

        {loading ? (
          <p className="text-gray-400">Loading reviews...</p>
        ) : reviews.length === 0 ? (
          <p className="text-gray-400">No reviews yet. Be the first!</p>
        ) : (
          <div className="space-y-4">
            {reviews.map(r => (
              <div key={r.id} className="bg-white rounded-2xl shadow p-5">
                <div className="flex items-center justify-between mb-1">
                  <div>
                    <span className="font-semibold text-gray-800 text-sm">{r.userName}</span>
                    <span className="ml-3"><Stars value={r.rating} /></span>
                  </div>
                  <div className="flex items-center gap-3">
                    <span className="text-xs text-gray-400">
                      {new Date(r.createdAt).toLocaleDateString()}
                    </span>
                    <button
                      onClick={() => handleDelete(r.id)}
                      className="text-red-400 hover:text-red-600 text-xs transition"
                    >
                      Delete
                    </button>
                  </div>
                </div>
                <p className="text-sm text-gray-600 mt-1">{r.comment}</p>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}
