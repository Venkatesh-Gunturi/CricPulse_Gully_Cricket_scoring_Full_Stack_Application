import { useState } from "react";

import MainPage from "./pages/MainPage";
import LoginModal from "./components/Auth/LoginModal";
import RegisterModal from "./components/Auth/RegisterModal";
import VerifyMobileModal from "./components/Auth/VerifyMobileModal";
import MatchCreation from "./components/Match/MatchCreation";

function App() {
  const [showLoginModal, setShowLoginModal] = useState(false);
  const [showRegisterModal, setShowRegisterModal] = useState(false);
  const [showVerifyMobileModal, setShowVerifyMobileModal] = useState(false);

  const [registeredUserId, setRegisteredUserId] = useState(null);

  const [loggedInUser, setLoggedInUser] = useState(
    JSON.parse(localStorage.getItem("user"))
  );

  return (
    <>
      <MainPage
        onLogin={() => setShowLoginModal(true)}
        onRegister={() => setShowRegisterModal(true)}
      />

      <LoginModal
        show={showLoginModal}
        onClose={() => setShowLoginModal(false)}
        onLoginSuccess={(user) => setLoggedInUser(user)}
      />

      <RegisterModal
        show={showRegisterModal}
        onClose={() => setShowRegisterModal(false)}
        onRegistrationSuccess={(userId) => {
          setRegisteredUserId(userId);
          setShowRegisterModal(false);
          setShowVerifyMobileModal(true);
        }}
      />

      <VerifyMobileModal
        show={showVerifyMobileModal}
        userId={registeredUserId}
        onClose={() => setShowVerifyMobileModal(false)}
      />

      {loggedInUser?.isUmpire && (
        <MatchCreation />
      )}
    </>
  );
}

export default App;