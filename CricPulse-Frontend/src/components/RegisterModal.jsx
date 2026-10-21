
import { useState } from 'react';
import { FaEye, FaEyeSlash, FaTimes } from 'react-icons/fa';

import apiClient from '../services/apiClient';
import '../styles/auth-modal.css';

const initialFormData = {
  firstName: '',
  lastName: '',
  email: '',
  mobileNumber: '',
  password: '',
  confirmPassword: '',
};

function getApiError(error) {
  const data = error.response?.data;

  if (typeof data?.message === 'string') return data.message;
  if (typeof data?.title === 'string') return data.title;

  if (data?.errors) {
    const messages = Object.values(data.errors).flat();
    if (messages.length) return messages.join(' ');
  }

  if (!error.response) {
    return 'Unable to reach the server. Check that the backend is running and the API URL is correct.';
  }

  return 'Registration failed. Please try again.';
}

function RegisterModal({ show, onClose, onSwitchToLogin }) {
  const [formData, setFormData] = useState(initialFormData);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [errors, setErrors] = useState({});
  const [apiError, setApiError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [success, setSuccess] = useState(false);

  if (!show) return null;

  const handleChange = (event) => {
    const { name, value } = event.target;

    setFormData((previous) => ({ ...previous, [name]: value }));
    setErrors((previous) => ({ ...previous, [name]: '' }));
    setApiError('');
  };

  const handleClose = () => {
    if (isSubmitting) return;

    setFormData(initialFormData);
    setErrors({});
    setApiError('');
    setSuccess(false);
    onClose();
  };

  const handleSubmit = async (event) => {
    event.preventDefault();

    const nextErrors = {};
    const email = formData.email.trim();
    const mobile = formData.mobileNumber.trim();

    if (!formData.firstName.trim()) {
      nextErrors.firstName = 'Enter your first name.';
    }

    // Last name is optional.

    if (!email) {
      nextErrors.email = 'Enter your email address.';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      nextErrors.email = 'Enter a valid email address.';
    }

    if (!mobile) {
      nextErrors.mobileNumber = 'Enter your mobile number.';
    } else if (!/^\d{10}$/.test(mobile)) {
      nextErrors.mobileNumber = 'Enter a valid 10-digit mobile number.';
    }

    if (!formData.password) {
      nextErrors.password = 'Create a password.';
    } else if (formData.password.length < 8) {
      nextErrors.password = 'Password must contain at least 8 characters.';
    }

    if (!formData.confirmPassword) {
      nextErrors.confirmPassword = 'Confirm your password.';
    } else if (formData.password !== formData.confirmPassword) {
      nextErrors.confirmPassword = 'Passwords do not match.';
    }

    setErrors(nextErrors);
    setApiError('');

    if (Object.keys(nextErrors).length) return;

    setIsSubmitting(true);

    try {
      await apiClient.post('/Auth/register', {
        FirstName: formData.firstName.trim(),
        LastName: formData.lastName.trim() || null,
        MobileNumber: mobile,
        Email: email,
        Password: formData.password,
        ConfirmPassword: formData.confirmPassword,
      });

      setSuccess(true);
      setFormData(initialFormData);
      setErrors({});
    } catch (error) {
      setApiError(getApiError(error));
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
        className="cp-modal cp-register-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="register-modal-title"
      >
        <button
          type="button"
          className="cp-modal-close"
          onClick={handleClose}
          disabled={isSubmitting}
          aria-label="Close registration modal"
        >
          <FaTimes />
        </button>

        {success ? (
          <>
            <div className="cp-modal-heading">
              <span className="cp-modal-eyebrow">WELCOME TO CRICPULSE</span>
              <h2 id="register-modal-title">Registration successful!</h2>
              <p>Your account has been created. You can now try logging in.</p>
            </div>

            <button
              type="button"
              className="btn btn-success cp-submit-button"
              onClick={() => {
                setSuccess(false);
                onSwitchToLogin();
              }}
            >
              Continue to Login
            </button>
          </>
        ) : (
          <>
            <div className="cp-modal-heading">
              <span className="cp-modal-eyebrow">JOIN THE COMMUNITY</span>
              <h2 id="register-modal-title">Create your account</h2>
              <p>Get ready to make every cricket moment count.</p>
            </div>

            <form onSubmit={handleSubmit} noValidate>
              <div className="row g-3">
                <div className="col-6">
                  <label htmlFor="register-first-name" className="form-label">
                    First name
                  </label>
                  <input
                    id="register-first-name"
                    name="firstName"
                    className={`form-control ${errors.firstName ? 'is-invalid' : ''}`}
                    placeholder="First name"
                    value={formData.firstName}
                    onChange={handleChange}
                    autoComplete="given-name"
                    autoFocus
                  />
                  {errors.firstName && (
                    <div className="invalid-feedback">{errors.firstName}</div>
                  )}
                </div>

                <div className="col-6">
                  <label htmlFor="register-last-name" className="form-label">
                    Last name <span className="text-secondary">(optional)</span>
                  </label>
                  <input
                    id="register-last-name"
                    name="lastName"
                    className="form-control"
                    placeholder="Last name"
                    value={formData.lastName}
                    onChange={handleChange}
                    autoComplete="family-name"
                  />
                </div>

                <div className="col-12">
                  <label htmlFor="register-email" className="form-label">
                    Email address
                  </label>
                  <input
                    id="register-email"
                    type="email"
                    name="email"
                    className={`form-control ${errors.email ? 'is-invalid' : ''}`}
                    placeholder="you@example.com"
                    value={formData.email}
                    onChange={handleChange}
                    autoComplete="email"
                  />
                  {errors.email && (
                    <div className="invalid-feedback">{errors.email}</div>
                  )}
                </div>

                <div className="col-12">
                  <label htmlFor="register-mobile" className="form-label">
                    Mobile number
                  </label>
                  <input
                    id="register-mobile"
                    type="tel"
                    name="mobileNumber"
                    inputMode="numeric"
                    maxLength={10}
                    className={`form-control ${errors.mobileNumber ? 'is-invalid' : ''}`}
                    placeholder="10-digit mobile number"
                    value={formData.mobileNumber}
                    onChange={handleChange}
                    autoComplete="tel"
                  />
                  {errors.mobileNumber && (
                    <div className="invalid-feedback">{errors.mobileNumber}</div>
                  )}
                </div>

                <div className="col-12">
                  <label htmlFor="register-password" className="form-label">
                    Password
                  </label>
                  <div className="cp-password-field">
                    <input
                      id="register-password"
                      type={showPassword ? 'text' : 'password'}
                      name="password"
                      className={`form-control ${errors.password ? 'is-invalid' : ''}`}
                      placeholder="At least 8 characters"
                      value={formData.password}
                      onChange={handleChange}
                      autoComplete="new-password"
                    />
                    <button
                      type="button"
                      className="cp-password-toggle"
                      onClick={() => setShowPassword((value) => !value)}
                      aria-label={showPassword ? 'Hide password' : 'Show password'}
                    >
                      {showPassword ? <FaEyeSlash /> : <FaEye />}
                    </button>
                    {errors.password && (
                      <div className="invalid-feedback">{errors.password}</div>
                    )}
                  </div>
                </div>

                <div className="col-12">
                  <label htmlFor="register-confirm-password" className="form-label">
                    Confirm password
                  </label>
                  <div className="cp-password-field">
                    <input
                      id="register-confirm-password"
                      type={showConfirmPassword ? 'text' : 'password'}
                      name="confirmPassword"
                      className={`form-control ${errors.confirmPassword ? 'is-invalid' : ''}`}
                      placeholder="Re-enter your password"
                      value={formData.confirmPassword}
                      onChange={handleChange}
                      autoComplete="new-password"
                    />
                    <button
                      type="button"
                      className="cp-password-toggle"
                      onClick={() => setShowConfirmPassword((value) => !value)}
                      aria-label={
                        showConfirmPassword ? 'Hide password' : 'Show password'
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
              </div>

              {apiError && (
                <div className="alert alert-danger py-2 mt-3" role="alert">
                  {apiError}
                </div>
              )}

              <button
                type="submit"
                className="btn btn-success cp-submit-button mt-4"
                disabled={isSubmitting}
              >
                {isSubmitting ? 'Creating account...' : 'Create account'}
              </button>
            </form>

            <div className="cp-modal-switch">
              Already have an account?{' '}
              <button
                type="button"
                className="cp-text-button"
                onClick={onSwitchToLogin}
                disabled={isSubmitting}
              >
                Login
              </button>
            </div>
          </>
        )}
      </section>
    </div>
  );
}

export default RegisterModal;
