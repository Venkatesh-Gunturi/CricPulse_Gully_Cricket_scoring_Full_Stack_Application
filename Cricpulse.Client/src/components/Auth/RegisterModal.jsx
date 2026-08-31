import { useState } from "react";
import { registerPlayer } from "../../services/authService";

function RegisterModal({ show, onClose, onRegistered }) {
  const [formData, setFormData] = useState({
    firstName: "",
    lastName: "",
    email: "",
    mobileNumber: "",
    password: "",
    confirmPassword: ""
  });

  const handleChange = (event) => {
    const { name, value } = event.target;

    setFormData((previousData) => ({
      ...previousData,
      [name]: value
    }));
  };

 const handleSubmit = async (event) => {
  event.preventDefault();

  console.log("Create Account clicked");

  if (formData.password !== formData.confirmPassword) {
    console.log("Passwords do not match.");
    return;
  }

  const request = {
    firstName: formData.firstName,
    lastName: formData.lastName,
    email: formData.email,
    mobileNumber: formData.mobileNumber,
    password: formData.password
  };

  console.log("Registration request:", request);

  try {
    const response = await registerPlayer(request);

    console.log("Registration successful:", response);

    onRegistered(response.id);

  } catch (error) {
    console.error("Registration failed:", error);
  }
};

  if (!show) {
    return null;
  }

  return (
    <>
      <div
        className="modal show d-block"
        tabIndex="-1"
        role="dialog"
        aria-modal="true"
      >
        <div className="modal-dialog modal-dialog-centered">
          <div className="modal-content">

            <div className="modal-header">
              <h5 className="modal-title">
                Create CricPulse Account
              </h5>

              <button
                type="button"
                className="btn-close"
                onClick={onClose}
                aria-label="Close"
              ></button>
            </div>

            <div className="modal-body">

              <form onSubmit={handleSubmit}>

                <div className="mb-3">
                  <label className="form-label">
                    First Name
                  </label>

                  <input
                    type="text"
                    className="form-control"
                    name="firstName"
                    value={formData.firstName}
                    onChange={handleChange}
                    required
                  />
                </div>

                <div className="mb-3">
                  <label className="form-label">
                    Last Name
                  </label>

                  <input
                    type="text"
                    className="form-control"
                    name="lastName"
                    value={formData.lastName}
                    onChange={handleChange}
                  />
                </div>

                <div className="mb-3">
                  <label className="form-label">
                    Email
                  </label>

                  <input
                    type="email"
                    className="form-control"
                    name="email"
                    value={formData.email}
                    onChange={handleChange}
                    required
                  />
                </div>

                <div className="mb-3">
                  <label className="form-label">
                    Mobile Number
                  </label>

                  <input
                    type="text"
                    className="form-control"
                    name="mobileNumber"
                    value={formData.mobileNumber}
                    onChange={handleChange}
                    required
                  />
                </div>

                <div className="mb-3">
                  <label className="form-label">
                    Password
                  </label>

                  <input
                    type="password"
                    className="form-control"
                    name="password"
                    value={formData.password}
                    onChange={handleChange}
                    minLength="8"
                    required
                  />
                </div>

                <div className="mb-3">
                  <label className="form-label">
                    Confirm Password
                  </label>

                  <input
                    type="password"
                    className="form-control"
                    name="confirmPassword"
                    value={formData.confirmPassword}
                    onChange={handleChange}
                    minLength="8"
                    required
                  />
                </div>

                <button
                  type="submit"
                  className="btn btn-primary w-100"
                >
                  Create Account
                </button>

              </form>

            </div>

          </div>
        </div>
      </div>

      <div className="modal-backdrop show"></div>
    </>
  );
}

export default RegisterModal;