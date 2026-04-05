import { api } from './client';

export interface FavoriteItem {
  id: number;
  recipeId: number;
  recipeTitle: string;
  primaryPhotoFilename: string | null;
}

export function getFavorites() {
  return api.get<FavoriteItem[]>('/favorites');
}

export function addFavorite(recipeId: number) {
  return api.post<{ id: number }>('/favorites', { recipeId });
}

export function removeFavorite(id: number) {
  return api.delete(`/favorites/${id}`);
}
