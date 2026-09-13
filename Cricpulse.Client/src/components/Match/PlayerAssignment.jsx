import { useEffect, useState } from "react";
import "./MatchPlayerAssignment.css";
import {
  lookupPlayerByMobile,
  startPlayerOnboarding,
  verifyPlayerOnboarding
} from "../../services/matchService";

const createEmptySlot = () => ({
  mobileNumber: "",
  playerId: null,
  displayName: "",
  verified: false,
  onboardingRequired: false,
  userId: null,
  otp: "",
  error: "",
  submitting: false,
  verifying: false
});

// Purpose:
// Convert existing match-player data into the internal slot structure.
const createInitialSlots = (players) => {
  if (!players || players.length === 0) {
    return [createEmptySlot()];
  }

  return players.map((player) => ({
    mobileNumber: player.mobileNumber,
    playerId: player.playerId,
    displayName: player.displayName,
    verified: true,
    onboardingRequired: false,
    userId: null,
    otp: "",
    error: "",
    submitting: false,
    verifying: false
  }));
};

const MatchPlayerAssignment = ({
  playersPerTeam,
  team1Players,
  team2Players,
  setTeam1Players,
  setTeam2Players,
  initialTeam1Players = [],
  initialTeam2Players = []
}) => {
  const [team1Slots, setTeam1Slots] = useState(() =>
    createInitialSlots(initialTeam1Players)
  );

  const [team2Slots, setTeam2Slots] = useState(() =>
    createInitialSlots(initialTeam2Players)
  );

  // Purpose:
// Synchronize Team 1 slots when the selected team size or initial edit lineup changes.
useEffect(() => {
  setTeam1Slots((current) => {
    const hasInitialPlayers =
      initialTeam1Players &&
      initialTeam1Players.length > 0;

    if (hasInitialPlayers) {
      const initialSlots =
        createInitialSlots(
          initialTeam1Players
        );

      const updated =
        initialSlots.slice(
          0,
          playersPerTeam
        );

      if (
        updated.length < playersPerTeam &&
        updated.every((slot) => slot.verified)
      ) {
        updated.push(createEmptySlot());
      }

      setTeam1Players(
        updated
          .filter((slot) => slot.verified)
          .map(toPlayer)
      );

      return updated;
    }

    const updated =
      current.slice(
        0,
        playersPerTeam
      );

    if (
      updated.length === 0 ||
      (
        updated.length < playersPerTeam &&
        updated.every((slot) => slot.verified)
      )
    ) {
      updated.push(createEmptySlot());
    }

    setTeam1Players(
      updated
        .filter((slot) => slot.verified)
        .map(toPlayer)
    );

    return updated;
  });
}, [playersPerTeam, initialTeam1Players]);

// Purpose:
// Synchronize Team 2 slots when the selected team size or initial edit lineup changes.
useEffect(() => {
  setTeam2Slots((current) => {
    const hasInitialPlayers =
      initialTeam2Players &&
      initialTeam2Players.length > 0;

    if (hasInitialPlayers) {
      const initialSlots =
        createInitialSlots(
          initialTeam2Players
        );

      const updated =
        initialSlots.slice(
          0,
          playersPerTeam
        );

      if (
        updated.length < playersPerTeam &&
        updated.every((slot) => slot.verified)
      ) {
        updated.push(createEmptySlot());
      }

      setTeam2Players(
        updated
          .filter((slot) => slot.verified)
          .map(toPlayer)
      );

      return updated;
    }

    const updated =
      current.slice(
        0,
        playersPerTeam
      );

    if (
      updated.length === 0 ||
      (
        updated.length < playersPerTeam &&
        updated.every((slot) => slot.verified)
      )
    ) {
      updated.push(createEmptySlot());
    }

    setTeam2Players(
      updated
        .filter((slot) => slot.verified)
        .map(toPlayer)
    );

    return updated;
  });
}, [playersPerTeam, initialTeam2Players]);


  // Purpose:
  // Convert a verified slot into the player object used by the parent component.
  const toPlayer = (slot) => ({
    mobileNumber: slot.mobileNumber,
    playerId: slot.playerId,
    displayName: slot.displayName,
    verified: true
  });

  // Purpose:
  // Extract the useful error message returned by the API.
  const getErrorMessage = (error) => {
    const responseData = error?.response?.data;

    if (typeof responseData === "string") {
      return responseData;
    }

    if (responseData?.message) {
      return responseData.message;
    }

    return "Something went wrong. Please try again.";
  };

  // Purpose:
  // Check whether a player is already assigned anywhere in either team.
  const isDuplicatePlayer = (
    playerId,
    mobileNumber,
    currentTeam,
    currentIndex
  ) => {
    const normalizedMobile = mobileNumber.trim();

    const allSlots = [
      ...team1Slots.map((slot, index) => ({
        slot,
        team: "team1",
        index
      })),
      ...team2Slots.map((slot, index) => ({
        slot,
        team: "team2",
        index
      }))
    ];

    return allSlots.some(
      ({ slot, team, index }) =>
        slot.verified &&
        !(team === currentTeam && index === currentIndex) &&
        (
          slot.playerId === playerId ||
          slot.mobileNumber === normalizedMobile
        )
    );
  };

  // Purpose:
  // Update a player's mobile number and reset previous verification state.
  const handleMobileChange = (team, index, value) => {
    const setter =
      team === "team1"
        ? setTeam1Slots
        : setTeam2Slots;

    setter((current) =>
      current.map((slot, slotIndex) => {
        if (slotIndex !== index) {
          return slot;
        }

        if (!value.trim()) {
          return createEmptySlot();
        }

        return {
          ...slot,
          mobileNumber: value,
          playerId: null,
          displayName: "",
          verified: false,
          onboardingRequired: false,
          userId: null,
          otp: "",
          error: ""
        };
      })
    );
  };

  // Purpose:
  // Update one property of a specific player slot.
  const updateSlot = (team, index, changes) => {
    const setter =
      team === "team1"
        ? setTeam1Slots
        : setTeam2Slots;

    setter((current) =>
      current.map((slot, slotIndex) =>
        slotIndex === index
          ? { ...slot, ...changes }
          : slot
      )
    );
  };

  // Purpose:
  // Look up an existing player or start onboarding for an unregistered player.
  const handleSubmit = async (team, index) => {
    const slots =
      team === "team1"
        ? team1Slots
        : team2Slots;

    const slot = slots[index];

    const mobileNumber =
      slot?.mobileNumber?.trim() || "";

    if (!mobileNumber) {
      updateSlot(team, index, {
        error: "Mobile number is required."
      });

      return;
    }

    if (
      mobileNumber.length !== 10 ||
      !/^[0-9]{10}$/.test(mobileNumber)
    ) {
      updateSlot(team, index, {
        error: "Mobile number must contain exactly 10 digits."
      });

      return;
    }

    updateSlot(team, index, {
      submitting: true,
      error: ""
    });

    try {
      const result =
        await lookupPlayerByMobile(mobileNumber);

      if (result.isRegistered) {
        if (!result.playerId) {
          updateSlot(team, index, {
            submitting: false,
            error: "Player profile is not available."
          });

          return;
        }

        if (
          isDuplicatePlayer(
            result.playerId,
            mobileNumber,
            team,
            index
          )
        ) {
          updateSlot(team, index, {
            submitting: false,
            error: "This player is already assigned to a team."
          });

          return;
        }

        markPlayerVerified(team, index, {
          mobileNumber,
          playerId: result.playerId,
          displayName:
            result.displayName || "Player"
        });

        return;
      }

      const onboarding =
        await startPlayerOnboarding(mobileNumber);

      updateSlot(team, index, {
        submitting: false,
        onboardingRequired: true,
        userId: onboarding.userId,
        otp: "",
        error: ""
      });
    } catch (error) {
      updateSlot(team, index, {
        submitting: false,
        error: getErrorMessage(error)
      });
    }
  };

  // Purpose:
  // Verify a new player's OTP and reload their player profile.
  const handleVerifyOtp = async (team, index) => {
    const slots =
      team === "team1"
        ? team1Slots
        : team2Slots;

    const slot = slots[index];

    const otp = slot?.otp?.trim() || "";

    if (!otp) {
      updateSlot(team, index, {
        error: "OTP is required."
      });

      return;
    }

    if (!/^[0-9]{6}$/.test(otp)) {
      updateSlot(team, index, {
        error: "OTP must contain exactly 6 digits."
      });

      return;
    }

    updateSlot(team, index, {
      verifying: true,
      error: ""
    });

    try {
      await verifyPlayerOnboarding(
        slot.userId,
        otp
      );

      const result =
        await lookupPlayerByMobile(
          slot.mobileNumber.trim()
        );

      if (
        !result.isRegistered ||
        !result.playerId
      ) {
        updateSlot(team, index, {
          verifying: false,
          error:
            "Verification completed, but player profile could not be loaded."
        });

        return;
      }

      if (
        isDuplicatePlayer(
          result.playerId,
          slot.mobileNumber,
          team,
          index
        )
      ) {
        updateSlot(team, index, {
          verifying: false,
          error:
            "This player is already assigned to a team."
        });

        return;
      }

      markPlayerVerified(team, index, {
        mobileNumber:
          slot.mobileNumber.trim(),
        playerId: result.playerId,
        displayName:
          result.displayName || "Player"
      });
    } catch (error) {
      updateSlot(team, index, {
        verifying: false,
        error: getErrorMessage(error)
      });
    }
  };

  // Purpose:
  // Mark a player as verified and reveal the next available player slot.
  const markPlayerVerified = (team, index, player) => {
    const setter =
      team === "team1"
        ? setTeam1Slots
        : setTeam2Slots;

    const setPlayers =
      team === "team1"
        ? setTeam1Players
        : setTeam2Players;

    setter((current) => {
      const updated = current.map(
        (slot, slotIndex) =>
          slotIndex === index
            ? {
                ...slot,
                ...player,
                verified: true,
                onboardingRequired: false,
                userId: null,
                otp: "",
                error: "",
                submitting: false,
                verifying: false
              }
            : slot
      );

      const verifiedPlayers =
        updated
          .filter((slot) => slot.verified)
          .map(toPlayer);

      setPlayers(verifiedPlayers);

      if (
        updated.length < playersPerTeam &&
        updated.every((slot) => slot.verified)
      ) {
        updated.push(createEmptySlot());
      }

      return updated;
    });
  };

  // Purpose:
  // Remove a player and make the slot available again.
  const handleRemove = (team, index) => {
    const setter =
      team === "team1"
        ? setTeam1Slots
        : setTeam2Slots;

    const setPlayers =
      team === "team1"
        ? setTeam1Players
        : setTeam2Players;

    setter((current) => {
      const updated = current.filter(
        (_, slotIndex) => slotIndex !== index
      );

      if (updated.length === 0) {
        updated.push(createEmptySlot());
      }

      setPlayers(
        updated
          .filter((slot) => slot.verified)
          .map(toPlayer)
      );

      return updated;
    });
  };

  // Purpose:
  // Render one team's player assignment slots.
  const renderTeamSlots = (team, slots) => {
    const teamName =
      team === "team1"
        ? "Team 1"
        : "Team 2";

    return (
      <div className="player-team-column">
        <h3>{teamName}</h3>

        <div className="player-list">
          {slots.map((slot, index) => {
            const isCaptain = index === 0;

            return (
              <div
                key={`${team}-${index}`}
                className="player-slot"
              >
                <div className="player-slot-header">
                  <span>
                    {slot.verified
                      ? `${slot.displayName}${
                          isCaptain ? " (C)" : ""
                        }`
                      : `Player ${index + 1}`}
                  </span>
                </div>

                {!slot.verified ? (
                  <>
                    <div className="player-input-row">
                      <input
                        type="tel"
                        placeholder="Mobile number"
                        value={slot.mobileNumber}
                        onChange={(event) =>
                          handleMobileChange(
                            team,
                            index,
                            event.target.value
                          )
                        }
                        disabled={
                          slot.submitting ||
                          slot.verifying
                        }
                      />

                      <button
                        type="button"
                        onClick={() =>
                          handleSubmit(
                            team,
                            index
                          )
                        }
                        disabled={
                          slot.submitting ||
                          slot.verifying
                        }
                      >
                        {slot.submitting
                          ? "Checking..."
                          : "Submit"}
                      </button>
                    </div>

                    {slot.onboardingRequired && (
                      <div className="otp-section">
                        <div className="unregistered-message">
                          Player not registered
                        </div>

                        <div className="otp-row">
                          <input
                            type="text"
                            placeholder="Enter OTP"
                            value={slot.otp}
                            onChange={(event) =>
                              updateSlot(
                                team,
                                index,
                                {
                                  otp: event.target.value,
                                  error: ""
                                }
                              )
                            }
                            disabled={
                              slot.verifying
                            }
                          />

                          <button
                            type="button"
                            onClick={() =>
                              handleVerifyOtp(
                                team,
                                index
                              )
                            }
                            disabled={
                              slot.verifying
                            }
                          >
                            {slot.verifying
                              ? "Verifying..."
                              : "Verify OTP"}
                          </button>
                        </div>
                      </div>
                    )}

                    {slot.error && (
                      <div className="player-error">
                        {slot.error}
                      </div>
                    )}
                  </>
                ) : (
                  <div className="verified-player">
                    <div className="verified-player-info">
                      <strong>
                        {slot.displayName}
                        {isCaptain && " (C)"}
                      </strong>

                      <span className="player-mobile">
                        {slot.mobileNumber}
                      </span>

                      <span className="verified-status">
                        ✓ Verified
                      </span>
                    </div>

                    <button
                      type="button"
                      onClick={() =>
                        handleRemove(
                          team,
                          index
                        )
                      }
                    >
                      Remove
                    </button>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </div>
    );
  };

  return (
    <section className="match-player-assignment">
      <h2>PLAYERS</h2>

      <div className="players-columns">
        {renderTeamSlots(
          "team1",
          team1Slots
        )}

        {renderTeamSlots(
          "team2",
          team2Slots
        )}
      </div>
    </section>
  );
};

export default MatchPlayerAssignment;