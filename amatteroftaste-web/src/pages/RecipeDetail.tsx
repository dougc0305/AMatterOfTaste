import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getRecipe } from '../api/recipes';
import { addFavorite } from '../api/favorites';
import type { Recipe } from '../types';

interface RecipeDetailProps {
  isLoggedIn: boolean;
}

export default function RecipeDetail({ isLoggedIn }: RecipeDetailProps) {
  const { id } = useParams<{ id: string }>();
  const [recipe, setRecipe] = useState<Recipe | null>(null);
  const [favoriteAdded, setFavoriteAdded] = useState(false);

  useEffect(() => {
    if (id) getRecipe(Number(id)).then(setRecipe);
  }, [id]);

  if (!recipe) {
    return <div className="max-w-3xl mx-auto px-6 py-20 text-center text-text-faint italic">Loading...</div>;
  }

  const handleFavorite = async () => {
    try {
      await addFavorite(recipe.id);
      setFavoriteAdded(true);
    } catch {
      alert('Could not add to favorites. You may have already added this recipe.');
    }
  };

  const pinterestUrl = recipe.photos.length > 0
    ? `https://pinterest.com/pin/create/button/?url=${encodeURIComponent(window.location.href)}&media=${encodeURIComponent(window.location.origin + '/photos/' + recipe.photos[0].filename)}&description=${encodeURIComponent(recipe.title)}`
    : null;

  return (
    <div className="max-w-3xl mx-auto px-6 py-12">
      <Link to="/browse" className="text-text-faint hover:text-terracotta no-underline text-xs tracking-widest uppercase" style={{ fontFamily: "'Cinzel', serif" }}>
        &larr; Back to Recipes
      </Link>

      {/* Header with photo left, title right */}
      <div className="flex flex-col md:flex-row gap-8 mt-8 mb-10 items-start">
        {/* Photo with blurred edges */}
        <div className="md:w-72 flex-shrink-0">
          <img
            src={recipe.photos.length > 0 ? `/photos/${recipe.photos[0].filename}` : '/default-recipe.png'}
            alt={recipe.title}
            className="w-full h-64 object-cover"
            style={{
              maskImage: 'radial-gradient(ellipse 80% 75% at center, black 30%, transparent 95%)',
              WebkitMaskImage: 'radial-gradient(ellipse 80% 75% at center, black 30%, transparent 95%)',
            }}
          />
        </div>

        {/* Title & description */}
        <div className="flex-1 md:pt-2">
          <p className="text-xs tracking-widest uppercase text-terracotta-light mb-3" style={{ fontFamily: "'Cinzel', serif" }}>
            {recipe.categoryName}
          </p>
          <h1 className="text-3xl md:text-4xl text-wood-dark mb-4">{recipe.title}</h1>
          {recipe.attribution && (
            <p className="text-text-light italic mb-4">{recipe.attribution}</p>
          )}
          {recipe.description && (
            <p className="text-text-light leading-relaxed mb-5">{recipe.description}</p>
          )}

          {/* Meta */}
          <div className="flex flex-wrap gap-6 mb-5 text-sm text-text-faint">
            {recipe.servings && (
              <div>
                <span className="text-xs tracking-widest uppercase" style={{ fontFamily: "'Cinzel', serif" }}>Serves </span>
                <span className="text-wood">{recipe.servings}</span>
              </div>
            )}
            {recipe.prepTimeMinutes && (
              <div>
                <span className="text-xs tracking-widest uppercase" style={{ fontFamily: "'Cinzel', serif" }}>Prep </span>
                <span className="text-wood">{recipe.prepTimeMinutes} min</span>
              </div>
            )}
            {recipe.cookTimeMinutes && (
              <div>
                <span className="text-xs tracking-widest uppercase" style={{ fontFamily: "'Cinzel', serif" }}>Cook </span>
                <span className="text-wood">{recipe.cookTimeMinutes} min</span>
              </div>
            )}
          </div>

          {/* Actions */}
          <div className="flex gap-3">
            {isLoggedIn && (
              <button
                onClick={handleFavorite}
                disabled={favoriteAdded}
                className="bg-sage text-white px-5 py-2 rounded-full hover:bg-sage-dark disabled:opacity-50 transition-all text-xs tracking-widest uppercase"
                style={{ fontFamily: "'Cinzel', serif" }}
              >
                {favoriteAdded ? 'Saved!' : 'Save to My Cookbook'}
              </button>
            )}
            {pinterestUrl && (
              <a
                href={pinterestUrl}
                target="_blank"
                rel="noopener noreferrer"
                className="px-5 py-2 rounded-full no-underline transition-all text-xs tracking-widest uppercase"
                style={{ fontFamily: "'Cinzel', serif", backgroundColor: '#C4835A', color: '#fff', fontWeight: 600 }}
              >
                Pin It
              </a>
            )}
          </div>
        </div>
      </div>

      <div className="divider-floral">&middot; &middot; &middot;</div>

      {/* Ingredients & Steps */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-12 mt-10">
        {/* Ingredients */}
        <div className="md:col-span-1">
          <h2 className="text-lg text-wood-dark mb-6 tracking-widest uppercase text-xs" style={{ fontFamily: "'Cinzel', serif" }}>
            Ingredients
          </h2>
          {recipe.ingredients.length > 0 ? (
            <ul className="space-y-3 list-none pl-0">
              {recipe.ingredients.map((ing) => (
                <li key={ing.id} className="text-text pb-2" style={{ borderBottom: '1px solid rgba(201, 189, 168, 0.2)' }}>
                  {ing.quantity && <span className="text-wood font-semibold">{ing.quantity} </span>}
                  {ing.unit && <span className="text-text-light">{ing.unit} </span>}
                  {ing.name}
                </li>
              ))}
            </ul>
          ) : (
            <p className="text-text-faint italic text-sm">See notes below</p>
          )}
        </div>

        {/* Steps */}
        <div className="md:col-span-2">
          <h2 className="text-lg text-wood-dark mb-6 tracking-widest uppercase text-xs" style={{ fontFamily: "'Cinzel', serif" }}>
            Instructions
          </h2>
          {recipe.steps.length > 0 ? (
            <ol className="space-y-6 list-none pl-0">
              {recipe.steps.map((step) => (
                <li key={step.id} className="flex gap-4">
                  <span className="flex-shrink-0 w-8 h-8 bg-linen text-wood rounded-full flex items-center justify-center text-sm" style={{ fontFamily: "'Cinzel', serif" }}>
                    {step.stepNumber}
                  </span>
                  <p className="text-text pt-1 leading-relaxed">{step.instruction}</p>
                </li>
              ))}
            </ol>
          ) : (
            <p className="text-text-faint italic text-sm">See notes below</p>
          )}
        </div>
      </div>

      {/* Notes */}
      {recipe.notes && (
        <div className="mt-12 bg-linen/60 p-8 rounded-xl">
          <h2 className="text-xs tracking-widest uppercase text-wood-dark mb-4" style={{ fontFamily: "'Cinzel', serif" }}>Notes</h2>
          <p className="text-text leading-relaxed whitespace-pre-wrap">{recipe.notes}</p>
        </div>
      )}

      {/* Story */}
      {recipe.story && (
        <div className="mt-10 p-8 rounded-xl" style={{ borderLeft: '3px solid rgba(212, 169, 106, 0.4)', background: 'rgba(232, 213, 163, 0.1)' }}>
          <h2 className="text-xs tracking-widest uppercase text-wood-dark mb-4" style={{ fontFamily: "'Cinzel', serif" }}>
            The Story Behind This Recipe
          </h2>
          <p className="text-text-light italic leading-relaxed whitespace-pre-wrap">{recipe.story}</p>
        </div>
      )}
    </div>
  );
}
