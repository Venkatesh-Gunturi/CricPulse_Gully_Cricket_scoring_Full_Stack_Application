import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import MainLayout from './layouts/MainLayout';
import Home from './pages/Home';
import Matches from './pages/Matches';
import About from './pages/About';
import CricketIQ from './pages/CricketIQ';
import UmpireDashboard from './pages/UmpireDashboard';
import MatchCreation from './pages/MatchCreation';
import MatchReview from './pages/MatchReview';
import TossPage from './pages/TossPage';
import InningsSetup from './pages/InningsSetup';
import LiveScoring from './pages/LiveScoring';
import MatchResult from './pages/MatchResult';
import PlayerProfile from './pages/PlayerProfile';

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route element={<MainLayout />}>
            <Route path="/" element={<Home />} />
            <Route path="/matches" element={<Matches />} />
            <Route path="/about" element={<About />} />
            <Route path="/cricket-iq" element={<CricketIQ />} />
          </Route>

          <Route path="/umpire" element={<UmpireDashboard />} />
          <Route
            path="/umpire/create-match"
            element={<MatchCreation mode="create" />}
          />
          <Route
            path="/umpire/matches/:matchId"
            element={<MatchReview />}
          />
          <Route
            path="/umpire/matches/:matchId/edit"
            element={<MatchCreation mode="edit" />}
          />
          <Route
            path="/umpire/matches/:matchId/toss"
            element={<TossPage />}
          />

          <Route
            path="/umpire/matches/:matchId/innings-setup"
            element={<InningsSetup />}
          />

          <Route
            path="/umpire/matches/:matchId/live-scoring"
            element={<LiveScoring />}
          />
          
          <Route
            path="/umpire/matches/:matchId/result"
            element={<MatchResult />}
          />

          <Route path="/player/profile" element={<PlayerProfile />}/>
  
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
