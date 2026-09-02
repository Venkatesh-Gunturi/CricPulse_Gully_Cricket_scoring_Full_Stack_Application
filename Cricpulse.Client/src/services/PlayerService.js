import axios from "axios";

const API_URL = "https://localhost:7238/api/Player";

//Creating player API
export const createPlayerProfile = async (profileData) => {
  const token = localStorage.getItem("token");

  const response = await axios.post(
    `${API_URL}/profile`,
    profileData,
    {
      headers: {
        Authorization: `Bearer ${token}`
      }
    }
  );

  return response.data;
};

//Fetching Player API
export const getPlayerProfile = async () => {
  const token = localStorage.getItem("token");

  const response = await axios.get(
    `${API_URL}/profile`,
    {
      headers: {
        Authorization: `Bearer ${token}`
      }
    }
  );

  return response.data;
};

//update API
export const updatePlayerProfile = async (profileData) => {
  const token = localStorage.getItem("token");

  const response = await axios.put(
    `${API_URL}/profile`,
    profileData,
    {
      headers: {
        Authorization: `Bearer ${token}`
      }
    }
  );

  return response.data;
};