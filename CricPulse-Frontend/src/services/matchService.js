
import apiClient from './apiClient';

// Create a new match
export const createMatch = async (matchData) => {
  const response = await apiClient.post('/Match/create', matchData);
  return response.data;
};

// Get matches for public discovery
export const getMatches = async (params = {}) => {
  const response = await apiClient.get('/Match', { params });
  return response.data;
};

// Get matches created by a particular organizer
export const getMyMatches = async (createdByUserId, status) => {
  const params = { createdByUserId };

  if (status) {
    params.status = status;
  }

  const response = await apiClient.get('/Match/mine', { params });
  return response.data;
};

// View one match
export const getMatchById = async (matchId, createdByUserId) => {
  const response = await apiClient.get(`/Match/${matchId}`, {
    params: { createdByUserId },
  });

  return response.data;
};

// Update an existing match
export const updateMatch = async (
  matchId,
  createdByUserId,
  matchData
) => {
  const response = await apiClient.put(`/Match/${matchId}`, matchData, {
    params: { createdByUserId },
  });

  return response.data;
};

// Cancel an upcoming match
export const cancelMatch = async (
  matchId,
  createdByUserId,
  cancellationData
) => {
  const response = await apiClient.patch(
    `/Match/${matchId}/cancel`,
    cancellationData,
    { params: { createdByUserId } }
  );

  return response.data;
};
export const getPublicMatchById = async (matchId) => {
  const response = await apiClient.get(`/Match/public/${matchId}`);
  return response.data;
};
// Get all Indian states and union territories
export const getIndianStates = async () => {
  const response = await apiClient.get('/Match/states');
  return response.data;
};