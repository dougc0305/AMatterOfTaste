import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useAuth } from './hooks/useAuth';
import { usePageView } from './hooks/usePageView';
import Nav from './components/layout/Nav';
import Footer from './components/layout/Footer';
import Home from './pages/Home';
import Browse from './pages/Browse';
import RecipeDetail from './pages/RecipeDetail';
import Login from './pages/Login';
import Register from './pages/Register';
import MyCookbook from './pages/MyCookbook';
import Admin from './pages/Admin';
import Account from './pages/Account';

function RouteTracker() {
  usePageView();
  return null;
}

function App() {
  const { user, isLoggedIn, isAdmin, saveAuth, logout } = useAuth();

  return (
    <BrowserRouter>
      <RouteTracker />
      <div className="min-h-screen bg-cream flex flex-col">
        <Nav user={user} onLogout={logout} />
        <main className="flex-1">
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/browse" element={<Browse />} />
            <Route path="/recipe/:id" element={<RecipeDetail isLoggedIn={isLoggedIn} />} />
            <Route path="/login" element={<Login onLogin={saveAuth} />} />
            <Route path="/register" element={<Register onLogin={saveAuth} />} />
            <Route
              path="/my-cookbook"
              element={isLoggedIn ? <MyCookbook /> : <Navigate to="/login" />}
            />
            <Route
              path="/account"
              element={isLoggedIn ? <Account /> : <Navigate to="/login" />}
            />
            <Route
              path="/admin"
              element={isAdmin ? <Admin /> : <Navigate to="/" />}
            />
          </Routes>
        </main>
        <Footer />
      </div>
    </BrowserRouter>
  );
}

export default App;
