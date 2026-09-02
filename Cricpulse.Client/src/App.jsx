import { useState } from "react";
import MainPage from "./pages/MainPage";
import RegisterModal from "./components/Auth/RegisterModal";
import VerifyMobileModal from "./components/Auth/VerifyMobileModal";
import LoginModal from "./components/Auth/LoginModal";
import PlayerProfile from "./components/Player/PlayerProfile";

function App() {
  return (
    <>
      <PlayerProfile />
    </>
  );
}

export default App;

