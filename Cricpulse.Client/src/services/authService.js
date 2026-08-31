import axios from "axios";

const API_URL = "https://localhost:7238/api/Auth";

export const registerPlayer = async (formData) => {
  const response = await axios.post(
    `${API_URL}/register`,
    formData
  );

  return response.data;
};

export const verifyOtp = async (userId, otpCode, otpType) => {
  const response = await axios.post(
    `${API_URL}/verify-otp`,
    {
      userId: userId,
      otpCode: otpCode,
      otpType: otpType
    }
  );

  return response.data;
};

export const loginPlayer = async (loginData) => {
const response = await axios.post(
`${API_URL}/login`,
loginData
);

return response.data;
};