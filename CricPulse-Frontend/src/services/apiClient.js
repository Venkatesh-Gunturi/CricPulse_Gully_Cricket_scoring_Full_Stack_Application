import axios from 'axios';

const apiClient = axios.create({
  baseURL:
  import.meta.env.VITE_API_BASE_URL ||
  'http://192.168.1.6:7066/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

export default apiClient;