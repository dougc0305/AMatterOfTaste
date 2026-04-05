import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getCategories, createRecipe, updateRecipe, getRecipes, deleteRecipe } from '../api/recipes';
import type { RecipeListItem } from '../api/recipes';
import type { Category } from '../types';
import AiParser from '../components/admin/AiParser';

interface IngredientRow {
  sortOrder: number;
  quantity: string;
  unit: string;
  name: string;
}

interface StepRow {
  stepNumber: number;
  instruction: string;
}

export default function Admin() {
  const navigate = useNavigate();
  const [categories, setCategories] = useState<Category[]>([]);
  const [recipes, setRecipes] = useState<RecipeListItem[]>([]);
  const [apiKey, setApiKey] = useState(() => localStorage.getItem('anthropic_api_key') ?? '');

  // Editor state
  const [editingId, setEditingId] = useState<number | null>(null);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [notes, setNotes] = useState('');
  const [story, setStory] = useState('');
  const [attribution, setAttribution] = useState('');
  const [originalText, setOriginalText] = useState('');
  const [categoryId, setCategoryId] = useState<number>(0);
  const [servings, setServings] = useState('');
  const [prepTime, setPrepTime] = useState('');
  const [cookTime, setCookTime] = useState('');
  const [ingredients, setIngredients] = useState<IngredientRow[]>([]);
  const [steps, setSteps] = useState<StepRow[]>([]);
  const [showEditor, setShowEditor] = useState(false);
  const [photos, setPhotos] = useState<{ id: number; filename: string; isPrimary: boolean }[]>([]);
  const [uploading, setUploading] = useState(false);
  const [parsingNotes, setParsingNotes] = useState(false);

  useEffect(() => {
    getCategories().then(setCategories);
    loadRecipes();
  }, []);

  const loadRecipes = () => {
    getRecipes(1, 100).then((result) => setRecipes(result.items));
  };

  const resetEditor = () => {
    setEditingId(null);
    setTitle('');
    setDescription('');
    setNotes('');
    setStory('');
    setAttribution('');
    setOriginalText('');
    setCategoryId(categories[0]?.id ?? 0);
    setServings('');
    setPrepTime('');
    setCookTime('');
    setIngredients([{ sortOrder: 0, quantity: '', unit: '', name: '' }]);
    setSteps([{ stepNumber: 1, instruction: '' }]);
    setPhotos([]);
  };

  const handleNew = () => {
    resetEditor();
    setShowEditor(true);
  };

  const handleEdit = async (id: number) => {
    const res = await fetch(`/api/recipes/${id}`);
    const recipe = await res.json();
    setEditingId(id);
    setTitle(recipe.title);
    setDescription(recipe.description ?? '');
    setNotes(recipe.notes ?? '');
    setStory(recipe.story ?? '');
    setAttribution(recipe.attribution ?? '');
    setOriginalText(recipe.originalText ?? '');
    setCategoryId(recipe.categoryId);
    setServings(recipe.servings?.toString() ?? '');
    setPrepTime(recipe.prepTimeMinutes?.toString() ?? '');
    setCookTime(recipe.cookTimeMinutes?.toString() ?? '');
    setIngredients(
      recipe.ingredients.length > 0
        ? recipe.ingredients.map((i: IngredientRow) => ({ sortOrder: i.sortOrder, quantity: i.quantity ?? '', unit: i.unit ?? '', name: i.name }))
        : [{ sortOrder: 0, quantity: '', unit: '', name: '' }]
    );
    setSteps(
      recipe.steps.length > 0
        ? recipe.steps.map((s: StepRow) => ({ stepNumber: s.stepNumber, instruction: s.instruction }))
        : [{ stepNumber: 1, instruction: '' }]
    );
    setPhotos(recipe.photos ?? []);
    setShowEditor(true);
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Are you sure you want to delete this recipe?')) return;
    await deleteRecipe(id);
    loadRecipes();
  };

  const handleSave = async () => {
    const data = {
      title,
      description: description || null,
      notes: notes || null,
      story: story || null,
      attribution: attribution || null,
      originalText: originalText || null,
      categoryId,
      servings: servings ? Number(servings) : null,
      prepTimeMinutes: prepTime ? Number(prepTime) : null,
      cookTimeMinutes: cookTime ? Number(cookTime) : null,
      ingredients: ingredients.filter((i) => i.name.trim()),
      steps: steps.filter((s) => s.instruction.trim()),
    };

    if (editingId) {
      await updateRecipe(editingId, data);
    } else {
      await createRecipe(data);
    }

    setShowEditor(false);
    loadRecipes();
  };

  const handleAiParsed = (parsed: {
    title: string;
    description: string;
    servings: number | null;
    preptimeminutes: number | null;
    cooktimeminutes: number | null;
    ingredients: { quantity: string; unit: string; name: string }[];
    steps: { stepnumber: number; instruction: string }[];
  }, rawText: string) => {
    setOriginalText(rawText);
    setTitle(parsed.title ?? '');
    setDescription(parsed.description ?? '');
    setServings(parsed.servings?.toString() ?? '');
    setPrepTime(parsed.preptimeminutes?.toString() ?? '');
    setCookTime(parsed.cooktimeminutes?.toString() ?? '');
    setIngredients(
      parsed.ingredients?.map((i, idx) => ({
        sortOrder: idx,
        quantity: i.quantity ?? '',
        unit: i.unit ?? '',
        name: i.name ?? '',
      })) ?? []
    );
    setSteps(
      parsed.steps?.map((s) => ({
        stepNumber: s.stepnumber,
        instruction: s.instruction,
      })) ?? []
    );
  };

  const handleApiKeyChange = (key: string) => {
    setApiKey(key);
    localStorage.setItem('anthropic_api_key', key);
  };

  const handleParseNotes = async () => {
    if (!notes.trim()) return;
    if (!apiKey) {
      alert('Enter your Anthropic API key at the top of the page first.');
      return;
    }
    setParsingNotes(true);
    try {
      const response = await fetch('https://api.anthropic.com/v1/messages', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'x-api-key': apiKey,
          'anthropic-version': '2023-06-01',
          'anthropic-dangerous-direct-browser-access': 'true',
        },
        body: JSON.stringify({
          model: 'claude-sonnet-4-20250514',
          max_tokens: 2048,
          messages: [
            {
              role: 'user',
              content: `Parse the following recipe notes into structured ingredients and steps. The notes contain both ingredients and instructions mixed together. Separate them out. Return ONLY valid JSON, no markdown, no preamble. Use this exact shape:
{
  "ingredients": [{ "quantity": "string or empty", "unit": "string or empty", "name": "string" }],
  "steps": [{ "stepnumber": 1, "instruction": "string" }]
}

Recipe title: ${title}

Notes:
${notes}`,
            },
          ],
        }),
      });

      const data = await response.json();
      const text = data.content?.[0]?.text;
      if (!text) throw new Error('No response');

      const parsed = JSON.parse(text);
      if (parsed.ingredients?.length) {
        setIngredients(
          parsed.ingredients.map((i: { quantity: string; unit: string; name: string }, idx: number) => ({
            sortOrder: idx,
            quantity: i.quantity ?? '',
            unit: i.unit ?? '',
            name: i.name ?? '',
          }))
        );
      }
      if (parsed.steps?.length) {
        setSteps(
          parsed.steps.map((s: { stepnumber: number; instruction: string }) => ({
            stepNumber: s.stepnumber,
            instruction: s.instruction,
          }))
        );
      }
      // Save current notes as original text, then clear notes
      setOriginalText(notes);
      setNotes('');
    } catch {
      alert('Failed to parse notes. You can break them out manually.');
    } finally {
      setParsingNotes(false);
    }
  };

  const handleSetPrimary = async (photoId: number) => {
    const token = localStorage.getItem('token');
    const res = await fetch(`/api/photos/${photoId}/primary`, {
      method: 'PUT',
      headers: { Authorization: `Bearer ${token}` },
    });
    if (res.ok) {
      setPhotos((prev) => prev.map((p) => ({ ...p, isPrimary: p.id === photoId })));
    }
  };

  const handleDeletePhoto = async (photoId: number) => {
    if (!confirm('Delete this photo?')) return;
    const token = localStorage.getItem('token');
    const res = await fetch(`/api/photos/${photoId}`, {
      method: 'DELETE',
      headers: { Authorization: `Bearer ${token}` },
    });
    if (res.ok) {
      setPhotos((prev) => {
        const remaining = prev.filter((p) => p.id !== photoId);
        // If we deleted the primary, the API promotes the next one
        const deleted = prev.find((p) => p.id === photoId);
        if (deleted?.isPrimary && remaining.length > 0) {
          remaining[0].isPrimary = true;
        }
        return remaining;
      });
    }
  };

  const handlePhotoUpload = async (recipeId: number, file: File) => {
    setUploading(true);
    try {
      const formData = new FormData();
      formData.append('file', file);
      const token = localStorage.getItem('token');
      const res = await fetch(`/api/photos/upload/${recipeId}`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${token}` },
        body: formData,
      });
      if (res.ok) {
        const photo = await res.json();
        setPhotos((prev) => [...prev, photo]);
      }
    } finally {
      setUploading(false);
    }
  };

  const addIngredient = () =>
    setIngredients([...ingredients, { sortOrder: ingredients.length, quantity: '', unit: '', name: '' }]);

  const addStep = () =>
    setSteps([...steps, { stepNumber: steps.length + 1, instruction: '' }]);

  const updateIngredient = (idx: number, field: keyof IngredientRow, value: string) => {
    const updated = [...ingredients];
    updated[idx] = { ...updated[idx], [field]: value };
    setIngredients(updated);
  };

  const removeIngredient = (idx: number) =>
    setIngredients(ingredients.filter((_, i) => i !== idx));

  const updateStep = (idx: number, value: string) => {
    const updated = [...steps];
    updated[idx].instruction = value;
    setSteps(updated);
  };

  const removeStep = (idx: number) =>
    setSteps(steps.filter((_, i) => i !== idx).map((s, i) => ({ ...s, stepNumber: i + 1 })));

  if (!showEditor) {
    return (
      <div className="max-w-6xl mx-auto px-4 py-8">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-3xl text-wood-dark">Admin — Recipes</h1>
          <button
            onClick={handleNew}
            className="bg-terracotta text-white px-6 py-2 rounded hover:bg-terracotta/90 transition"
            style={{ fontFamily: "'Cinzel', serif" }}
          >
            + New Recipe
          </button>
        </div>

        <table className="w-full border-collapse">
          <thead>
            <tr className="border-b-2 border-parchment text-left">
              <th className="py-2 text-wood-dark" style={{ fontFamily: "'Cinzel', serif" }}>Title</th>
              <th className="py-2 text-wood-dark" style={{ fontFamily: "'Cinzel', serif" }}>Category</th>
              <th className="py-2 text-wood-dark text-right" style={{ fontFamily: "'Cinzel', serif" }}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {recipes.map((r) => (
              <tr key={r.id} className="border-b border-parchment/50 hover:bg-parchment/10">
                <td className="py-3">{r.title}</td>
                <td className="py-3 text-gray-800">{r.categoryName}</td>
                <td className="py-3 text-right space-x-3">
                  <button onClick={() => navigate(`/recipe/${r.id}`)} className="text-wood-dark hover:text-terracotta bg-transparent border-none cursor-pointer text-sm">View</button>
                  <button onClick={() => handleEdit(r.id)} className="text-wood-dark hover:text-terracotta bg-transparent border-none cursor-pointer text-sm">Edit</button>
                  <button onClick={() => handleDelete(r.id)} className="text-red-500 hover:text-red-700 bg-transparent border-none cursor-pointer text-sm">Delete</button>
                </td>
              </tr>
            ))}
            {recipes.length === 0 && (
              <tr>
                <td colSpan={3} className="text-center py-8 text-gray-600">No recipes yet. Add your first one!</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl text-wood-dark">
          {editingId ? 'Edit Recipe' : 'New Recipe'}
        </h1>
        <button
          onClick={() => setShowEditor(false)}
          className="text-gray-700 hover:text-gray-700 bg-transparent border-none cursor-pointer"
        >
          Cancel
        </button>
      </div>

      {/* API Key */}
      <div className="mb-4">
        <label className="block text-xs text-gray-700 mb-1">Anthropic API Key (stored locally)</label>
        <input
          type="password"
          value={apiKey}
          onChange={(e) => handleApiKeyChange(e.target.value)}
          placeholder="sk-ant-..."
          className="w-full border border-parchment rounded px-3 py-1 text-sm focus:outline-none focus:border-china-blue"
        />
      </div>

      {/* AI Parser */}
      <AiParser onParsed={handleAiParsed} apiKey={apiKey} />

      {/* Recipe Form */}
      <div className="space-y-4">
        <div>
          <label className="block text-sm text-gray-800 font-semibold mb-1" style={{ fontFamily: "'Cinzel', serif" }}>Title</label>
          <input value={title} onChange={(e) => setTitle(e.target.value)} className="w-full border border-parchment rounded px-4 py-2 focus:outline-none focus:border-china-blue" />
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm text-gray-800 font-semibold mb-1" style={{ fontFamily: "'Cinzel', serif" }}>Category</label>
            <select value={categoryId} onChange={(e) => setCategoryId(Number(e.target.value))} className="w-full border border-parchment rounded px-4 py-2 focus:outline-none focus:border-china-blue">
              <option value={0}>Select...</option>
              {categories.map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-sm text-gray-800 font-semibold mb-1" style={{ fontFamily: "'Cinzel', serif" }}>Attribution</label>
            <input value={attribution} onChange={(e) => setAttribution(e.target.value)} placeholder="From Grandma's kitchen" className="w-full border border-parchment rounded px-4 py-2 focus:outline-none focus:border-china-blue" />
          </div>
        </div>

        <div>
          <label className="block text-sm text-gray-800 font-semibold mb-1" style={{ fontFamily: "'Cinzel', serif" }}>Description</label>
          <textarea value={description} onChange={(e) => setDescription(e.target.value)} rows={2} className="w-full border border-parchment rounded px-4 py-2 focus:outline-none focus:border-china-blue" />
        </div>

        <div className="grid grid-cols-3 gap-4">
          <div>
            <label className="block text-sm text-gray-800 font-semibold mb-1" style={{ fontFamily: "'Cinzel', serif" }}>Servings</label>
            <input type="number" value={servings} onChange={(e) => setServings(e.target.value)} className="w-full border border-parchment rounded px-4 py-2 focus:outline-none focus:border-china-blue" />
          </div>
          <div>
            <label className="block text-sm text-gray-800 font-semibold mb-1" style={{ fontFamily: "'Cinzel', serif" }}>Prep (min)</label>
            <input type="number" value={prepTime} onChange={(e) => setPrepTime(e.target.value)} className="w-full border border-parchment rounded px-4 py-2 focus:outline-none focus:border-china-blue" />
          </div>
          <div>
            <label className="block text-sm text-gray-800 font-semibold mb-1" style={{ fontFamily: "'Cinzel', serif" }}>Cook (min)</label>
            <input type="number" value={cookTime} onChange={(e) => setCookTime(e.target.value)} className="w-full border border-parchment rounded px-4 py-2 focus:outline-none focus:border-china-blue" />
          </div>
        </div>

        {/* Ingredients */}
        <div>
          <div className="flex justify-between items-center mb-2">
            <label className="text-sm text-gray-800 font-semibold" style={{ fontFamily: "'Cinzel', serif" }}>Ingredients</label>
            <button onClick={addIngredient} className="text-sm text-wood-dark hover:text-terracotta bg-transparent border-none cursor-pointer">+ Add</button>
          </div>
          {ingredients.map((ing, idx) => (
            <div key={idx} className="flex gap-2 mb-2">
              <input value={ing.quantity} onChange={(e) => updateIngredient(idx, 'quantity', e.target.value)} placeholder="Qty" className="w-20 border border-parchment rounded px-2 py-1 text-sm focus:outline-none focus:border-china-blue" />
              <input value={ing.unit} onChange={(e) => updateIngredient(idx, 'unit', e.target.value)} placeholder="Unit" className="w-24 border border-parchment rounded px-2 py-1 text-sm focus:outline-none focus:border-china-blue" />
              <input value={ing.name} onChange={(e) => updateIngredient(idx, 'name', e.target.value)} placeholder="Ingredient name" className="flex-1 border border-parchment rounded px-2 py-1 text-sm focus:outline-none focus:border-china-blue" />
              <button onClick={() => removeIngredient(idx)} className="text-red-400 hover:text-red-600 bg-transparent border-none cursor-pointer text-sm">×</button>
            </div>
          ))}
        </div>

        {/* Steps */}
        <div>
          <div className="flex justify-between items-center mb-2">
            <label className="text-sm text-gray-800 font-semibold" style={{ fontFamily: "'Cinzel', serif" }}>Instructions</label>
            <button onClick={addStep} className="text-sm text-wood-dark hover:text-terracotta bg-transparent border-none cursor-pointer">+ Add Step</button>
          </div>
          {steps.map((step, idx) => (
            <div key={idx} className="flex gap-2 mb-2">
              <span className="flex-shrink-0 w-8 h-8 bg-china-blue text-white rounded-full flex items-center justify-center text-sm">
                {step.stepNumber}
              </span>
              <textarea value={step.instruction} onChange={(e) => updateStep(idx, e.target.value)} rows={2} className="flex-1 border border-parchment rounded px-2 py-1 text-sm focus:outline-none focus:border-china-blue" />
              <button onClick={() => removeStep(idx)} className="text-red-400 hover:text-red-600 bg-transparent border-none cursor-pointer text-sm">×</button>
            </div>
          ))}
        </div>

        {/* Story & Notes */}
        <div>
          <label className="block text-sm text-gray-800 font-semibold mb-1" style={{ fontFamily: "'Cinzel', serif" }}>Story</label>
          <textarea value={story} onChange={(e) => setStory(e.target.value)} rows={3} placeholder="The story behind this recipe..." className="w-full border border-parchment rounded px-4 py-2 focus:outline-none focus:border-china-blue" />
        </div>
        <div>
          <label className="block text-sm text-gray-800 font-semibold mb-1" style={{ fontFamily: "'Cinzel', serif" }}>Notes</label>
          <textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={5} placeholder="Additional notes..." className="w-full border border-parchment rounded px-4 py-2 focus:outline-none focus:border-china-blue" />
          {notes.trim() && (
            <button
              onClick={handleParseNotes}
              disabled={parsingNotes}
              className="mt-2 px-4 py-2 rounded transition"
              style={{ backgroundColor: '#C4835A', color: '#fff', fontWeight: 700, fontSize: '0.9rem' }}
            >
              {parsingNotes ? 'Parsing...' : 'Parse Notes into Ingredients & Steps'}
            </button>
          )}
        </div>

        {/* Original Text */}
        {originalText && (
          <div>
            <label className="block text-sm text-gray-800 font-semibold mb-1" style={{ fontFamily: "'Cinzel', serif" }}>Original Recipe Text</label>
            <textarea value={originalText} readOnly rows={6} className="w-full border border-parchment rounded px-4 py-2 bg-linen/50 text-text-light text-sm cursor-default" />
          </div>
        )}

        {/* Photos */}
        {editingId && (
          <div>
            <label className="block text-sm text-gray-800 font-semibold mb-2" style={{ fontFamily: "'Cinzel', serif" }}>Photos</label>
            <div className="flex flex-wrap gap-4 mb-3">
              {photos.map((p) => (
                <div key={p.id} className="relative group">
                  <img src={`/photos/${p.filename}`} alt="" className="w-32 h-32 object-cover rounded-lg" />
                  {p.isPrimary && (
                    <span className="absolute top-1 left-1 bg-terracotta text-white text-xs px-2 py-0.5 rounded">Primary</span>
                  )}
                  <div className="absolute bottom-0 left-0 right-0 bg-black/60 rounded-b-lg p-1 flex justify-center gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                    {!p.isPrimary && (
                      <button
                        onClick={() => handleSetPrimary(p.id)}
                        className="text-white text-xs bg-transparent border-none cursor-pointer hover:text-butter"
                      >
                        Set Primary
                      </button>
                    )}
                    <button
                      onClick={() => handleDeletePhoto(p.id)}
                      className="text-red-300 text-xs bg-transparent border-none cursor-pointer hover:text-red-100"
                    >
                      Delete
                    </button>
                  </div>
                </div>
              ))}
            </div>
            <label className="inline-block bg-sage text-white px-4 py-2 rounded cursor-pointer hover:bg-sage-dark transition text-sm">
              {uploading ? 'Uploading...' : 'Upload Photo'}
              <input
                type="file"
                accept="image/*"
                className="hidden"
                disabled={uploading}
                onChange={(e) => {
                  const file = e.target.files?.[0];
                  if (file && editingId) handlePhotoUpload(editingId, file);
                  e.target.value = '';
                }}
              />
            </label>
          </div>
        )}
        {!editingId && (
          <p className="text-sm text-text-faint italic">Save the recipe first, then you can upload photos.</p>
        )}

        {/* Save */}
        <div className="flex gap-3 pt-4">
          <button
            onClick={handleSave}
            className="px-8 py-3 rounded transition"
            style={{ fontFamily: "'Cinzel', serif", backgroundColor: '#6B7F6B', color: '#fff', fontWeight: 700, fontSize: '1rem' }}
          >
            {editingId ? 'Update Recipe' : 'Create Recipe'}
          </button>
          <button
            onClick={() => setShowEditor(false)}
            className="px-8 py-3 rounded transition"
            style={{ fontFamily: "'Cinzel', serif", border: '2px solid #8B7355', color: '#3A2E1E', fontWeight: 600 }}
          >
            Cancel
          </button>
        </div>
      </div>
    </div>
  );
}
