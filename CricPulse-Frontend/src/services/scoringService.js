
import apiClient from './apiClient';

export const getMatchState = async (matchId) => {
  const response = await apiClient.get(
    `/Scoring/${matchId}/state`
  );

  return response.data;
};

export const scoreRuns = async (
  inningsId,
  createdByUserId,
  runs
) => {
  const response = await apiClient.post(
    `/Scoring/innings/${inningsId}/runs`,
    {
      createdByUserId,
      runs,
    }
  );

  return response.data;
};

export const scoreWicket = async (
  inningsId,
  wicketData
) => {
  const response = await apiClient.post(
    `/Scoring/innings/${inningsId}/wicket`,
    wicketData
  );

  return response.data;
};

export const scoreExtra = async (
  inningsId,
  extraData
) => {
  const response = await apiClient.post(
    `/Scoring/innings/${inningsId}/extra`,
    extraData
  );

  return response.data;
};

export const undoLastBall = async (
  inningsId,
  createdByUserId
) => {
  const response = await apiClient.post(
    `/Scoring/innings/${inningsId}/undo`,
    createdByUserId,
    {
      headers: {
        'Content-Type': 'application/json',
      },
    }
  );

  return response.data;
};

export const changeBowler = async (
  inningsId,
  createdByUserId,
  bowlerMatchPlayerId
) => {
  const response = await apiClient.post(
    `/Scoring/innings/${inningsId}/bowler`,
    {
      createdByUserId,
      bowlerMatchPlayerId,
    }
  );

  return response.data;
};


export const completeInnings = async (
  inningsId,
  createdByUserId
) => {
  const response = await apiClient.post(
    `/Scoring/innings/${inningsId}/complete`,
    {
      createdByUserId,
    }
  );

  return response.data;
};
