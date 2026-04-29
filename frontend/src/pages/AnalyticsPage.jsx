import { useEffect, useState, useCallback, useRef } from 'react'
import { api } from '../api/api'

const REFRESH_INTERVAL = 10_000

const MetricCard = ({ label, sub, icon, value, prev }) => {
  const delta = prev !== null && value !== prev ? value - prev : null
  return (
    <div className="bg-white rounded-2xl shadow p-6 text-center relative overflow-hidden">
      <div className="text-3xl mb-1">{icon}</div>
      <p className="text-3xl font-bold text-amber-700">{value}</p>
      {delta !== null && (
        <span className={`absolute top-3 right-3 text-xs font-semibold px-2 py-0.5 rounded-full ${delta > 0 ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-600'}`}>
          {delta > 0 ? '+' : ''}{delta}
        </span>
      )}
      <p className="text-sm font-medium text-gray-700 mt-1">{label}</p>
      <p className="text-xs text-gray-400">{sub}</p>
    </div>
  )
}


const METRIC_META = {
  adoptions_last_7d:    { label: 'Adoptions',    sub: 'triggered by: completing an adoption', icon: '🐾' },
  applications_last_7d: { label: 'Applications', sub: 'triggered by: submitting "Adopt Me" form', icon: '📋' },
  pet_views_last_7d:    { label: 'Pet Views',    sub: 'triggered by: visiting the home page', icon: '👁️' },
}

export default function AnalyticsPage() {
  const [metrics, setMetrics] = useState([])
  const [prevMetrics, setPrevMetrics] = useState({})
  const [trends, setTrends] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [lastUpdated, setLastUpdated] = useState(null)
  const [secondsUntilRefresh, setSecondsUntilRefresh] = useState(REFRESH_INTERVAL / 1000)
  const timerRef = useRef(null)
  const countdownRef = useRef(null)

  const load = useCallback(async (showSpinner = false) => {
    if (showSpinner) setLoading(true)
    setError('')
    try {
      const from = new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString()
      const to = new Date().toISOString()
      const [m, t] = await Promise.all([api.getMetrics(), api.getTrends(from, to)])
      setPrevMetrics(prev => {
        const map = {}
        m.forEach(x => { map[x.metricName] = prev[x.metricName] ?? null })
        return map
      })
      setMetrics(m)
      setTrends([...t].reverse())
      setLastUpdated(new Date())
      setSecondsUntilRefresh(REFRESH_INTERVAL / 1000)
    } catch {
      setError('Failed to load analytics.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    load(true)

    timerRef.current = setInterval(() => load(false), REFRESH_INTERVAL)
    countdownRef.current = setInterval(() => {
      setSecondsUntilRefresh(s => (s <= 1 ? REFRESH_INTERVAL / 1000 : s - 1))
    }, 1000)

    return () => {
      clearInterval(timerRef.current)
      clearInterval(countdownRef.current)
    }
  }, [load])

  return (
    <div className="min-h-screen bg-amber-50 px-6 py-10">
      <div className="max-w-4xl mx-auto space-y-8">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-amber-800">Analytics</h1>
            {lastUpdated && (
              <p className="text-xs text-gray-400 mt-0.5">
                Updated {lastUpdated.toLocaleTimeString()} · refreshing in {secondsUntilRefresh}s
              </p>
            )}
          </div>
          <div className="flex gap-2">
            <button
              onClick={() => load(false)}
              disabled={loading}
              className="bg-amber-700 hover:bg-amber-600 disabled:bg-gray-300 text-white text-sm font-semibold px-4 py-2 rounded-lg transition"
            >
              {loading ? 'Loading...' : 'Refresh now'}
            </button>
          </div>
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
                    label={meta?.label ?? m.metricName}
                    sub={meta?.sub ?? ''}
                    value={m.value}
                    prev={prevMetrics[m.metricName] ?? null}
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
                <p className="px-6 py-8 text-gray-400 text-sm">No trend data yet.</p>
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
