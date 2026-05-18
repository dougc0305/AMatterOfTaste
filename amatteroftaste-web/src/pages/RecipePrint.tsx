import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { getRecipe } from '../api/recipes';
import type { Recipe } from '../types';

export default function RecipePrint() {
  const { id } = useParams<{ id: string }>();
  const [recipe, setRecipe] = useState<Recipe | null>(null);

  useEffect(() => {
    if (id) getRecipe(Number(id), true).then(setRecipe);
  }, [id]);

  if (!recipe) {
    return (
      <div className="p-8 text-black bg-white" style={{ fontFamily: "'EB Garamond', serif" }}>
        Loading...
      </div>
    );
  }

  return (
    <div
      className="max-w-3xl mx-auto p-8 bg-white text-black"
      style={{ fontFamily: "'EB Garamond', serif" }}
    >
      <h1 className="text-4xl font-bold mb-2">{recipe.title}</h1>
      <p className="text-xs uppercase tracking-widest mb-2">{recipe.categoryName}</p>
      {recipe.attribution && <p className="italic mb-4">{recipe.attribution}</p>}
      {recipe.description && <p className="mb-6 leading-relaxed">{recipe.description}</p>}

      {(recipe.servings || recipe.prepTimeMinutes || recipe.cookTimeMinutes) && (
        <div className="flex flex-wrap gap-x-8 gap-y-2 mb-6 text-sm border-y border-black/40 py-2">
          {recipe.servings && <div><strong>Serves:</strong> {recipe.servings}</div>}
          {recipe.prepTimeMinutes && <div><strong>Prep:</strong> {recipe.prepTimeMinutes} min</div>}
          {recipe.cookTimeMinutes && <div><strong>Cook:</strong> {recipe.cookTimeMinutes} min</div>}
        </div>
      )}

      {recipe.ingredients.length > 0 && (
        <section className="mb-6">
          <h2 className="text-xl font-bold mb-3 border-b border-black/40 pb-1">Ingredients</h2>
          <ul className="list-disc pl-6 space-y-1">
            {recipe.ingredients.map((ing) => (
              <li key={ing.id}>
                {ing.quantity && <strong>{ing.quantity} </strong>}
                {ing.unit && <span>{ing.unit} </span>}
                {ing.name}
              </li>
            ))}
          </ul>
        </section>
      )}

      {recipe.steps.length > 0 && (
        <section className="mb-6">
          <h2 className="text-xl font-bold mb-3 border-b border-black/40 pb-1">Instructions</h2>
          <ol className="list-decimal pl-6 space-y-3">
            {recipe.steps.map((step) => (
              <li key={step.id} className="leading-relaxed">{step.instruction}</li>
            ))}
          </ol>
        </section>
      )}

      {recipe.notes && (
        <section className="mb-6">
          <h2 className="text-xl font-bold mb-3 border-b border-black/40 pb-1">Notes</h2>
          <p className="whitespace-pre-wrap leading-relaxed">{recipe.notes}</p>
        </section>
      )}

      {recipe.story && (
        <section className="mb-6">
          <h2 className="text-xl font-bold mb-3 border-b border-black/40 pb-1">The Story</h2>
          <p className="italic whitespace-pre-wrap leading-relaxed">{recipe.story}</p>
        </section>
      )}
    </div>
  );
}
