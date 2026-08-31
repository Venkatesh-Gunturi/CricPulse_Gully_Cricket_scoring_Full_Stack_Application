import { useState } from "react";
import { loginPlayer } from "../../services/authService";

function LoginModal({ show, onClose }) {
  const [loginData, setLoginData] = useState({identifier: "", password: ""});
  
  const handleChange = (event) => {
    const { name, value } = event.target;

    setLoginData((previousData) => ({
      ...previousData,
      [name]: value
    }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();

    console.log("Login clicked");

 try {
 const response = await loginPlayer(loginData);

 console.log("Login successful:", response);

 onClose();

 } catch (error) {
    console.error("Login failed:", error);
 }
 };

 if(!show){
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
                Login to CricPulse
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
                    Mobile Number / Email
                  </label>

                  <input
                        type="text"
                        className="form-control"
                        name="identifier"
                        value={loginData.identifier}
                        onChange={handleChange}
                        placeholder="Enter email or mobile number"
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
                    value={loginData.password}
                    onChange={handleChange}
                    placeholder="Enter your password"
                    required
                  />
                </div>

                <button
                  type="submit"
                  className="btn btn-primary w-100"
                >
                  Login
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

export default LoginModal;