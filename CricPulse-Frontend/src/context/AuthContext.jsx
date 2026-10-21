
import { createContext, useContext, useState } from 'react';

const AuthContext = createContext(null);

const USER_STORAGE_KEY = 'user';
const ROLE_STORAGE_KEY = 'role';

function readStoredValue(key) {
  try {
    const persistentValue = localStorage.getItem(key);

    if (persistentValue !== null) {
      return JSON.parse(persistentValue);
    }

    const sessionValue = sessionStorage.getItem(key);

    return sessionValue !== null ? JSON.parse(sessionValue) : null;
  } catch (error) {
    console.error('Unable to restore stored session:', error);
    return null;
  }
}

function saveStoredValue(key, value, rememberMe) {
  try {
    // Prevent stale data in the other storage.
    localStorage.removeItem(key);
    sessionStorage.removeItem(key);

    if (value === null || value === undefined) {
      return;
    }

    const storage = rememberMe ? localStorage : sessionStorage;

    storage.setItem(key, JSON.stringify(value));
  } catch (error) {
    console.error('Unable to save session data:', error);
  }
}

export function AuthProvider({ children }) {
  const [user, setUserState] = useState(() =>
    readStoredValue(USER_STORAGE_KEY)
  );

  const [role, setRoleState] = useState(
    () => readStoredValue(ROLE_STORAGE_KEY) || 'Player'
  );

  const [rememberMe, setRememberMeState] = useState(() => {
    try {
      return localStorage.getItem(USER_STORAGE_KEY) !== null;
    } catch {
      return false;
    }
  });

  const setUser = (nextUser, shouldRemember = rememberMe) => {
    const resolvedUser =
      typeof nextUser === 'function' ? nextUser(user) : nextUser;

    setUserState(resolvedUser);
    setRememberMeState(Boolean(shouldRemember));

    saveStoredValue(USER_STORAGE_KEY, resolvedUser, shouldRemember);

    // Keep the role in the same storage as the user.
    saveStoredValue(
      ROLE_STORAGE_KEY,
      resolvedUser ? role : null,
      shouldRemember
    );
  };

  const setRole = (nextRole, shouldRemember = rememberMe) => {
    const resolvedRole =
      typeof nextRole === 'function' ? nextRole(role) : nextRole;

    setRoleState(resolvedRole);
    setRememberMeState(Boolean(shouldRemember));

    saveStoredValue(ROLE_STORAGE_KEY, resolvedRole, shouldRemember);
  };

  const logout = () => {
    setUserState(null);
    setRoleState('Player');
    setRememberMeState(false);

    try {
      localStorage.removeItem(USER_STORAGE_KEY);
      localStorage.removeItem(ROLE_STORAGE_KEY);
      sessionStorage.removeItem(USER_STORAGE_KEY);
      sessionStorage.removeItem(ROLE_STORAGE_KEY);
    } catch (error) {
      console.error('Unable to clear stored session:', error);
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        setUser,
        role,
        setRole,
        logout,
        rememberMe,
        authReady: true,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }

  return context;
}
