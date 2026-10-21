
import apiClient from './apiClient';

export const getMatchState = async (matchId) => {
  const response = await apiClient.get(
    `/Scoring/${matchId}/state`
  );

  return response.data;
};

export const scoreRuns = async (inningsId, createdByUserId, runs) => {
  const response = await apiClient.post(
    `/Scoring/innings/${inningsId}/runs`,
    {
      createdByUserId,
      runs,
    }
  );

  return response.data;
};
