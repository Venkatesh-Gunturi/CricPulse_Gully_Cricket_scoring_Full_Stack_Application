import { useState } from "react";
import {
  lookupPlayerByMobile
} from "../../services/matchService";

const MatchPlayerAssignment = ({
  playersPerTeam,
  team1Players,
  team2Players,
  setTeam1Players,
  setTeam2Players
}) => {
  const [mobileNumber, setMobileNumber] =
    useState("");

  const [selectedTeam, setSelectedTeam] =
    useState("Team1");

  const [loading, setLoading] =
    useState(false);

  const [error, setError] =
    useState("");

  const handleAddPlayer = async () => {
    if (!mobileNumber.trim()) {
      setError("Enter a mobile number.");
      return;
    }

    const allPlayers = [
      ...team1Players,
      ...team2Players
    ];

    if (
      allPlayers.some(
        player =>
          player.mobileNumber ===
          mobileNumber.trim()
      )
    ) {
      setError(
        "This player is already assigned to this match."
      );

      return;
    }

    const currentTeamPlayers =
      selectedTeam === "Team1"
        ? team1Players
        : team2Players;

    if (
      currentTeamPlayers.length >=
      playersPerTeam
    ) {
      setError(
        "This team already has the required number of players."
      );

      return;
    }

    try {
      setLoading(true);
      setError("");

      const result =
        await lookupPlayerByMobile(
          mobileNumber.trim()
        );

      if (!result.isRegistered) {
        setError(
          "This player is not registered. OTP onboarding is required."
        );

        return;
      }

      const player = {
        mobileNumber:
          mobileNumber.trim(),

        playerId:
          result.playerId,

        displayName:
          result.displayName,

        verified: true
      };

      if (selectedTeam === "Team1") {
        setTeam1Players([
          ...team1Players,
          player
        ]);
      } else {
        setTeam2Players([
          ...team2Players,
          player
        ]);
      }

      setMobileNumber("");
    } catch (error) {
      console.error(
        "Player lookup failed:",
        error
      );

      setError(
        "Unable to find player."
      );
    } finally {
      setLoading(false);
    }
  };

  const removePlayer = (
    team,
    index
  ) => {
    if (team === "Team1") {
      setTeam1Players(
        team1Players.filter(
          (_, i) => i !== index
        )
      );
    } else {
      setTeam2Players(
        team2Players.filter(
          (_, i) => i !== index
        )
      );
    }
  };

  const renderTeam = (
    title,
    players,
    team
  ) => (
    <div className="col-md-6">
      <div className="card">
        <div className="card-body">

          <h5>
            {title}
          </h5>

          <p className="text-muted">
            {players.length} /{" "}
            {playersPerTeam} players
          </p>

          {players.map(
            (player, index) => (
              <div
                key={`${player.playerId}-${index}`}
                className="d-flex justify-content-between align-items-center border rounded p-2 mb-2"
              >
                <div>
                  <strong>
                    {player.displayName}
                  </strong>

                  <div className="text-success small">
                    ✓ Verified player
                  </div>
                </div>

                <button
                  type="button"
                  className="btn btn-sm btn-outline-danger"
                  onClick={() =>
                    removePlayer(
                      team,
                      index
                    )
                  }
                >
                  Remove
                </button>
              </div>
            )
          )}

          {players.length <
            playersPerTeam && (
            <p className="text-muted small">
              {playersPerTeam -
                players.length}{" "}
              player slot(s) remaining.
            </p>
          )}

        </div>
      </div>
    </div>
  );

  return (
    <div className="mt-4">

      <h4>
        Add Players
      </h4>

      <div className="row mb-3">

        <div className="col-md-4">
          <label className="form-label">
            Team
          </label>

          <select
            className="form-select"
            value={selectedTeam}
            onChange={(e) =>
              setSelectedTeam(
                e.target.value
              )
            }
          >
            <option value="Team1">
              Team 1
            </option>

            <option value="Team2">
              Team 2
            </option>
          </select>
        </div>

        <div className="col-md-5">
          <label className="form-label">
            Player Mobile Number
          </label>

          <input
            type="tel"
            className="form-control"
            value={mobileNumber}
            onChange={(e) =>
              setMobileNumber(
                e.target.value
              )
            }
            placeholder="Enter mobile number"
          />
        </div>

        <div className="col-md-3 d-flex align-items-end">
          <button
            type="button"
            className="btn btn-primary w-100"
            onClick={handleAddPlayer}
            disabled={loading}
          >
            {loading
              ? "Checking..."
              : "Add Player"}
          </button>
        </div>

      </div>

      {error && (
        <div className="alert alert-warning">
          {error}
        </div>
      )}

      <div className="row">
        {renderTeam(
          "Team 1",
          team1Players,
          "Team1"
        )}

        {renderTeam(
          "Team 2",
          team2Players,
          "Team2"
        )}
      </div>

    </div>
  );
};

export default MatchPlayerAssignment;