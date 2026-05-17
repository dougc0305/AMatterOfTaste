import { Link } from 'react-router-dom';
import type { User } from '../../types';

interface NavProps {
  user: User | null;
  onLogout: () => void;
}

export default function Nav({ user, onLogout }: NavProps) {
  return (
    <nav className="bg-warm-white/80 backdrop-blur-sm sticky top-0 z-50" style={{ borderBottom: '1px solid rgba(201, 189, 168, 0.3)' }}>
      <div className="max-w-5xl mx-auto px-6 py-4 flex items-center justify-between">
        <Link to="/" className="no-underline group">
          <span className="text-3xl text-wood-dark group-hover:text-terracotta transition-colors" style={{ fontFamily: "'Pinyon Script', cursive" }}>
            A Matter of Taste
          </span>
        </Link>
        <div className="flex items-center gap-8 text-xs tracking-widest uppercase" style={{ fontFamily: "'Cinzel', serif" }}>
          <Link to="/browse" className="text-text-light hover:text-terracotta no-underline transition-colors">
            Recipes
          </Link>
          {user && (
            <Link to="/my-cookbook" className="text-text-light hover:text-terracotta no-underline transition-colors">
              My Cookbook
            </Link>
          )}
          {user?.isAdmin && (
            <Link to="/admin" className="text-text-light hover:text-terracotta no-underline transition-colors">
              Admin
            </Link>
          )}
          {user ? (
            <>
              <Link to="/account" className="text-text-light hover:text-terracotta no-underline transition-colors">
                Account
              </Link>
              <button
                onClick={onLogout}
                className="text-text-light hover:text-terracotta bg-transparent border-none cursor-pointer tracking-widest uppercase"
                style={{ fontFamily: "'Cinzel', serif", fontSize: '0.75rem' }}
              >
                Logout
              </button>
            </>
          ) : (
            <Link to="/login" className="text-text-light hover:text-terracotta no-underline transition-colors">
              Sign In
            </Link>
          )}
        </div>
      </div>
    </nav>
  );
}
