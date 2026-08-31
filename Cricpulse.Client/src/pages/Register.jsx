import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { registerPlayer } from "../services/authService";

function Register() {

    const navigate = useNavigate();

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

    setFormData({
      ...formData,
      [name]: value
    });
  };

 const handleSubmit = async (event) => {
  event.preventDefault();

  const request = {
    firstName: formData.firstName,
    lastName: formData.lastName,
    email: formData.email,
    mobileNumber: formData.mobileNumber,
    password: formData.password
  };

try {
  const response = await registerPlayer(request);

  console.log("Registration successful:", response);

 navigate("/verify-mobile", {
  state: {
    userId: response.id
  }
});

  } catch (error) {
    console.error("Registration failed:", error);
  }
};


  return (
    <div className="container min-vh-100 d-flex align-items-center justify-content-center py-4">

      <div className="card shadow-sm p-4" style={{ maxWidth: "520px", width: "100%" }}>

        <div className="text-center mb-4">
          <h1 className="fw-bold">CricPulse</h1>
          <h4>Create your account</h4>
          <p className="text-muted">
            Join CricPulse and start your cricket journey.
          </p>
        </div>

        <form onSubmit={handleSubmit}>

          <div className="row">

            <div className="col-md-6 mb-3">
              <label className="form-label">First Name</label>

              <input
                type="text"
                name="firstName"
                value={formData.firstName}
                onChange={handleChange}
                className="form-control"
                placeholder="First name"
              />
            </div>

            <div className="col-md-6 mb-3">
              <label className="form-label">Last Name /Surname</label>

              <input
                type="text"
                name="lastName"
                value={formData.lastName}
                onChange={handleChange}
                className="form-control"
                placeholder="Last name"
              />
            </div>

          </div>

          <div className="mb-3">
            <label className="form-label">Email</label>

            <input
              type="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              className="form-control"
              placeholder="Enter your email"
            />
          </div>

          <div className="mb-3">
            <label className="form-label">Mobile Number</label>

            <input
              type="tel"
              name="mobileNumber"
              value={formData.mobileNumber}
              onChange={handleChange}
              className="form-control"
              placeholder="Enter your mobile number"
            />
          </div>

          <div className="mb-3">
            <label className="form-label">Password</label>

            <input
              type="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              className="form-control"
              placeholder="Create a password"
            />
          </div>

          <div className="mb-4">
            <label className="form-label">Confirm Password</label>

            <input
              type="password"
              name="confirmPassword"
              value={formData.confirmPassword}
              onChange={handleChange}
              className="form-control"
              placeholder="Confirm your password"
            />
          </div>

          <button
            type="submit"
            className="btn btn-primary w-100"
          >
            Create Account
          </button>

        </form>

        <div className="text-center mt-4">
          <span className="text-muted">
            Already have an account?{" "}
          </span>

          <a href="#" className="text-decoration-none">
            Login
          </a>
        </div>

      </div>

    </div>
  );
}

export default Register;