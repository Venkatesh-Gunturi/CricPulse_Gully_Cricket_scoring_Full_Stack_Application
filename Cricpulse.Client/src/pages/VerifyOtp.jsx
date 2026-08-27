import { useState } from "react";
import { useLocation } from "react-router-dom";

function VerifyOtp() {

    const location = useLocation();
    const userId = location.state?.userId;
    console.log("User ID:", userId);



  const [emailOtp, setEmailOtp] = useState("");
  const [mobileOtp, setMobileOtp] = useState("");

  const handleSubmit = (event) => {
    event.preventDefault();

    console.log("Email OTP:", emailOtp);
    console.log("Mobile OTP:", mobileOtp);
  };

  return (
    <div className="container min-vh-100 d-flex align-items-center justify-content-center py-4">

      <div
        className="card shadow-sm p-4"
        style={{ maxWidth: "520px", width: "100%" }}
      >

        <div className="text-center mb-4">
          <h2 className="fw-bold">Verify Your Account</h2>

          <p className="text-muted">
            Enter the OTPs sent to your email and mobile number.
          </p>
        </div>

        <form onSubmit={handleSubmit}>

          {/* Email OTP */}

          <div className="mb-4">
            <label className="form-label fw-semibold">
              Email OTP
            </label>

            <input
              type="text"
              className="form-control text-center"
              value={emailOtp}
              onChange={(event) => setEmailOtp(event.target.value)}
              placeholder="Enter 6-digit email OTP"
              maxLength="6"
            />
          </div>


          {/* Mobile OTP */}

          <div className="mb-4">
            <label className="form-label fw-semibold">
              Mobile OTP
            </label>

            <input
              type="text"
              className="form-control text-center"
              value={mobileOtp}
              onChange={(event) => setMobileOtp(event.target.value)}
              placeholder="Enter 6-digit mobile OTP"
              maxLength="6"
            />
          </div>


          <button
            type="submit"
            className="btn btn-primary w-100"
          >
            Verify Account
          </button>

        </form>

      </div>

    </div>
  );
}

export default VerifyOtp;