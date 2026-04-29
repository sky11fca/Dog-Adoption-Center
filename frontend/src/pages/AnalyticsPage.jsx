import { useEffect, useState, useCallback } from 'react'
import { api } from '../api/api'

const MetricCard = ({ label, value, icon }) => (
  <div className="bg-white rounded-2xl shadow p-6 text-center">
    <div className="text-3xl mb-1">{icon}</div>
    <p className="text-3xl font-bold text-amber-700">{value}</p>
    <p className="text-sm text-gray-500 mt-1">{label}</p>
  </div>
)

const METRIC_META = {
  adoptions_last_7d:    { label: 'Adoptions',    sub: 'last 7 days', icon: '🐾' },
  applications_last_7d: { label: 'Applications', sub: 'last 7 days', icon: '📋' },
  pet_views_last_7d:    { label: 'Pet Views',    sub: 'last 7 days', icon: '👁️' },
}

export default function AnalyticsPage() {
  const [metrics, setMetrics] = useState([])
  const [trends, setTrends] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [lastUpdated, setLastUpdated] = useState(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const from = new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString()
      const to = new Date().toISOString()
      const [m, t] = await Promise.all([api.getMetrics(), api.getTrends(from, to)])
      setMetrics(m)
      setTrends([...t].reverse())
      setLastUpdated(new Date())
    } catch {
      setError('Failed to load analytics.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { load() }, [load])

  return (
    <div className="min-h-screen bg-amber-50 px-6 py-10">
      <div className="max-w-4xl mx-auto space-y-8">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-amber-800">Analytics</h1>
            {lastUpdated && (
              <p className="text-xs text-gray-400 mt-0.5">
                Last updated {lastUpdated.toLocaleTimeString()}
              </p>
            )}
          </div>
          <button
            onClick={load}
            disabled={loading}
            className="bg-amber-700 hover:bg-amber-600 disabled:bg-gray-300 text-white text-sm font-semibold px-4 py-2 rounded-lg transition"
          >
            {loading ? 'Loading...' : 'Refresh'}
          </button>
        </div>

        {error && <p className="text-red-500 text-sm">{error}</p>}

        {!loading && (
          <>
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
              {metrics.map(m => {
                const meta = METRIC_META[m.metricName]
                return (
                  <MetricCard
                    key={m.metricName}
                    icon={meta?.icon ?? '📊'}
                    label={`${meta?.label ?? m.metricName} · ${meta?.sub ?? ''}`}
                    value={m.value}
                  />
                )
              })}
            </div>

            <div className="bg-white rounded-2xl shadow overflow-hidden">
              <div className="px-6 py-4 border-b border-amber-100">
                <h2 className="font-semibold text-amber-800">Trends — last 30 days</h2>
                <p className="text-xs text-gray-400 mt-0.5">Most recent first</p>
              </div>
              {trends.length === 0 ? (
                <p className="px-6 py-8 text-gray-400 text-sm">No trend data yet — visit the home page to generate views.</p>
              ) : (
                <table className="w-full text-sm">
                  <thead className="bg-amber-50 text-amber-800 text-xs uppercase tracking-wide">
                    <tr>
                      <th className="px-6 py-3 text-left">Date</th>
                      <th className="px-6 py-3 text-right">🐾 Adoptions</th>
                      <th className="px-6 py-3 text-right">📋 Applications</th>
                      <th className="px-6 py-3 text-right">👁️ Views</th>
                    </tr>
                  </thead>
                  <tbody>
                    {trends.map((t, i) => (
                      <tr key={t.date} className={i % 2 === 0 ? 'bg-white' : 'bg-amber-50'}>
                        <td className="px-6 py-3 text-gray-600 font-medium">
                          {new Date(t.date).toLocaleDateString(undefined, { weekday: 'short', month: 'short', day: 'numeric' })}
                        </td>
                        <td className="px-6 py-3 text-right font-semibold text-amber-700">{t.adoptions}</td>
                        <td className="px-6 py-3 text-right text-gray-600">{t.applications}</td>
                        <td className="px-6 py-3 text-right text-gray-400">{t.views}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
          </>
        )}
      </div>
    </div>
  )
}
