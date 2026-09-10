import axios from "axios";

const API_URL = "https://localhost:7238/api/Match";

export const createMatch = async (matchData) => {
  const token = localStorage.getItem("token");

  const response = await axios.post(
    API_URL,
    matchData,
    {
      headers: {
        Authorization: `Bearer ${token}`
      }
    }
  );

  return response.data;
};

export const getMatchById = async (id) => {
  const token = localStorage.getItem("token");

  const response = await axios.get(
    `${API_URL}/${id}`,
    {
      headers: {
        Authorization: `Bearer ${token}`
      }
    }
  );

  return response.data;
};

export const getAllMatches = async () => {
  const response = await axios.get(API_URL);

  return response.data;
};

export const getNearbyMatches = async (latitude, longitude) => {
  const response = await axios.get(
    `${API_URL}/nearby`,
    {
      params: {
        latitude,
        longitude
      }
    }
  );

  return response.data;
};

export const getMatchesByState = async (state) => {
  const response = await axios.get(
    `${API_URL}/state/${state}`
  );

  return response.data;
};

export const updateMatch = async (id, matchData) => {
  const token = localStorage.getItem("token");

  const response = await axios.put(
    `${API_URL}/${id}`,
    matchData,
    {
      headers: {
        Authorization: `Bearer ${token}`
      }
    }
  );

  return response.data;
};

export const startMatch = async (id, latitude, longitude) => {
  const token = localStorage.getItem("token");

  const response = await axios.post(
    `${API_URL}/${id}/start`,
    {
      latitude,
      longitude
    },
    {
      headers: {
        Authorization: `Bearer ${token}`
      }
    }
  );

  return response.data;
};

export const cancelMatch = async (id) => {
  const token = localStorage.getItem("token");

  const response = await axios.post(
    `${API_URL}/${id}/cancel`,
    {},
    {
      headers: {
        Authorization: `Bearer ${token}`
      }
    }
  );

  return response.data;
};

export const getMyMatches = async () => {
  const token = localStorage.getItem("token");

  const response = await axios.get(
    `${API_URL}/my-matches`,
    {
      headers: {
        Authorization: `Bearer ${token}`
      }
    }
  );

  return response.data;
};

export const lookupPlayerByMobile = async (
  mobileNumber
) => {
  const token =
    localStorage.getItem("token");

  const response = await axios.get(
    `${API_URL}/player-lookup`,
    {
      params: {
        mobileNumber
      },
      headers: {
        Authorization:
          `Bearer ${token}`
      }
    }
  );

  return response.data;
};