import { api } from './client';

export interface ParsedIngredient {
  quantity: string;
  unit: string;
  name: string;
}

export interface ParsedStep {
  stepnumber: number;
  instruction: string;
}

export interface ParsedRecipe {
  title: string;
  description: string;
  servings: number | null;
  preptimeminutes: number | null;
  cooktimeminutes: number | null;
  ingredients: ParsedIngredient[];
  steps: ParsedStep[];
}

export interface ParsedNotes {
  ingredients: ParsedIngredient[];
  steps: ParsedStep[];
}

export function parseRecipe(text: string) {
  return api.post<ParsedRecipe>('/ai/parse-recipe', { text });
}

export function parseNotes(title: string, notes: string) {
  return api.post<ParsedNotes>('/ai/parse-notes', { title, notes });
}
