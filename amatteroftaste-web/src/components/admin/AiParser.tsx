import { useState } from 'react';

interface ParsedRecipe {
  title: string;
  description: string;
  servings: number | null;
  preptimeminutes: number | null;
  cooktimeminutes: number | null;
  ingredients: { quantity: string; unit: string; name: string }[];
  steps: { stepnumber: number; instruction: string }[];
}

interface AiParserProps {
  onParsed: (data: ParsedRecipe, rawText: string) => void;
  apiKey: string;
}

export default function AiParser({ onParsed, apiKey }: AiParserProps) {
  const [rawText, setRawText] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleParse = async () => {
    if (!rawText.trim() || !apiKey) return;
    setLoading(true);
    setError('');

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
              content: `Parse the following recipe text into structured JSON. Return ONLY valid JSON, no markdown, no preamble. Use this exact shape:
{
  "title": "string",
  "description": "brief description",
  "servings": number or null,
  "preptimeminutes": number or null,
  "cooktimeminutes": number or null,
  "ingredients": [{ "quantity": "string", "unit": "string", "name": "string" }],
  "steps": [{ "stepnumber": 1, "instruction": "string" }]
}

Recipe text:
${rawText}`,
            },
          ],
        }),
      });

      const data = await response.json();
      const text = data.content?.[0]?.text;
      if (!text) throw new Error('No response from AI');

      const parsed: ParsedRecipe = JSON.parse(text);
      onParsed(parsed, rawText);
      setRawText('');
    } catch {
      setError('Failed to parse recipe. You can enter the details manually.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="bg-butter/20 p-6 rounded-lg mb-8 border border-butter">
      <h3 className="text-lg text-china-blue mb-3" style={{ fontFamily: "'Cinzel', serif" }}>
        AI Recipe Parser
      </h3>
      <p className="text-sm text-gray-600 mb-3">
        Paste a recipe from any source and let AI parse it into structured fields.
      </p>
      <textarea
        value={rawText}
        onChange={(e) => setRawText(e.target.value)}
        rows={8}
        placeholder="Paste your recipe text here..."
        className="w-full border border-parchment rounded p-3 mb-3 focus:outline-none focus:border-china-blue"
      />
      {error && <p className="text-red-500 text-sm mb-3">{error}</p>}
      <button
        onClick={handleParse}
        disabled={loading || !rawText.trim() || !apiKey}
        className="bg-china-blue text-white px-6 py-2 rounded hover:bg-china-blue/90 disabled:opacity-50 transition"
        style={{ fontFamily: "'Cinzel', serif" }}
      >
        {loading ? 'Parsing...' : 'Parse Recipe'}
      </button>
      {!apiKey && (
        <p className="text-xs text-gray-400 mt-2">Enter your Anthropic API key above to enable AI parsing.</p>
      )}
    </div>
  );
}
