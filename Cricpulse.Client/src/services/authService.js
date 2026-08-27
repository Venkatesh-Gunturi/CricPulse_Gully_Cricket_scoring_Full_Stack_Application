import axios from "axios";

const API_URL = "https://localhost:7238/api/Auth";

export const registerPlayer = async (formData) => {
  const response = await axios.post(
    `${API_URL}/register`,
    formData
  );

  return response.data;
};