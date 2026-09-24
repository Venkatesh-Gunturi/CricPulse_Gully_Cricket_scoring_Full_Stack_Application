import axios from "axios";

const API_URL = "https://localhost:7238/api/Match";

const authConfig = () => ({
  headers: {
    Authorization: `Bearer ${localStorage.getItem("token")}`
  }
});

export const createMatch = async (matchData) => {
  const response = await axios.post(
    API_URL,
    matchData,
    authConfig()
  );

  return response.data;
};

export const getMatchById = async (id) => {
  const response = await axios.get(
    `${API_URL}/${id}`,
    authConfig()
  );

  return response.data;
};

export const getAllMatches = async () => {
  const response = await axios.get(API_URL);

  return response.data;
};

export const getNearbyMatches = async (
  latitude,
  longitude
) => {
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

export const updateMatch = async (
  id,
  matchData
) => {
  const response = await axios.put(
    `${API_URL}/${id}`,
    matchData,
    authConfig()
  );

  return response.data;
};

export const startMatch = async (
  id,
  latitude,
  longitude
) => {
  const response = await axios.post(
    `${API_URL}/${id}/start`,
    {
      latitude,
      longitude
    },
    authConfig()
  );

  return response.data;
};

export const cancelMatch = async (id) => {
  const response = await axios.post(
    `${API_URL}/${id}/cancel`,
    {},
    authConfig()
  );

  return response.data;
};

export const getMyMatches = async () => {
  const response = await axios.get(
    `${API_URL}/my-matches`,
    authConfig()
  );

  return response.data;
};

export const lookupPlayerByMobile = async (
  mobileNumber
) => {
  const response = await axios.get(
    `${API_URL}/player-lookup`,
    {
      params: {
        mobileNumber
      },
      ...authConfig()
    }
  );

  return response.data;
};

export const startPlayerOnboarding = async (
  mobileNumber
) => {
  const response = await axios.post(
    `${API_URL}/player-onboarding`,
    {
      mobileNumber
    },
    authConfig()
  );

  return response.data;
};

export const verifyPlayerOnboarding = async (
  userId,
  otpCode
) => {
  const response = await axios.post(
    `${API_URL}/player-onboarding/verify`,
    {
      userId,
      otpCode
    },
    authConfig()
  );

  return response.data;
};

export const recordToss = async (
  matchId,
  tossWinnerTeam,
  tossDecision
) => {
  const response = await axios.post(
    `${API_URL}/record-toss`,
    {
      matchId,
      tossWinnerTeam,
      tossDecision
    },
    authConfig()
  );

  return response.data;
};

export const startInnings = async (
  matchId,
  inningsNumber,
  strikerMatchPlayerId,
  nonStrikerMatchPlayerId,
  bowlerMatchPlayerId
) => {
  const response = await axios.post(
    `${API_URL}/start-innings`,
    {
      matchId,
      inningsNumber,
      strikerMatchPlayerId,
      nonStrikerMatchPlayerId,
      bowlerMatchPlayerId
    },
    authConfig()
  );

  return response.data;
};

export const getLiveMatch = async (matchId) => {
  const response = await axios.get(
    `${API_URL}/live/${matchId}`,
    authConfig()
  );

  return response.data;
};

export const scoreRuns = async (
  inningsId,
  runs
) => {
  const response = await axios.post(
    `${API_URL}/score-runs`,
    {
      inningsId,
      runs
    },
    authConfig()
  );

  return response.data;
};

export const scoreWicket = async (
  wicketData
) => {
  const response = await axios.post(
    `${API_URL}/score-wicket`,
    wicketData,
    authConfig()
  );

  return response.data;
};

/*
 * Record a normal extra delivery.
 *
 * RunsCompleted is calculated according to the
 * type of extra:
 *
 * WIDE:
 *   WD1 -> 0 completed runs
 *   WD2 -> 1 completed run
 *   WD3 -> 2 completed runs
 *
 * NO BALL:
 *   NB1 -> 0 completed runs
 *   NB2 -> 1 completed run
 *   NB3 -> 2 completed runs
 *
 * BYE:
 *   B1 -> 1 completed run
 *   B2 -> 2 completed runs
 *
 * LEG BYE:
 *   LB1 -> 1 completed run
 *   LB2 -> 2 completed runs
 */
export const scoreExtra = async (
  inningsId,
  extraType,
  runs,
  batterRuns = 0
) => {
  const normalizedExtraType = String(
    extraType || ""
  ).toUpperCase();

  let runsCompleted = 0;

  if (
    normalizedExtraType === "WIDE" ||
    normalizedExtraType === "NO BALL"
  ) {
    runsCompleted = Math.max(
      Number(runs) - 1,
      0
    );
  } else if (
    normalizedExtraType === "BYE" ||
    normalizedExtraType === "LEG BYE"
  ) {
    runsCompleted = Math.max(
      Number(runs),
      0
    );
  }

  const response = await axios.post(
    `${API_URL}/score-extra`,
    {
      inningsId,
      extraType: normalizedExtraType,
      runs,
      batterRuns,
      runsCompleted
    },
    authConfig()
  );

  return response.data;
};

/*
 * Record an extra delivery that also contains
 * a run out.
 *
 * This uses the same score-extra endpoint because
 * ScoreExtraDto contains the run-out information.
 */
export const scoreExtraRunOut = async (
  inningsId,
  extraType,
  runs,
  batterRuns,
  dismissedMatchPlayerId,
  runsCompleted,
  didBattersCross,
  newBatterMatchPlayerId = null
) => {
  const response = await axios.post(
    `${API_URL}/score-extra`,
    {
      inningsId,
      extraType,
      runs,
      batterRuns,
      runsCompleted,
      dismissedMatchPlayerId,
      didBattersCross,
      newBatterMatchPlayerId
    },
    authConfig()
  );

  return response.data;
};

// ====================================================================
// CHANGE BOWLER
// ====================================================================

export const changeBowler = async (
  inningsId,
  newBowlerMatchPlayerId
) => {
  const response = await axios.post(
    `${API_URL}/change-bowler`,
    {
      inningsId,
      newBowlerMatchPlayerId
    },
    authConfig()
  );

  return response.data;
};

export const completeMatch = async (
  matchId
) => {
  const response = await axios.post(
    `${API_URL}/${matchId}/complete`,
    {},
    authConfig()
  );

  return response.data;
};

export const undoLastScore = async (
  inningsId,
  ballId
) => {
  const response = await axios.post(
    `${API_URL}/undo-score`,
    {
      inningsId,
      ballId
    },
    authConfig()
  );

  return response.data;
};