import { useEffect, useState } from "react";
import { registerPlayer, verifyOtp } from "../../services/authService";
import "./RegisterModal.css";

function RegisterModal({ show, onClose, onRegistered, onLogin }) {
  const [formData, setFormData] = useState({
    firstName: "",
    lastName: "",
    mobileNumber: "",
    password: "",
    confirmPassword: ""
  });

  const [otp, setOtp] = useState("");
  const [registrationId, setRegistrationId] = useState(null);

  const [otpSent, setOtpSent] = useState(false);
  const [otpVerified, setOtpVerified] = useState(false);

  const [otpLoading, setOtpLoading] = useState(false);
  const [accountLoading, setAccountLoading] = useState(false);

  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  const [otpSecondsLeft, setOtpSecondsLeft] = useState(0);

  // Purpose:
  // Count down the five-minute OTP validity period on the registration screen.
  useEffect(() => {
    if (!otpSent || otpVerified || otpSecondsLeft <= 0) {
      return;
    }

    const timer = setInterval(() => {
      setOtpSecondsLeft((previousSeconds) => {
        if (previousSeconds <= 1) {
          clearInterval(timer);
          return 0;
        }

        return previousSeconds - 1;
      });
    }, 1000);

    return () => clearInterval(timer);
  }, [otpSent, otpVerified, otpSecondsLeft]);

  // Purpose:
  // Update registration form values while clearing previous messages.
  const handleChange = (event) => {
    const { name, value } = event.target;

    setFormData((previousData) => ({
      ...previousData,
      [name]: value
    }));

    setError("");
    setSuccessMessage("");
  };

  // Purpose:
  // Validate basic registration details and request a mobile OTP.
  const handleSendOtp = async () => {
    setError("");
    setSuccessMessage("");

    if (!formData.firstName.trim()) {
      setError("First name is required.");
      return;
    }

    if (formData.firstName.trim().length > 100) {
      setError("First name cannot exceed 100 characters.");
      return;
    }

    if (formData.lastName.trim().length > 100) {
      setError("Last name cannot exceed 100 characters.");
      return;
    }

    if (!/^\d{10}$/.test(formData.mobileNumber)) {
      setError("Mobile number must contain exactly 10 digits.");
      return;
    }

    try {
      setOtpLoading(true);

      const response = await registerPlayer({
        firstName: formData.firstName.trim(),
        lastName: formData.lastName.trim() || null,
        mobileNumber: formData.mobileNumber
      });

      setRegistrationId(response.registrationId);
      setOtpSent(true);
      setOtpVerified(false);
      setOtp("");
      setOtpSecondsLeft(300);

      setSuccessMessage(
        "OTP sent successfully. Please enter the OTP below."
      );
    } catch (error) {
      console.error("OTP request failed:", error);

      setError(
        error?.response?.data?.message ||
        "Unable to send OTP. Please try again."
      );
    } finally {
      setOtpLoading(false);
    }
  };

  // Purpose:
  // Verify the OTP belonging to the temporary registration.
  const handleVerifyOtp = async () => {
    setError("");
    setSuccessMessage("");

    if (!/^\d{6}$/.test(otp)) {
      setError("OTP must contain exactly 6 digits.");
      return;
    }

    try {
      setOtpLoading(true);

      await verifyOtp(registrationId, otp);

      setOtpVerified(true);
      setOtpSecondsLeft(0);

      setSuccessMessage(
        "Mobile number verified successfully. You can now create your password."
      );
    } catch (error) {
      console.error("OTP verification failed:", error);

      setError(
        error?.response?.data?.message ||
        "Invalid or expired OTP."
      );
    } finally {
      setOtpLoading(false);
    }
  };

  // Purpose:
  // Complete registration and create the User and Player after OTP verification.
  const handleCreateAccount = async (event) => {
    event.preventDefault();

    setError("");
    setSuccessMessage("");

    if (!otpVerified) {
      setError("Please verify your mobile number first.");
      return;
    }

    if (formData.password.length < 8) {
      setError("Password must contain at least 8 characters.");
      return;
    }

    if (formData.password !== formData.confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    try {
      setAccountLoading(true);

      const response = await registerPlayer({
        firstName: formData.firstName.trim(),
        lastName: formData.lastName.trim() || null,
        mobileNumber: formData.mobileNumber,
        password: formData.password
      });

      setSuccessMessage("Account created successfully!");

      setTimeout(() => {
        onRegistered(response);
      }, 800);
    } catch (error) {
      console.error("Account creation failed:", error);

      setError(
        error?.response?.data?.message ||
        "Unable to create your account. Please try again."
      );
    } finally {
      setAccountLoading(false);
    }
  };

  // Purpose:
  // Reset all registration state when the modal is closed.
  const handleClose = () => {
    setFormData({
      firstName: "",
      lastName: "",
      mobileNumber: "",
      password: "",
      confirmPassword: ""
    });

    setOtp("");
    setRegistrationId(null);

    setOtpSent(false);
    setOtpVerified(false);
    setOtpSecondsLeft(0);

    setOtpLoading(false);
    setAccountLoading(false);

    setError("");
    setSuccessMessage("");

    onClose();
  };

  // Purpose:
  // Format the remaining OTP validity time as minutes and seconds.
  const formatOtpTime = () => {
    const minutes = Math.floor(otpSecondsLeft / 60);
    const seconds = otpSecondsLeft % 60;

    return `${minutes}:${seconds.toString().padStart(2, "0")}`;
  };

  if (!show) {
    return null;
  }

  return (
    <>
      <div
        className="modal show d-block register-modal"
        tabIndex="-1"
        role="dialog"
        aria-modal="true"
      >
        <div className="modal-dialog modal-dialog-centered modal-dialog-scrollable">
          <div className="modal-content register-modal-content">

            <div className="modal-header register-modal-header">
              <div className="register-brand">
                <span className="register-brand-icon">🏏</span>

                <div>
                  <h5 className="modal-title">
                    Create your account
                  </h5>

                  <small>
                    Join CricPulse and start your cricket journey
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

            <div className="modal-body register-modal-body">

              {error && (
                <div className="alert alert-danger register-alert">
                  {error}
                </div>
              )}

              {successMessage && (
                <div className="alert alert-success register-alert">
                  {successMessage}
                </div>
              )}

              <form onSubmit={handleCreateAccount}>

                {/* Name */}
                <div className="row g-3">

                  <div className="col-sm-6">
                    <label className="form-label">
                      First Name
                    </label>

                    <input
                      type="text"
                      name="firstName"
                      className="form-control register-input"
                      placeholder="First name"
                      value={formData.firstName}
                      onChange={handleChange}
                      maxLength="100"
                      disabled={otpSent}
                      required
                    />
                  </div>

                  <div className="col-sm-6">
                    <label className="form-label">
                      Last Name
                      <span className="optional-label">
                        {" "}Optional
                      </span>
                    </label>

                    <input
                      type="text"
                      name="lastName"
                      className="form-control register-input"
                      placeholder="Last name"
                      value={formData.lastName}
                      onChange={handleChange}
                      maxLength="100"
                      disabled={otpSent}
                    />
                  </div>

                </div>

                {/* Mobile */}
                <div className="mt-3">

                  <label className="form-label">
                    Mobile Number
                  </label>

                  <div className="input-group">

                    <input
                      type="tel"
                      name="mobileNumber"
                      className="form-control register-input"
                      placeholder="10-digit mobile number"
                      value={formData.mobileNumber}
                      onChange={(event) => {
                        const value =
                          event.target.value.replace(/\D/g, "");

                        if (value.length <= 10) {
                          setFormData((previousData) => ({
                            ...previousData,
                            mobileNumber: value
                          }));
                        }

                        setError("");
                        setSuccessMessage("");
                      }}
                      maxLength="10"
                      disabled={otpSent}
                      required
                    />

                    <button
                      type="button"
                      className={`btn verify-button ${
                        otpVerified
                          ? "otp-verified-button"
                          : otpSent && otpSecondsLeft > 0
                            ? "otp-sent-button"
                            : "btn-outline-primary"
                      }`}
                      onClick={handleSendOtp}
                      disabled={
                        otpLoading ||
                        otpVerified ||
                        (otpSent && otpSecondsLeft > 0)
                      }
                    >
                      {otpLoading
                        ? "Sending..."
                        : otpVerified
                          ? "✓ Verified"
                          : !otpSent
                            ? "Send OTP"
                            : otpSecondsLeft > 0
                              ? "✓ OTP Sent"
                              : "Resend OTP"}
                    </button>

                  </div>

                  {otpSent && !otpVerified && otpSecondsLeft > 0 && (
                    <div className="otp-timer">
                      OTP expires in <strong>{formatOtpTime()}</strong>
                    </div>
                  )}

                </div>

                {/* OTP */}
                {otpSent && !otpVerified && (
                  <div className="otp-box mt-3">

                    <label className="form-label">
                      Enter OTP
                    </label>

                    <div className="input-group">

                      <input
                        type="text"
                        className="form-control register-input otp-input"
                        placeholder="6-digit OTP"
                        value={otp}
                        onChange={(event) => {
                          const value =
                            event.target.value.replace(/\D/g, "");

                          if (value.length <= 6) {
                            setOtp(value);
                          }

                          setError("");
                        }}
                        maxLength="6"
                        autoComplete="one-time-code"
                      />

                      <button
                        type="button"
                        className="btn btn-primary verify-button"
                        onClick={handleVerifyOtp}
                        disabled={
                          otp.length !== 6 ||
                          otpLoading ||
                          otpSecondsLeft === 0
                        }
                      >
                        {otpLoading ? "Verifying..." : "Verify"}
                      </button>

                    </div>

                    {otpSecondsLeft === 0 && (
                      <div className="otp-expired">
                        OTP expired. Click <strong>Resend OTP</strong> below.
                      </div>
                    )}

                  </div>
                )}

                {/* Password */}
                <div className="row g-3 mt-1">

                  <div className="col-sm-6">
                    <label className="form-label">
                      Password
                    </label>

                    <input
                      type="password"
                      name="password"
                      className="form-control register-input"
                      placeholder="Minimum 8 characters"
                      value={formData.password}
                      onChange={handleChange}
                      minLength="8"
                      disabled={!otpVerified}
                      required
                    />
                  </div>

                  <div className="col-sm-6">
                    <label className="form-label">
                      Confirm Password
                    </label>

                    <input
                      type="password"
                      name="confirmPassword"
                      className="form-control register-input"
                      placeholder="Re-enter password"
                      value={formData.confirmPassword}
                      onChange={handleChange}
                      minLength="8"
                      disabled={!otpVerified}
                      required
                    />
                  </div>

                </div>

                {/* Create Account */}
                <button
                  type="submit"
                  className="btn btn-primary w-100 create-account-button"
                  disabled={
                    !otpVerified ||
                    accountLoading
                  }
                >
                  {accountLoading
                    ? "Creating Account..."
                    : "Create Account"}
                </button>

              </form>

              <div className="register-footer">
                <span>
                  Already have an account?
                </span>

                <button
                  type="button"
                  className="login-link"
                  onClick={() => {
                    handleClose();

                    if (onLogin) {
                      onLogin();
                    }
                  }}
                >
                  Login
                </button>
              </div>

            </div>
          </div>
        </div>
      </div>

      <div className="modal-backdrop show register-backdrop"></div>
    </>
  );
}

export default RegisterModal;