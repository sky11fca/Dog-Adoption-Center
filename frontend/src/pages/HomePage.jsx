import { useEffect, useRef, useState } from 'react'
import { api } from '../api/api'
import { useAuth } from '../contexts/AuthContext'

const MOCK_DOGS = [
  { id: 1, name: 'Buddy', breed: 'Golden Retriever', age: '2 years', status: 'Available' },
  { id: 2, name: 'Max', breed: 'German Shepherd', age: '3 years', status: 'Available' },
  { id: 3, name: 'Bella', breed: 'Labrador', age: '1 year', status: 'Pending' },
  { id: 4, name: 'Charlie', breed: 'Beagle', age: '4 years', status: 'Available' },
  { id: 5, name: 'Luna', breed: 'Husky', age: '2 years', status: 'Available' },
  { id: 6, name: 'Rocky', breed: 'Bulldog', age: '5 years', status: 'Adopted' },
]

const statusStyle = {
  Available: 'bg-green-100 text-green-700',
  Pending: 'bg-yellow-100 text-yellow-700',
  Adopted: 'bg-gray-100 text-gray-500',
}

function AdoptModal({ dog, onClose }) {
  const { user } = useAuth()
  const [form, setForm] = useState({ message: '' })
  const [submitting, setSubmitting] = useState(false)
  const [success, setSuccess] = useState(false)

  const handleSubmit = async (e) => {
    e.preventDefault()
    setSubmitting(true)
    try {
      await api.trackEvent({
        petId: '00000000-0000-0000-0000-00000000000' + dog.id,
        userId: '00000000-0000-0000-0000-000000000002',
        shelterId: '00000000-0000-0000-0000-000000000001',
        eventType: 'application.submitted',
        occurredAt: new Date().toISOString(),
        metadata: { petName: dog.name, applicantName: user?.username ?? '' },
      })
    } catch {
      // analytics is best-effort
    }
    setSuccess(true)
    setSubmitting(false)
  }

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 px-4">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-md p-6">
        {success ? (
          <div className="text-center py-6">
            <div className="text-5xl mb-4">🐾</div>
            <h2 className="text-xl font-bold text-amber-800 mb-2">Application Submitted!</h2>
            <p className="text-gray-500 text-sm mb-6">
              We'll be in touch about <strong>{dog.name}</strong> soon.
            </p>
            <button
              onClick={onClose}
              className="bg-amber-700 hover:bg-amber-600 text-white font-semibold px-6 py-2 rounded-lg transition"
            >
              Close
            </button>
          </div>
        ) : (
          <>
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-bold text-amber-800">Adopt {dog.name}</h2>
              <button onClick={onClose} className="text-gray-400 hover:text-gray-600 text-xl leading-none">&times;</button>
            </div>
            <form onSubmit={handleSubmit} className="space-y-4">
              <p className="text-sm text-gray-500">
                Applying as <span className="font-medium text-gray-700">{user?.username ?? user?.email}</span>
              </p>
              <div>
                <label className="block text-xs text-gray-500 mb-1">Why do you want to adopt {dog.name}?</label>
                <textarea
                  required
                  value={form.message}
                  onChange={e => setForm({ ...form, message: e.target.value })}
                  rows={3}
                  className="w-full border border-amber-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-amber-400 resize-none"
                  placeholder="Tell us a bit about yourself..."
                />
              </div>
              <button
                type="submit"
                disabled={submitting}
                className="w-full bg-amber-700 hover:bg-amber-600 disabled:bg-gray-300 text-white font-semibold py-2 rounded-lg transition"
              >
                {submitting ? 'Submitting...' : 'Submit Application'}
              </button>
            </form>
          </>
        )}
      </div>
    </div>
  )
}

export default function HomePage() {
  const dogsRef = useRef(null)
  const [selectedDog, setSelectedDog] = useState(null)

  const scrollToDogs = () => dogsRef.current?.scrollIntoView({ behavior: 'smooth' })

  useEffect(() => {
    MOCK_DOGS.forEach(dog => {
      api.trackEvent({
        petId: '00000000-0000-0000-0000-00000000000' + dog.id,
        userId: '00000000-0000-0000-0000-000000000002',
        shelterId: '00000000-0000-0000-0000-000000000001',
        eventType: 'pet.viewed',
        occurredAt: new Date().toISOString(),
        metadata: { petName: dog.name },
      }).catch(() => {})
    })
  }, [])

  return (
    <div className="min-h-screen bg-amber-50">
      <div className="bg-amber-700 text-white py-16 px-6 text-center">
        <h1 className="text-4xl font-bold mb-3">Find Your Perfect Companion</h1>
        <p className="text-amber-200 text-lg max-w-xl mx-auto">
          Every dog deserves a loving home. Browse our available dogs and start your adoption journey today.
        </p>
        <button
          onClick={scrollToDogs}
          className="mt-6 bg-white text-amber-700 font-semibold px-6 py-2 rounded-full hover:bg-amber-100 transition"
        >
          Start Adopting
        </button>
      </div>

      <div ref={dogsRef} className="max-w-5xl mx-auto px-6 py-12">
        <h2 className="text-2xl font-bold text-amber-800 mb-6">Dogs Available for Adoption</h2>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {MOCK_DOGS.map(dog => (
            <div key={dog.id} className="bg-white rounded-2xl shadow hover:shadow-md transition overflow-hidden">
              <div className="bg-amber-100 h-36 flex items-center justify-center text-6xl select-none">
                🐕
              </div>
              <div className="p-4">
                <div className="flex items-center justify-between mb-1">
                  <h3 className="text-lg font-bold text-gray-800">{dog.name}</h3>
                  <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${statusStyle[dog.status]}`}>
                    {dog.status}
                  </span>
                </div>
                <p className="text-sm text-gray-500">{dog.breed} · {dog.age}</p>
                <button
                  disabled={dog.status !== 'Available'}
                  onClick={() => setSelectedDog(dog)}
                  className="mt-3 w-full bg-amber-700 hover:bg-amber-600 disabled:bg-gray-200 disabled:text-gray-400 disabled:cursor-not-allowed text-white text-sm font-semibold py-1.5 rounded-lg transition"
                >
                  {dog.status === 'Available' ? 'Adopt Me' : dog.status}
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>

      {selectedDog && (
        <AdoptModal dog={selectedDog} onClose={() => setSelectedDog(null)} />
      )}
    </div>
  )
}
