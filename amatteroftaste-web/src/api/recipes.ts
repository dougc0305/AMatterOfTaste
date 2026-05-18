import { api } from './client';
import type { Recipe, Category } from '../types';

export interface RecipeListItem {
  id: number;
  title: string;
  description: string | null;
  attribution: string | null;
  categoryId: number;
  categoryName: string;
  primaryPhotoFilename: string | null;
  viewCount: number;
}

export interface RecipePagedResult {
  items: RecipeListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export function getRecipes(page = 1, pageSize = 20, categoryId?: number, search?: string) {
  const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
  if (categoryId) params.set('categoryId', String(categoryId));
  if (search) params.set('search', search);
  return api.get<RecipePagedResult>(`/recipes?${params}`);
}

export function getRecipe(id: number, noCount = false) {
  return api.get<Recipe>(`/recipes/${id}${noCount ? '?noCount=true' : ''}`);
}

export function getCategories() {
  return api.get<Category[]>('/categories');
}

export function createRecipe(data: unknown) {
  return api.post<{ id: number }>('/recipes', data);
}

export function updateRecipe(id: number, data: unknown) {
  return api.put<{ id: number }>(`/recipes/${id}`, data);
}

export function deleteRecipe(id: number) {
  return api.delete(`/recipes/${id}`);
}
