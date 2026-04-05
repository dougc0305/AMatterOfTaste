export interface Category {
  id: number;
  name: string;
  sortOrder: number;
}

export interface Ingredient {
  id: number;
  recipeId: number;
  sortOrder: number;
  quantity: string | null;
  unit: string | null;
  name: string;
}

export interface Step {
  id: number;
  recipeId: number;
  stepNumber: number;
  instruction: string;
}

export interface RecipePhoto {
  id: number;
  recipeId: number;
  filename: string;
  isPrimary: boolean;
  sortOrder: number;
}

export interface Recipe {
  id: number;
  title: string;
  description: string | null;
  notes: string | null;
  story: string | null;
  attribution: string | null;
  originalText: string | null;
  categoryId: number;
  categoryName: string;
  servings: number | null;
  prepTimeMinutes: number | null;
  cookTimeMinutes: number | null;
  ingredients: Ingredient[];
  steps: Step[];
  photos: RecipePhoto[];
}

export interface User {
  id: number;
  name: string;
  email: string;
  cookbookSlug: string | null;
  isAdmin: boolean;
}

export interface LoginResponse {
  token: string;
  user: User;
}
