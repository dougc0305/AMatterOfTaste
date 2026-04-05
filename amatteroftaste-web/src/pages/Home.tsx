import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getCategories } from '../api/recipes';
import type { Category } from '../types';

export default function Home() {
  const [categories, setCategories] = useState<Category[]>([]);

  useEffect(() => {
    getCategories().then(setCategories);
  }, []);

  return (
    <div>
      {/* Hero */}
      <section className="relative py-32 text-center overflow-hidden min-h-[70vh] flex items-center justify-center">
        {/* Background image */}
        <div
          className="absolute inset-0 bg-cover bg-center"
          style={{ backgroundImage: "url('/hero.png')" }}
        />
        {/* Dark overlay for text readability */}
        <div className="absolute inset-0 bg-black/40" />
        <div className="relative z-10 max-w-3xl mx-auto px-6">
          <p className="text-sm tracking-widest uppercase text-white/70 mb-6" style={{ fontFamily: "'Cinzel', serif" }}>
            Welcome to
          </p>
          <h1
            className="text-7xl md:text-8xl text-white mb-6 drop-shadow-lg"
            style={{ fontFamily: "'Pinyon Script', cursive", fontWeight: 400 }}
          >
            A Matter of Taste
          </h1>
          <div className="text-white/40 text-2xl tracking-widest">&mdash;</div>
          <p className="text-xl text-white/80 mt-4 mb-10 italic">
            A collection of treasured family recipes, gathered with love over the years
          </p>
          <Link
            to="/browse"
            className="inline-block bg-white px-10 py-3 rounded-full no-underline hover:bg-white/90 transition-all text-sm tracking-widest uppercase shadow-lg"
            style={{ fontFamily: "'Cinzel', serif", color: '#1a1a1a', fontWeight: 600 }}
          >
            Browse Recipes
          </Link>
        </div>
      </section>

      {/* Categories */}
      <section className="max-w-4xl mx-auto px-6 py-20">
        <p className="text-center text-xs tracking-widest uppercase text-text-faint mb-2" style={{ fontFamily: "'Cinzel', serif" }}>
          Explore by
        </p>
        <h2 className="text-3xl text-wood-dark text-center mb-12">Category</h2>
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          {categories.map((cat) => (
            <Link
              key={cat.id}
              to={`/browse?category=${cat.id}`}
              className="block py-4 px-6 text-center no-underline text-text hover:text-terracotta rounded-lg hover:bg-linen transition-all group"
            >
              <span className="text-sm tracking-wide" style={{ fontFamily: "'Cinzel', serif" }}>
                {cat.name}
              </span>
              <span className="block w-0 group-hover:w-8 h-px bg-terracotta mx-auto mt-2 transition-all duration-300" />
            </Link>
          ))}
        </div>
      </section>
    </div>
  );
}
