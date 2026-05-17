import { useEffect } from 'react';
import { useLocation } from 'react-router-dom';
import { pingVisit } from '../api/visits';

const DEDUPE_KEY = 'visit_last_ping_at';
const DEDUPE_MS = 60 * 1000;

export function usePageView() {
  const location = useLocation();

  useEffect(() => {
    const last = Number(sessionStorage.getItem(DEDUPE_KEY) ?? 0);
    const now = Date.now();
    if (now - last < DEDUPE_MS) return;
    sessionStorage.setItem(DEDUPE_KEY, String(now));
    pingVisit().catch(() => { /* best-effort */ });
  }, [location.pathname]);
}
