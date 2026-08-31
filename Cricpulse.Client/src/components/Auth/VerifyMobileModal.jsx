import { useState } from "react";
import { verifyOtp } from "../../services/authService";

function VerifyMobileModal({ show, userId, onVerified }) {
  const [mobileOtp, setMobileOtp] = useState("");

  const handleSubmit = async (event) => {
    
    event.preventDefault();

    try {
      await verifyOtp(userId, mobileOtp, 2);

      console.log("Mobile OTP verified successfully!");

      onVerified();

    } catch (error) {
      console.error("Mobile OTP verification failed:", error);
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
                Verify Your Mobile
              </h5>
            </div>

            <div className="modal-body">

              <p className="text-muted">
                We've sent a 6-digit OTP to your mobile number.
              </p>

              <form onSubmit={handleSubmit}>

                <div className="mb-3">
                  <label className="form-label fw-semibold">
                    Mobile OTP
                  </label>

                  <input
                    type="text"
                    className="form-control text-center"
                    value={mobileOtp}
                    onChange={(event) =>
                      setMobileOtp(event.target.value)
                    }
                    placeholder="Enter 6-digit OTP"
                    maxLength="6"
                    required
                  />
                </div>

                <button
                  type="submit"
                  className="btn btn-primary w-100"
                >
                  Verify Mobile
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

export default VerifyMobileModal;