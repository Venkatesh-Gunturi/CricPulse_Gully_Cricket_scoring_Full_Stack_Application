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

// Purpose: Start player onboarding and generate an OTP for an unregistered mobile number.
export const startPlayerOnboarding = async (mobileNumber) => {
  const token = localStorage.getItem("token");

  const response = await axios.post(
    `${API_URL}/player-onboarding`,
    {
      mobileNumber
    },
    {
      headers: {
        Authorization: `Bearer ${token}`
      }
    }
  );

  return response.data;
};

// Purpose: Verify the OTP generated for a new player during match creation.
export const verifyPlayerOnboarding = async (
  userId,
  otpCode
) => {
  const token = localStorage.getItem("token");

  const response = await axios.post(
    `${API_URL}/player-onboarding/verify`,
    {
      userId,
      otpCode
    },
    {
      headers: {
        Authorization: `Bearer ${token}`
      }
    }
  );

  return response.data;
};

// Purpose:
// Persist the toss winner and the winner's BAT/BOWL decision for the match.
export const recordToss = async (
  matchId,
  tossWinnerTeam,
  tossDecision
) => {
  const token = localStorage.getItem("token");

  const response = await axios.post(
    `${API_URL}/record-toss`,
    {
      matchId,
      tossWinnerTeam,
      tossDecision
    },
    {
      headers: {
        Authorization: `Bearer ${token}`
      }
    }
  );

  return response.data;
};

// Purpose:
// Start an innings by sending the selected striker, non-striker,
// and bowler to the backend scoring workflow.
export const startInnings = async (
  matchId,
  strikerMatchPlayerId,
  nonStrikerMatchPlayerId,
  bowlerMatchPlayerId
) => {
  const token = localStorage.getItem("token");

  const response = await axios.post(
    `${API_URL}/start-innings`,
    {
      matchId,
      strikerMatchPlayerId,
      nonStrikerMatchPlayerId,
      bowlerMatchPlayerId
    },
    {
      headers: {
        Authorization: `Bearer ${token}`
      }
    }
  );

  return response.data;
};

// Purpose:
// Fetch the current live match state, including the active innings,
// selected players, score, overs, and ball history.
export const getLiveMatch = async (matchId) => {
  const token = localStorage.getItem("token");

  const response = await axios.get(
    `${API_URL}/live/${matchId}`,
    {
      headers: {
        Authorization: `Bearer ${token}`
      }
    }
  );

  return response.data;
};

// Purpose:
// Record runs scored directly from the bat for the current live innings.
export const scoreRuns = async (
  inningsId,
  runs
) => {
  const token = localStorage.getItem("token");

  const response = await axios.post(
    `${API_URL}/score-runs`,
    {
      inningsId,
      runs
    },
    {
      headers: {
        Authorization: `Bearer ${token}`
      }
    }
  );

  return response.data;
};



// Purpose:
// Record an extra delivery such as wide, no-ball, bye, or leg-bye.
export const scoreExtra = async (
  inningsId,
  extraType,
  runs,
  batterRuns = 0
) => {
  const token = localStorage.getItem("token");

  const response = await axios.post(
    `${API_URL}/score-extra`,
    {
      inningsId,
      extraType,
      runs,
      batterRuns
    },
    {
      headers: {
        Authorization: `Bearer ${token}`
      }
    }
  );

  return response.data;
};