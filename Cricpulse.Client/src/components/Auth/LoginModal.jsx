import { useState } from "react";
import { loginPlayer } from "../../services/authService";
import "./LoginModal.css";

function LoginModal({
  show,
  onClose,
  onLoginSuccess,
  onRegister
}) {
  const [loginData, setLoginData] = useState({
    identifier: "",
    password: ""
  });

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  // Purpose:
  // Update login fields and clear previous login errors.
  const handleChange = (event) => {
    const { name, value } = event.target;

    setLoginData((previousData) => ({
      ...previousData,
      [name]: value
    }));

    setError("");
  };

  // Purpose:
  // Authenticate the user and handle successful or failed login attempts.
  const handleSubmit = async (event) => {
    event.preventDefault();

    setError("");

    if (!loginData.identifier.trim()) {
      setError("Please enter your mobile number or email.");
      return;
    }

    if (!loginData.password) {
      setError("Please enter your password.");
      return;
    }

    try {
      setLoading(true);

      const response = await loginPlayer({
        identifier: loginData.identifier.trim(),
        password: loginData.password
      });

      localStorage.setItem("token", response.token);
      localStorage.setItem(
        "user",
        JSON.stringify(response.user)
      );

      onLoginSuccess(response.user);

      setLoginData({
        identifier: "",
        password: ""
      });

      onClose();

    } catch (error) {
      console.error("Login failed:", error);

      setError(
        error?.response?.data?.message ||
        "Unable to login. Please try again."
      );
    } finally {
      setLoading(false);
    }
  };

  // Purpose:
  // Close the login modal and clear temporary login state.
  const handleClose = () => {
    setLoginData({
      identifier: "",
      password: ""
    });

    setError("");
    setLoading(false);

    onClose();
  };

  if (!show) {
    return null;
  }

  return (
    <>
      <div
        className="modal show d-block login-modal"
        tabIndex="-1"
        role="dialog"
        aria-modal="true"
      >
        <div className="modal-dialog modal-dialog-centered">
          <div className="modal-content login-modal-content">

            {/* Header */}
            <div className="modal-header login-modal-header">

              <div className="login-brand">
                <span className="login-brand-icon">
                  🏏
                </span>

                <div>
                  <h5 className="modal-title">
                    Welcome back
                  </h5>

                  <small>
                    Login to continue your CricPulse journey
                  </small>
                </div>
              </div>

              <button
                type="button"
                className="btn-close"
                onClick={handleClose}
                aria-label="Close"
              />

            </div>

            {/* Body */}
            <div className="modal-body login-modal-body">

              <div className="login-welcome">
                <h4>
                  Let's get back to cricket.
                </h4>

                <p>
                  Sign in to follow matches, manage your
                  cricket activity and stay connected.
                </p>
              </div>

              {error && (
                <div
                  className="alert alert-danger login-alert"
                  role="alert"
                >
                  {error}

                  {error.toLowerCase().includes(
                    "not registered"
                  ) && (
                    <button
                      type="button"
                      className="register-inline-button"
                      onClick={() => {
                        handleClose();

                        if (onRegister) {
                          onRegister();
                        }
                      }}
                    >
                      Register now
                    </button>
                  )}
                </div>
              )}

              <form onSubmit={handleSubmit}>

                {/* Identifier */}
                <div className="mb-3">

                  <label className="form-label">
                    Mobile Number / Email
                  </label>

                  <input
                    type="text"
                    className="form-control login-input"
                    name="identifier"
                    value={loginData.identifier}
                    onChange={handleChange}
                    placeholder="Enter mobile number or email"
                    autoComplete="username"
                    disabled={loading}
                    required
                  />

                </div>

                {/* Password */}
                <div className="mb-4">

                  <label className="form-label">
                    Password
                  </label>

                  <input
                    type="password"
                    className="form-control login-input"
                    name="password"
                    value={loginData.password}
                    onChange={handleChange}
                    placeholder="Enter your password"
                    autoComplete="current-password"
                    disabled={loading}
                    required
                  />

                </div>

                {/* Login */}
                <button
                  type="submit"
                  className="btn btn-primary w-100 login-button"
                  disabled={loading}
                >
                  {loading
                    ? "Signing in..."
                    : "Login"}
                </button>

              </form>

              <div className="login-footer">

                <span>
                  Don't have a CricPulse account?
                </span>

                <button
                  type="button"
                  className="register-link"
                  onClick={() => {
                    handleClose();

                    if (onRegister) {
                      onRegister();
                    }
                  }}
                >
                  Create Account
                </button>

              </div>

            </div>
          </div>
        </div>
      </div>

      <div className="modal-backdrop show login-backdrop"></div>
    </>
  );
}

export default LoginModal;