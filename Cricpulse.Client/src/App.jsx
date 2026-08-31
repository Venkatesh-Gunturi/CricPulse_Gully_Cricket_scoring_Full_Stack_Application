import { useState } from "react";
import MainPage from "./pages/MainPage";
import RegisterModal from "./components/Auth/RegisterModal";
import VerifyMobileModal from "./components/Auth/VerifyMobileModal";
import LoginModal from "./components/Auth/LoginModal";

function App() {

  const [showLoginModal, setShowLoginModal] = useState(false);
  const [showVerifyMobileModal, setShowVerifyMobileModal] = useState(false);
const [registeredUserId, setRegisteredUserId] = useState(null);

  const [showRegisterModal, setShowRegisterModal] = useState(false);

 const handleRegistered = (userId) => {
  console.log("Registered User ID:", userId);

  setRegisteredUserId(userId);
  setShowRegisterModal(false);
  setShowVerifyMobileModal(true);
};

  return (
    <>
     <MainPage
       onRegister={() => setShowRegisterModal(true)}
       onLogin={() => setShowLoginModal(true)}
     />

      <RegisterModal
        show={showRegisterModal}
        onClose={() => setShowRegisterModal(false)}
        onRegistered={handleRegistered}
      />

      <VerifyMobileModal
        show={showVerifyMobileModal}
        userId={registeredUserId}
        onVerified={() => {
          setShowVerifyMobileModal(false);
          console.log("Registration completed successfully!");
      }}
      />

      <LoginModal
        show={showLoginModal}
        onClose={() => setShowLoginModal(false)}
      />
    </>
  );
}

export default App;