import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { register } from '../api/auth';
import type { User } from '../types';

interface RegisterProps {
  onLogin: (token: string, user: User) => void;
}

export default function Register({ onLogin }: RegisterProps) {
  const navigate = useNavigate();
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      const result = await register(name, email, password);
      onLogin(result.token, result.user);
      navigate('/');
    } catch {
      setError('Email may already be registered');
    }
  };

  return (
    <div className="max-w-sm mx-auto px-6 py-20">
      <div className="text-center mb-10">
        <h1 className="text-3xl text-wood-dark">Create Account</h1>
        <div className="divider-floral mt-2">&mdash;</div>
      </div>
      <form onSubmit={handleSubmit} className="space-y-5">
        {error && (
          <div className="bg-terracotta/10 text-terracotta p-3 rounded-lg text-sm text-center">{error}</div>
        )}
        <div>
          <label className="block text-xs tracking-widest uppercase text-text-faint mb-2" style={{ fontFamily: "'Cinzel', serif" }}>Name</label>
          <input
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
            className="w-full bg-warm-white border border-stone/30 rounded-lg px-4 py-3 text-text"
          />
        </div>
        <div>
          <label className="block text-xs tracking-widest uppercase text-text-faint mb-2" style={{ fontFamily: "'Cinzel', serif" }}>Email</label>
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            className="w-full bg-warm-white border border-stone/30 rounded-lg px-4 py-3 text-text"
          />
        </div>
        <div>
          <label className="block text-xs tracking-widest uppercase text-text-faint mb-2" style={{ fontFamily: "'Cinzel', serif" }}>Password</label>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            minLength={6}
            className="w-full bg-warm-white border border-stone/30 rounded-lg px-4 py-3 text-text"
          />
        </div>
        <button
          type="submit"
          className="w-full bg-sage text-white py-3 rounded-full hover:bg-sage-dark transition-all text-xs tracking-widest uppercase"
          style={{ fontFamily: "'Cinzel', serif" }}
        >
          Create Account
        </button>
      </form>
      <p className="text-center mt-8 text-sm text-text-faint">
        Already have an account?{' '}
        <Link to="/login" className="text-sage hover:text-terracotta no-underline">Sign in</Link>
      </p>
    </div>
  );
}
