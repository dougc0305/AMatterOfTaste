import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getFavorites, removeFavorite } from '../api/favorites';
import type { FavoriteItem } from '../api/favorites';

export default function MyCookbook() {
  const [favorites, setFavorites] = useState<FavoriteItem[]>([]);

  useEffect(() => {
    getFavorites().then(setFavorites);
  }, []);

  const handleRemove = async (id: number) => {
    await removeFavorite(id);
    setFavorites((prev) => prev.filter((f) => f.id !== id));
  };

  return (
    <div className="max-w-5xl mx-auto px-6 py-12">
      <div className="text-center mb-12">
        <h1 className="text-4xl text-wood-dark">My Cookbook</h1>
        <div className="divider-floral">&mdash;</div>
      </div>

      {favorites.length === 0 ? (
        <div className="text-center py-16">
          <p className="text-text-faint italic mb-6">You haven't saved any recipes yet.</p>
          <Link
            to="/browse"
            className="inline-block bg-sage text-white px-8 py-2.5 rounded-full no-underline hover:bg-sage-dark transition-all text-xs tracking-widest uppercase"
            style={{ fontFamily: "'Cinzel', serif" }}
          >
            Browse Recipes
          </Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          {favorites.map((fav) => (
            <div key={fav.id} className="recipe-card overflow-hidden">
              <Link to={`/recipe/${fav.recipeId}`} className="no-underline">
                {fav.primaryPhotoFilename ? (
                  <img
                    src={`/photos/${fav.primaryPhotoFilename}`}
                    alt={fav.recipeTitle}
                    className="w-full h-48 object-cover"
                  />
                ) : (
                  <img src="/default-recipe.png" alt={fav.recipeTitle} className="w-full h-48 object-cover" />
                )}
                <div className="p-5">
                  <h3 className="text-lg text-wood-dark" style={{ fontFamily: "'Cinzel', serif" }}>
                    {fav.recipeTitle}
                  </h3>
                </div>
              </Link>
              <div className="px-5 pb-4">
                <button
                  onClick={() => handleRemove(fav.id)}
                  className="text-xs text-text-faint hover:text-terracotta bg-transparent border-none cursor-pointer tracking-widest uppercase"
                  style={{ fontFamily: "'Cinzel', serif" }}
                >
                  Remove
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
