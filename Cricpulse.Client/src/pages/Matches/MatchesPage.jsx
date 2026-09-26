import { useState } from "react";
import MatchList from "../../components/Match/MatchList";
import "./MatchesPage.css";

const MATCH_STATUSES = [
  { key: "Scheduled", label: "Upcoming" },
  { key: "Live", label: "Live" },
  { key: "Completed", label: "Finished" },
  { key: "Cancelled", label: "Cancelled" }
];

const INDIAN_STATES = [
  "Andhra Pradesh",
  "Arunachal Pradesh",
  "Assam",
  "Bihar",
  "Chhattisgarh",
  "Goa",
  "Gujarat",
  "Haryana",
  "Himachal Pradesh",
  "Jharkhand",
  "Karnataka",
  "Kerala",
  "Madhya Pradesh",
  "Maharashtra",
  "Manipur",
  "Meghalaya",
  "Mizoram",
  "Nagaland",
  "Odisha",
  "Punjab",
  "Rajasthan",
  "Sikkim",
  "Tamil Nadu",
  "Telangana",
  "Tripura",
  "Uttar Pradesh",
  "Uttarakhand",
  "West Bengal"
];

function MatchesPage({ onBack }) {
  const [activeStatus, setActiveStatus] = useState("Scheduled");
  const [selectedState, setSelectedState] = useState("");

  return (
    <main className="cp-matches-page">
      <div className="cp-matches-container">

        {/* Header */}
        <section className="cp-matches-header">
          <button
            type="button"
            className="cp-matches-back"
            onClick={onBack}
          >
            <span>←</span>
            Back
          </button>

          <div className="cp-matches-heading">
            <span className="cp-section-eyebrow">
              CricPulse • Local Cricket
            </span>

            <h1>Matches</h1>

            <p>
              Find cricket matches happening around you and across the
              country.
            </p>
          </div>
        </section>

        {/* Status Tabs */}
        <section className="cp-match-status-tabs">
          {MATCH_STATUSES.map((status) => (
            <button
              key={status.key}
              type="button"
              className={`cp-match-status-tab ${
                activeStatus === status.key ? "active" : ""
              } ${
                status.key === "Live" ? "live-tab" : ""
              }`}
              onClick={() => setActiveStatus(status.key)}
            >
              {status.key === "Live" && (
                <span className="cp-live-dot" />
              )}

              {status.label}
            </button>
          ))}
        </section>

        {/* Nearby Matches */}
        <section className="cp-match-section">
          <div className="cp-match-section-header">
            <div>
              <span className="cp-section-eyebrow">
                Around you
              </span>

              <h2>Nearby Matches</h2>

              <p>
                Matches closest to your current location.
              </p>
            </div>

            <span className="cp-section-badge">
              {MATCH_STATUSES.find(
                (item) => item.key === activeStatus
              )?.label}
            </span>
          </div>

          <MatchList
            status={activeStatus}
            mode="nearby"
          />
        </section>

        {/* Divider */}
        <div className="cp-match-section-divider" />

        {/* State-wise Matches */}
        <section className="cp-match-section">
          <div className="cp-match-section-header cp-state-header">
            <div>
              <span className="cp-section-eyebrow">
                Explore by location
              </span>

              <h2>State-wise Matches</h2>

              <p>
                Browse matches from a specific Indian state.
              </p>
            </div>

            <div className="cp-state-selector">
              <label htmlFor="match-state">
                State
              </label>

              <select
                id="match-state"
                value={selectedState}
                onChange={(event) =>
                  setSelectedState(event.target.value)
                }
              >
                <option value="">
                  Select state
                </option>

                {INDIAN_STATES.map((state) => (
                  <option key={state} value={state}>
                    {state}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {selectedState ? (
            <MatchList
              status={activeStatus}
              mode="state"
              state={selectedState}
            />
          ) : (
            <div className="cp-state-empty">
              <div className="cp-state-empty-icon">
                🗺️
              </div>

              <h3>Select a state</h3>

              <p>
                Choose a state above to see matches available
                there.
              </p>
            </div>
          )}
        </section>

      </div>
    </main>
  );
}

export default MatchesPage;