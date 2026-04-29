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

export default function HomePage() {
  return (
    <div className="min-h-screen bg-amber-50">
      <div className="bg-amber-700 text-white py-16 px-6 text-center">
        <h1 className="text-4xl font-bold mb-3">Find Your Perfect Companion</h1>
        <p className="text-amber-200 text-lg max-w-xl mx-auto">
          Every dog deserves a loving home. Browse our available dogs and start your adoption journey today.
        </p>
        <button className="mt-6 bg-white text-amber-700 font-semibold px-6 py-2 rounded-full hover:bg-amber-100 transition">
          Start Adopting
        </button>
      </div>

      <div className="max-w-5xl mx-auto px-6 py-12">
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
                  className="mt-3 w-full bg-amber-700 hover:bg-amber-600 disabled:bg-gray-200 disabled:text-gray-400 disabled:cursor-not-allowed text-white text-sm font-semibold py-1.5 rounded-lg transition"
                >
                  {dog.status === 'Available' ? 'Adopt Me' : dog.status}
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}
