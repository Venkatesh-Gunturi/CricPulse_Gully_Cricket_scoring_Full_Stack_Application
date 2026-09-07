import axios from "axios";

export const getCurrentLocation = () => {
    
  return new Promise((resolve, reject) => {
    if (!navigator.geolocation) {
      reject(new Error("Geolocation is not supported by this browser."));
      return;
    }

    navigator.geolocation.getCurrentPosition(
      (position) => {
        resolve({
          latitude: position.coords.latitude,
          longitude: position.coords.longitude
        });
      },
      (error) => {
        reject(error);
      }
    );
  });
};

export const getStateByLocation = async (latitude, longitude) => {
  const response = await axios.get(
    "https://localhost:7238/api/Location/state",
    {
      params: {
        latitude,
        longitude
      }
    }
  );

  return response.data;
};