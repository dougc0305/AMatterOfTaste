import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { getRecipes, getCategories } from '../api/recipes';
import type { RecipeListItem } from '../api/recipes';
import type { Category } from '../types';

export default function Browse() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [recipes, setRecipes] = useState<RecipeListItem[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState(searchParams.get('search') ?? '');

  const categoryId = searchParams.get('category') ? Number(searchParams.get('category')) : undefined;

  useEffect(() => {
    getCategories().then(setCategories);
  }, []);

  useEffect(() => {
    getRecipes(page, 24, categoryId, search || undefined).then((result) => {
      setRecipes(result.items);
      setTotalCount(result.totalCount);
    });
  }, [page, categoryId, search]);

  const selectedCategory = categories.find((c) => c.id === categoryId);
  const totalPages = Math.ceil(totalCount / 24);

  // Try category-specific image, fall back to 'all'
  const [heroSrc, setHeroSrc] = useState('/categories/all.png');
  useEffect(() => {
    const slug = selectedCategory
      ? selectedCategory.name.toLowerCase().replace(/\s+/g, '-')
      : 'all';
    // Try png first, then jpg
    const img = new Image();
    img.onload = () => setHeroSrc(img.src);
    img.onerror = () => {
      const jpg = new Image();
      jpg.onload = () => setHeroSrc(jpg.src);
      jpg.onerror = () => setHeroSrc('/categories/all.png');
      jpg.src = `/categories/${slug}.jpg`;
    };
    img.src = `/categories/${slug}.png`;
  }, [selectedCategory]);

  return (
    <div>
      {/* Hero */}
      <section className="relative h-64 flex items-center justify-center overflow-hidden">
        <div
          className="absolute inset-0 bg-cover bg-center transition-all duration-500"
          style={{ backgroundImage: `url('${heroSrc}')` }}
        />
        <div className="absolute inset-0 bg-black/35" />
        <div className="relative z-10 text-center">
          <p className="text-xs tracking-widest uppercase text-white/60 mb-2" style={{ fontFamily: "'Cinzel', serif" }}>
            {selectedCategory ? 'Category' : 'All'}
          </p>
          <h1 className="text-5xl text-white drop-shadow-lg">
            {selectedCategory ? selectedCategory.name : 'Recipes'}
          </h1>
          {totalCount > 0 && (
            <p className="text-white/70 text-sm mt-3">{totalCount} recipes</p>
          )}
        </div>
      </section>

      <div className="max-w-5xl mx-auto px-6 py-10">

      {/* Filters */}
      <div className="flex flex-wrap gap-4 mb-10 items-center justify-center">
        <input
          type="text"
          placeholder="Search recipes..."
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPage(1);
          }}
          className="bg-warm-white border border-stone/30 rounded-full px-6 py-2.5 w-72 text-sm text-text placeholder:text-text-faint"
        />
        <select
          value={categoryId ?? ''}
          onChange={(e) => {
            const val = e.target.value;
            if (val) {
              setSearchParams({ category: val });
            } else {
              setSearchParams({});
            }
            setPage(1);
          }}
          className="bg-warm-white border border-stone/30 rounded-full px-6 py-2.5 text-sm text-text"
        >
          <option value="">All Categories</option>
          {categories.map((cat) => (
            <option key={cat.id} value={cat.id}>{cat.name}</option>
          ))}
        </select>
      </div>

      {/* Results */}
      {recipes.length === 0 ? (
        <p className="text-center text-text-faint py-16 italic">No recipes found.</p>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          {recipes.map((recipe) => (
            <Link
              key={recipe.id}
              to={`/recipe/${recipe.id}`}
              className="recipe-card block no-underline overflow-hidden group"
            >
              {recipe.primaryPhotoFilename ? (
                <img
                  src={`/photos/${recipe.primaryPhotoFilename}`}
                  alt={recipe.title}
                  className="w-full h-52 object-cover"
                />
              ) : (
                <img src="/default-recipe.png" alt={recipe.title} className="w-full h-52 object-cover" />
              )}
              <div className="p-5">
                <p className="text-xs tracking-widest uppercase text-terracotta-light mb-1" style={{ fontFamily: "'Cinzel', serif" }}>
                  {recipe.categoryName}
                </p>
                <h3 className="text-lg text-wood-dark mb-2 group-hover:text-terracotta transition-colors" style={{ fontFamily: "'Cinzel', serif" }}>
                  {recipe.title}
                </h3>
                {recipe.description && (
                  <p className="text-sm text-text-light line-clamp-2 leading-relaxed">{recipe.description}</p>
                )}
                {recipe.attribution && (
                  <p className="text-xs text-text-faint mt-3 italic">{recipe.attribution}</p>
                )}
              </div>
            </Link>
          ))}
        </div>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex justify-center items-center gap-4 mt-14">
          <button
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page === 1}
            className="text-sm text-text-light hover:text-terracotta disabled:opacity-30 bg-transparent border-none cursor-pointer tracking-widest uppercase"
            style={{ fontFamily: "'Cinzel', serif" }}
          >
            &larr; Previous
          </button>
          <span className="text-sm text-text-faint">
            {page} of {totalPages}
          </span>
          <button
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            disabled={page === totalPages}
            className="text-sm text-text-light hover:text-terracotta disabled:opacity-30 bg-transparent border-none cursor-pointer tracking-widest uppercase"
            style={{ fontFamily: "'Cinzel', serif" }}
          >
            Next &rarr;
          </button>
        </div>
      )}
      </div>
    </div>
  );
}
