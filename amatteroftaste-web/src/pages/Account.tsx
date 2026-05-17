import { useState } from 'react';
import { changePassword } from '../api/auth';

export default function Account() {
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setSubmitting(true);
    try {
      await changePassword(currentPassword, newPassword);
      setSuccess('Password changed');
      setCurrentPassword('');
      setNewPassword('');
    } catch {
      setError('Current password is incorrect');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="max-w-sm mx-auto px-6 py-20">
      <div className="text-center mb-10">
        <h1 className="text-3xl text-wood-dark">Account</h1>
        <div className="divider-floral mt-2">&mdash;</div>
        <p className="text-sm text-text-faint mt-4">Change your password</p>
      </div>
      <form onSubmit={handleSubmit} className="space-y-5">
        {error && (
          <div className="bg-terracotta/10 text-terracotta p-3 rounded-lg text-sm text-center">{error}</div>
        )}
        {success && (
          <div className="bg-sage/10 text-sage p-3 rounded-lg text-sm text-center">{success}</div>
        )}
        <div>
          <label className="block text-xs tracking-widest uppercase text-text-faint mb-2" style={{ fontFamily: "'Cinzel', serif" }}>Current Password</label>
          <input
            type="password"
            value={currentPassword}
            onChange={(e) => setCurrentPassword(e.target.value)}
            required
            className="w-full bg-warm-white border border-stone/30 rounded-lg px-4 py-3 text-text"
          />
        </div>
        <div>
          <label className="block text-xs tracking-widest uppercase text-text-faint mb-2" style={{ fontFamily: "'Cinzel', serif" }}>New Password</label>
          <input
            type="password"
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
            required
            className="w-full bg-warm-white border border-stone/30 rounded-lg px-4 py-3 text-text"
          />
        </div>
        <button
          type="submit"
          disabled={submitting}
          className="w-full bg-sage text-white py-3 rounded-full hover:bg-sage-dark transition-all text-xs tracking-widest uppercase disabled:opacity-60"
          style={{ fontFamily: "'Cinzel', serif" }}
        >
          {submitting ? 'Saving…' : 'Change Password'}
        </button>
      </form>
    </div>
  );
}
