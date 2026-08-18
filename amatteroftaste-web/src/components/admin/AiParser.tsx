import { useState } from 'react';
import { parseRecipe } from '../../api/ai';
import type { ParsedRecipe } from '../../api/ai';

interface AiParserProps {
  onParsed: (data: ParsedRecipe, rawText: string) => void;
}

export default function AiParser({ onParsed }: AiParserProps) {
  const [rawText, setRawText] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleParse = async () => {
    if (!rawText.trim()) return;
    setLoading(true);
    setError('');

    try {
      const parsed = await parseRecipe(rawText);
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
        disabled={loading || !rawText.trim()}
        className="bg-china-blue text-white px-6 py-2 rounded hover:bg-china-blue/90 disabled:opacity-50 transition"
        style={{ fontFamily: "'Cinzel', serif" }}
      >
        {loading ? 'Parsing...' : 'Parse Recipe'}
      </button>
    </div>
  );
}
