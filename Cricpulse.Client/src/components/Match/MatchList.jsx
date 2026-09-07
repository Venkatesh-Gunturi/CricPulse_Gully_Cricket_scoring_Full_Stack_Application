import { useEffect, useState } from "react";
import {  getAllMatches,getNearbyMatches,getMatchesByState} from "../../services/matchService";

const MatchList = ({ status, location, state   }) => {
  const [matches, setMatches] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

 useEffect(() => {
  const loadMatches = async () => {
    try {
      setLoading(true);

     const data = state
        ? await getMatchesByState(state) : location      
        ? await getNearbyMatches(
        location.latitude,
        location.longitude
      ): await getAllMatches();
    

      console.log("Location:", location);
      console.log("Matches received:", data);

      const filteredMatches = data.filter(
        (match) => match.status === status
      );

      console.log("Selected status:", status);
      console.log("Filtered matches:", filteredMatches);

      setMatches(filteredMatches);
    } catch (error) {
      console.error("Failed to load matches:", error);

      setError("Failed to load matches.");
    } finally {
      setLoading(false);
    }
  };

  loadMatches();
}, [status, location, state]);

  if (loading) {
    return <p>Loading matches...</p>;
  }

  if (error) {
    return <p>{error}</p>;
  }

  if (matches.length === 0) {
    return <p>No matches available.</p>;
  }

  return (
    <div className="container mt-5">
      <h2>
        {status === "Scheduled" && "Upcoming Matches"}
        {status === "Live" && "Live Matches"}
        {status === "Completed" && "Finished Matches"}
      </h2>

      {matches.map((match) => (
        <div key={match.id} className="card mb-3">
          <div className="card-body">

            <h5 className="card-title">
              {match.team1Name} vs {match.team2Name}
            </h5>

            <p className="card-text">
              <strong>Date:</strong>{" "}
              {new Date(match.matchDate).toLocaleDateString()}
            </p>

            <p className="card-text">
              <strong>Time:</strong> {match.matchTime}
            </p>

            <p className="card-text">
              <strong>Venue:</strong> {match.venueName}
            </p>

            <p className="card-text">
              <strong>Address:</strong> {match.address}
            </p>

            {(match.status === "Live" || match.status === "Scheduled") &&
                match.distanceInKm != null && (
            <p className="card-text">
            <strong>Distance:</strong>{" "}
            {match.distanceInKm.toFixed(1)} km away
            </p>
            )}

            <p className="card-text">
              <strong>Overs:</strong> {match.overs}
            </p>

            <p className="card-text">
              <strong>Players per team:</strong>{" "}
              {match.playersPerTeam}
            </p>

            <p className="card-text">
              <strong>Status:</strong> {match.status}
            </p>

          </div>
        </div>
      ))}
    </div>
  );
};

export default MatchList;