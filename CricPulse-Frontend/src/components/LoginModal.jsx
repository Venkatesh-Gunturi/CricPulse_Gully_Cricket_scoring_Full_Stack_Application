
import { useState } from 'react';
import { FaEye, FaEyeSlash, FaTimes, FaArrowLeft } from 'react-icons/fa';

import apiClient from '../services/apiClient';
import '../styles/auth-modal.css';

const initialLoginData = {
  emailOrMobile: '',
  password: '',
};

const initialForgotPasswordData = {
  mobileNumber: '',
  email: '',
  newPassword: '',
  confirmPassword: '',
  
};

function LoginModal({
  show,
  onClose,
  onSwitchToRegister,
  onLoginSuccess,
  loginMode = 'Player',
}) {
  const [mode, setMode] = useState('login');

  const [loginData, setLoginData] = useState(initialLoginData);
  const [forgotData, setForgotData] = useState(initialForgotPasswordData);

  const [showLoginPassword, setShowLoginPassword] = useState(false);
  const [showNewPassword, setShowNewPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  const [errors, setErrors] = useState({});
  const [apiError, setApiError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
const [rememberMe, setRememberMe] = useState(false);

  if (!show) return null;

  const clearMessages = () => {
    setErrors({});
    setApiError('');
    setSuccessMessage('');
  };

  const resetToLogin = () => {
    setMode('login');
    setForgotData(initialForgotPasswordData);
    setShowNewPassword(false);
    setShowConfirmPassword(false);
    clearMessages();
  };

  const handleClose = () => {
    resetToLogin();
    setLoginData(initialLoginData);
    setShowLoginPassword(false);
    onClose();
  };

  const openForgotPassword = () => {
    setMode('forgot');
    setForgotData(initialForgotPasswordData);
    clearMessages();
  };

  const handleLoginChange = (event) => {
    const { name, value } = event.target;

    setLoginData((previous) => ({
      ...previous,
      [name]: value,
    }));

    setErrors((previous) => ({
      ...previous,
      [name]: '',
    }));

    setApiError('');
  };

  const handleForgotChange = (event) => {
    const { name, value } = event.target;

    setForgotData((previous) => ({
      ...previous,
      [name]: value,
    }));

    setErrors((previous) => ({
      ...previous,
      [name]: '',
      ...(name === 'newPassword' || name === 'confirmPassword'
        ? { confirmPassword: '' }
        : {}),
    }));

    setApiError('');
    setSuccessMessage('');
  };

 
const handleLoginSubmit = async (event) => {
  event.preventDefault();
  clearMessages();

  const nextErrors = {};
  const loginIdentifier = loginData.emailOrMobile.trim();

  if (!loginIdentifier) {
    nextErrors.emailOrMobile =
      'Enter your email or mobile number.';
  }

  if (!loginData.password) {
    nextErrors.password = 'Enter your password.';
  }

  setErrors(nextErrors);

  if (Object.keys(nextErrors).length > 0) return;

  setIsSubmitting(true);

  try {
    const response = await apiClient.post('/Auth/login', {
      LoginIdentifier: loginIdentifier,
      Password: loginData.password,
    });

    const user = response.data;

    if (!user?.userId) {
      setApiError(
        'Login succeeded, but the server returned an unexpected response.'
      );
      return;
    }

    // Pass the returned account details to the parent component.
    onLoginSuccess?.(user, loginMode, rememberMe);

    // Close the modal after successful authentication.
    handleClose();
  } catch (error) {
    const message = error.response?.data?.message;

    if (message) {
      setApiError(message);
    } else if (error.response?.status === 404) {
      setApiError(
        'Login endpoint not found. Please check the API configuration.'
      );
    } else if (error.response) {
      setApiError(
        'Login failed. Please check your credentials and try again.'
      );
    } else {
      setApiError(
        'Unable to connect to the server. Please try again.'
      );
    }
  } finally {
    setIsSubmitting(false);
  }
};


  const handleForgotPasswordSubmit = async (event) => {
    event.preventDefault();

    clearMessages();

    const nextErrors = {};
    const mobile = forgotData.mobileNumber.trim();
    const email = forgotData.email.trim();

    if (!mobile) {
      nextErrors.mobileNumber = 'Enter your registered mobile number.';
    } else if (!/^\d{10}$/.test(mobile)) {
      nextErrors.mobileNumber = 'Enter a valid 10-digit mobile number.';
    }

    if (!email) {
      nextErrors.email = 'Enter your registered email address.';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      nextErrors.email = 'Enter a valid email address.';
    }

    if (!forgotData.newPassword) {
      nextErrors.newPassword = 'Enter your new password.';
    } else if (forgotData.newPassword.length < 8) {
      nextErrors.newPassword = 'Password must contain at least 8 characters.';
    }

    if (!forgotData.confirmPassword) {
      nextErrors.confirmPassword = 'Confirm your new password.';
    } else if (forgotData.newPassword !== forgotData.confirmPassword) {
      nextErrors.confirmPassword = 'Passwords do not match.';
    }

    setErrors(nextErrors);

    if (Object.keys(nextErrors).length > 0) return;

    setIsSubmitting(true);

    try {
      const response = await apiClient.post('/Auth/forgot-password', {
        MobileNumber: mobile,
        Email: email,
        NewPassword: forgotData.newPassword,
        ConfirmPassword: forgotData.confirmPassword,
      });

      setSuccessMessage(
        response.data?.message || 'Password reset successful.',
      );

      setForgotData(initialForgotPasswordData);
      setErrors({});
    } catch (error) {
      const message = error.response?.data?.message;

      if (message) {
        setApiError(message);
      } else if (error.response?.status === 404) {
        setApiError(
          'Password reset endpoint was not found. Please check the API configuration.',
        );
      } else {
        setApiError(
          'Unable to reset your password right now. Please try again.',
        );
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div
      className="cp-modal-backdrop"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) handleClose();
      }}
    >
      <section
        className={`cp-modal ${mode === 'forgot' ? 'cp-forgot-modal' : ''}`}
        role="dialog"
        aria-modal="true"
        aria-labelledby="auth-modal-title"
      >
        <button
          type="button"
          className="cp-modal-close"
          onClick={handleClose}
          aria-label="Close modal"
        >
          <FaTimes />
        </button>

        {mode === 'login' ? (
          <>
            <div className="cp-modal-heading">
              <span className="cp-modal-eyebrow">WELCOME BACK</span>
              <h2 id="auth-modal-title">Login to CricPulse</h2>
              <p>Your next cricket moment starts here.</p>
            </div>

            <form onSubmit={handleLoginSubmit} noValidate>
              <div className="mb-3">
                <label htmlFor="login-email-mobile" className="form-label">
                  Email or mobile number
                </label>

                <input
                  id="login-email-mobile"
                  type="text"
                  name="emailOrMobile"
                  className={`form-control ${
                    errors.emailOrMobile ? 'is-invalid' : ''
                  }`}
                  placeholder="Enter email or mobile number"
                  value={loginData.emailOrMobile}
                  onChange={handleLoginChange}
                  autoComplete="username"
                  autoFocus
                />

                {errors.emailOrMobile && (
                  <div className="invalid-feedback">
                    {errors.emailOrMobile}
                  </div>
                )}
              </div>

              <div className="mb-3">
                <label htmlFor="login-password" className="form-label">
                  Password
                </label>

                <div className="cp-password-field">
                  <input
                    id="login-password"
                    type={showLoginPassword ? 'text' : 'password'}
                    name="password"
                    className={`form-control ${
                      errors.password ? 'is-invalid' : ''
                    }`}
                    placeholder="Enter your password"
                    value={loginData.password}
                    onChange={handleLoginChange}
                    autoComplete="current-password"
                  />

                  <button
                    type="button"
                    className="cp-password-toggle"
                    onClick={() =>
                      setShowLoginPassword((visible) => !visible)
                    }
                    aria-label={
                      showLoginPassword ? 'Hide password' : 'Show password'
                    }
                  >
                    {showLoginPassword ? <FaEyeSlash /> : <FaEye />}
                  </button>

                  {errors.password && (
                    <div className="invalid-feedback">
                      {errors.password}
                    </div>
                  )}
                </div>
              </div>

              <div className="cp-login-options">
                <label className="cp-remember-label">
                  <input
                    type="checkbox"
                    name="rememberMe"
                    checked={rememberMe}
                    onChange={(event) => setRememberMe(event.target.checked)}
                  />                  <span>Remember me</span>
                </label>

                <button
                  type="button"
                  className="cp-text-button"
                  onClick={openForgotPassword}
                >
                  Forgot password?
                </button>
              </div>

              {apiError && (
                <div className="alert alert-danger py-2" role="alert">
                  {apiError}
                </div>
              )}

              <button
  type="submit"
  className="btn btn-success cp-submit-button"
  disabled={isSubmitting}
>
  {isSubmitting ? 'Logging in...' : 'Login'}
</button>
            </form>

            <div className="cp-modal-switch">
              Don't have an account?{' '}
              <button
                type="button"
                className="cp-text-button"
                onClick={onSwitchToRegister}
              >
                Register
              </button>
            </div>
          </>
        ) : (
          <>
            <button
              type="button"
              className="cp-forgot-back"
              onClick={resetToLogin}
              disabled={isSubmitting}
            >
              <FaArrowLeft />
              <span>Back to Login</span>
            </button>

            <div className="cp-modal-heading">
              <span className="cp-modal-eyebrow">ACCOUNT RECOVERY</span>
              <h2 id="auth-modal-title">Forgot password?</h2>
              <p>
                Enter your registered mobile number and email, then choose a
                new password.
              </p>
            </div>

            {successMessage ? (
              <div className="cp-reset-success" role="status">
                <div className="alert alert-success mb-3">
                  {successMessage}
                </div>

                <button
                  type="button"
                  className="btn btn-success cp-submit-button"
                  onClick={resetToLogin}
                >
                  Return to Login
                </button>
              </div>
            ) : (
              <form onSubmit={handleForgotPasswordSubmit} noValidate>
                <div className="mb-3">
                  <label htmlFor="forgot-mobile" className="form-label">
                    Registered mobile number
                  </label>

                  <input
                    id="forgot-mobile"
                    type="tel"
                    name="mobileNumber"
                    inputMode="numeric"
                    maxLength={10}
                    autoComplete="tel"
                    placeholder="Enter your 10-digit mobile number"
                    className={`form-control ${
                      errors.mobileNumber ? 'is-invalid' : ''
                    }`}
                    value={forgotData.mobileNumber}
                    onChange={handleForgotChange}
                    autoFocus
                  />

                  {errors.mobileNumber && (
                    <div className="invalid-feedback">
                      {errors.mobileNumber}
                    </div>
                  )}
                </div>

                <div className="mb-3">
                  <label htmlFor="forgot-email" className="form-label">
                    Registered email address
                  </label>

                  <input
                    id="forgot-email"
                    type="email"
                    name="email"
                    autoComplete="email"
                    placeholder="Enter your registered email"
                    className={`form-control ${
                      errors.email ? 'is-invalid' : ''
                    }`}
                    value={forgotData.email}
                    onChange={handleForgotChange}
                  />

                  {errors.email && (
                    <div className="invalid-feedback">{errors.email}</div>
                  )}
                </div>

                <div className="mb-3">
                  <label htmlFor="forgot-new-password" className="form-label">
                    New password
                  </label>

                  <div className="cp-password-field">
                    <input
                      id="forgot-new-password"
                      type={showNewPassword ? 'text' : 'password'}
                      name="newPassword"
                      autoComplete="new-password"
                      placeholder="At least 8 characters"
                      className={`form-control ${
                        errors.newPassword ? 'is-invalid' : ''
                      }`}
                      value={forgotData.newPassword}
                      onChange={handleForgotChange}
                    />

                    <button
                      type="button"
                      className="cp-password-toggle"
                      onClick={() =>
                        setShowNewPassword((visible) => !visible)
                      }
                      aria-label={
                        showNewPassword ? 'Hide password' : 'Show password'
                      }
                    >
                      {showNewPassword ? <FaEyeSlash /> : <FaEye />}
                    </button>

                    {errors.newPassword && (
                      <div className="invalid-feedback">
                        {errors.newPassword}
                      </div>
                    )}
                  </div>
                </div>

                <div className="mb-3">
                  <label
                    htmlFor="forgot-confirm-password"
                    className="form-label"
                  >
                    Confirm new password
                  </label>

                  <div className="cp-password-field">
                    <input
                      id="forgot-confirm-password"
                      type={showConfirmPassword ? 'text' : 'password'}
                      name="confirmPassword"
                      autoComplete="new-password"
                      placeholder="Re-enter your new password"
                      className={`form-control ${
                        errors.confirmPassword ? 'is-invalid' : ''
                      }`}
                      value={forgotData.confirmPassword}
                      onChange={handleForgotChange}
                    />

                    <button
                      type="button"
                      className="cp-password-toggle"
                      onClick={() =>
                        setShowConfirmPassword((visible) => !visible)
                      }
                      aria-label={
                        showConfirmPassword
                          ? 'Hide confirm password'
                          : 'Show confirm password'
                      }
                    >
                      {showConfirmPassword ? <FaEyeSlash /> : <FaEye />}
                    </button>

                    {errors.confirmPassword && (
                      <div className="invalid-feedback">
                        {errors.confirmPassword}
                      </div>
                    )}
                  </div>
                </div>

                {apiError && (
                  <div className="alert alert-danger py-2" role="alert">
                    {apiError}
                  </div>
                )}

                <button
                  type="submit"
                  className="btn btn-success cp-submit-button"
                  disabled={isSubmitting}
                >
                  {isSubmitting ? 'Resetting password...' : 'Reset Password'}
                </button>
              </form>
            )}
          </>
        )}
      </section>
    </div>
  );
}

export default LoginModal;
