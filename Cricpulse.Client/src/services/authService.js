import axios from "axios";

const API_URL = "https://localhost:7238/api/Auth";

// Purpose:
// Start registration or complete account creation after mobile verification.
export const registerPlayer = async (formData) => {
  const response = await axios.post(
    `${API_URL}/register`,
    formData
  );

  return response.data;
};

// Purpose:
// Verify the OTP associated with the temporary registration.
export const verifyOtp = async (registrationId, otpCode) => {
  const response = await axios.post(
    `${API_URL}/verify-otp`,
    {
      registrationId: registrationId,
      otpCode: otpCode
    }
  );

  return response.data;
};

// Purpose:
// Authenticate an existing CricPulse user.
export const loginPlayer = async (loginData) => {
  const response = await axios.post(
    `${API_URL}/login`,
    loginData
  );

  return response.data;
};

// Purpose:
// Test whether the current authentication token is valid.
export const testAuth = async () => {
  const token = localStorage.getItem("token");

  const response = await axios.get(
    `${API_URL}/test-auth`,
    {
      headers: {
        Authorization: `Bearer ${token}`
      }
    }
  );

  return response.data;
};