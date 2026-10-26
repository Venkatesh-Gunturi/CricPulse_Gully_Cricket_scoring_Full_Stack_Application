import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  FaArrowLeft,
  FaAward,
  FaBaseballBall,
  FaEnvelope,
  FaPhone,
  FaShieldAlt,
  FaSpinner,
  FaTrophy,
  FaUser,
  FaRedoAlt,
} from 'react-icons/fa';
import apiClient from '../services/apiClient';
import { useAuth } from '../context/AuthContext';
import '../styles/player-profile.css';

const EMPTY_VALUE = '—';

function read(object, ...keys) {
  for (const key of keys) {
    const value = object?.[key];
    if (value !== null && value !== undefined && value !== '') return value;
  }
  return null;
}

function display(value, suffix = '') {
  return value === null || value === undefined || value === ''
    ? EMPTY_VALUE
    : `${value}${suffix}`;
}

function calculateAge(dateOfBirth) {
  if (!dateOfBirth) return null;
  const birthDate = new Date(dateOfBirth);
  if (Number.isNaN(birthDate.getTime())) return null;

  const today = new Date();
  let age = today.getFullYear() - birthDate.getFullYear();
  const birthdayNotReached =
    today.getMonth() < birthDate.getMonth() ||
    (today.getMonth() === birthDate.getMonth() &&
      today.getDate() < birthDate.getDate());

  if (birthdayNotReached) age -= 1;
  return age >= 0 ? age : null;
}

function formatDate(value) {
  if (!value) return EMPTY_VALUE;
  const date = new Date(value);
  return Number.isNaN(date.getTime())
    ? EMPTY_VALUE
    : date.toLocaleDateString('en-IN', {
        day: '2-digit',
        month: 'short',
        year: 'numeric',
      });
}

function toDateInputValue(value) {
  if (!value) return '';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '';

  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(
    date.getDate()
  ).padStart(2, '0')}`;
}

function Detail({ label, value }) {
  return (
    <div className="cp-player-detail">
      <span className="cp-player-detail-label">{label}</span>
      <span className="cp-player-detail-value">{display(value)}</span>
    </div>
  );
}

function StatCard({ label, value, note }) {
  return (
    <article className="cp-player-stat-card">
      <span className="cp-player-stat-value">{display(value)}</span>
      <span className="cp-player-stat-label">{label}</span>
      {note ? <span className="cp-player-stat-note">{note}</span> : null}
    </article>
  );
}

function PlayerProfile() {
  const navigate = useNavigate();
  const { user } = useAuth();

  const [profileResponse, setProfileResponse] = useState(null);
  const [activeTab, setActiveTab] = useState('batting');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [isEditing, setIsEditing] = useState(false);
  const [saving, setSaving] = useState(false);
  const [saveError, setSaveError] = useState('');
  const [saveSuccess, setSaveSuccess] = useState('');
  const [editForm, setEditForm] = useState({
    firstName: '',
    lastName: '',
    profileImageUrl: '',
    dateOfBirth: '',
    gender: '',
    state: '',
    pinCode: '',
    battingStyle: '',
    bowlingStyle: '',
    playerRole: '',
  });

  useEffect(() => {
    let isMounted = true;

    async function loadProfile() {
      const userId = user?.userId ?? user?.id;
      if (!userId) {
        setError('Unable to identify your account. Please log in again.');
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError('');
        const response = await apiClient.get(`/AccountProfile/${userId}`);
        if (isMounted) setProfileResponse(response.data);
      } catch (requestError) {
        if (!isMounted) return;
        const status = requestError.response?.status;

        if (status === 401) {
          setError('Please log in to view your player profile.');
        } else if (status === 403) {
          setError('Only player accounts can access this profile.');
        } else if (status === 404) {
          setError('Your player profile was not found.');
        } else {
          setError(
            requestError.response?.data?.message ||
              requestError.response?.data?.title ||
              'We could not load your profile. Please try again.'
          );
        }
      } finally {
        if (isMounted) setLoading(false);
      }
    }

    loadProfile();
    return () => {
      isMounted = false;
    };
  }, [user]);

  const player = profileResponse;
  const stats = profileResponse?.statistics ?? profileResponse?.Statistics ?? {};
  const matchesPlayed = Number(read(stats, 'matches', 'Matches') ?? 0);
  const hasPlayedMatch = matchesPlayed > 0;

  const firstName = read(player, 'firstName', 'FirstName') ?? '';
  const lastName = read(player, 'lastName', 'LastName') ?? '';
  const playerName = `${firstName} ${lastName}`.trim() || 'Player';
  const imageUrl = read(player, 'profileImageUrl', 'ProfileImageUrl');
  const mobile = read(player, 'mobileNumber', 'MobileNumber');
  const email = read(player, 'email', 'Email');
  const dateOfBirth = read(player, 'dateOfBirth', 'DateOfBirth');
  const age = calculateAge(dateOfBirth);

  const battingAverage = useMemo(() => {
    const runs = Number(read(stats, 'runs', 'Runs') ?? 0);
    const innings = Number(read(stats, 'innings', 'Innings') ?? 0);
    const notOuts = Number(read(stats, 'notOuts', 'NotOuts') ?? 0);
    const dismissals = innings - notOuts;
    return dismissals > 0 ? (runs / dismissals).toFixed(2) : null;
  }, [stats]);

  const strikeRate = useMemo(() => {
    const runs = Number(read(stats, 'runs', 'Runs') ?? 0);
    const balls = Number(read(stats, 'ballsFaced', 'BallsFaced') ?? 0);
    return balls > 0 ? ((runs / balls) * 100).toFixed(2) : null;
  }, [stats]);

  const economy = useMemo(() => {
    const runs = Number(read(stats, 'runsConceded', 'RunsConceded') ?? 0);
    const balls = Number(read(stats, 'ballsBowled', 'BallsBowled') ?? 0);
    return balls > 0 ? ((runs / balls) * 6).toFixed(2) : null;
  }, [stats]);

  const initials =
    playerName
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part.charAt(0).toUpperCase())
      .join('') || 'P';

  const battingStats = [
    { label: 'Innings', value: read(stats, 'innings', 'Innings') },
    { label: 'Runs', value: read(stats, 'runs', 'Runs') },
    { label: 'Balls Faced', value: read(stats, 'ballsFaced', 'BallsFaced') },
    { label: 'Fours', value: read(stats, 'fours', 'Fours') },
    { label: 'Sixes', value: read(stats, 'sixes', 'Sixes') },
    { label: 'Highest Score', value: read(stats, 'highestScore', 'HighestScore') },
    { label: 'Fifties', value: read(stats, 'fifties', 'Fifties') },
    { label: 'Hundreds', value: read(stats, 'hundreds', 'Hundreds') },
    { label: 'Batting Average', value: battingAverage },
    { label: 'Strike Rate', value: strikeRate },
  ];

  const bowlingStats = [
    { label: 'Bowling Innings', value: read(stats, 'bowlingInnings', 'BowlingInnings') },
    { label: 'Balls Bowled', value: read(stats, 'ballsBowled', 'BallsBowled') },
    { label: 'Runs Conceded', value: read(stats, 'runsConceded', 'RunsConceded') },
    { label: 'Wickets', value: read(stats, 'wickets', 'Wickets') },
    {
      label: 'Maiden Overs',
      value: read(stats, 'maidens', 'Maidens', 'maidenOvers', 'MaidenOvers'),
    },
    { label: 'Economy Rate', value: economy },
    { label: '3-Wicket Hauls', value: read(stats, 'threeWicketHauls', 'ThreeWicketHauls') },
    { label: '5-Wicket Hauls', value: read(stats, 'fiveWicketHauls', 'FiveWicketHauls') },
    { label: 'Best Bowling', value: read(stats, 'bestBowling', 'BestBowling') },
  ];

  const handleEdit = () => {
    setEditForm({
      firstName: read(player, 'firstName', 'FirstName') ?? '',
      lastName: read(player, 'lastName', 'LastName') ?? '',
      profileImageUrl: read(player, 'profileImageUrl', 'ProfileImageUrl') ?? '',
      dateOfBirth: toDateInputValue(dateOfBirth),
      gender: read(player, 'gender', 'Gender') ?? '',
      state: read(player, 'state', 'State') ?? '',
      pinCode: read(player, 'pinCode', 'PinCode') ?? '',
      battingStyle: read(player, 'battingStyle', 'BattingStyle') ?? '',
      bowlingStyle: read(player, 'bowlingStyle', 'BowlingStyle') ?? '',
      playerRole: read(player, 'playerRole', 'PlayerRole') ?? '',
    });
    setSaveError('');
    setSaveSuccess('');
    setIsEditing(true);
  };

  const handleCancelEdit = () => {
    setIsEditing(false);
    setSaveError('');
    setSaveSuccess('');
  };

  const handleEditChange = (event) => {
    const { name, value } = event.target;
    setEditForm((previous) => ({ ...previous, [name]: value }));
  };

  const handleSaveProfile = async (event) => {
    event.preventDefault();

    const userId = user?.userId ?? user?.id;
    if (!userId) {
      setSaveError('Unable to identify your account. Please log in again.');
      return;
    }
    if (!editForm.firstName.trim()) {
      setSaveError('First name is required.');
      return;
    }
    if (editForm.pinCode.trim() && !/^\d{6}$/.test(editForm.pinCode.trim())) {
      setSaveError('Please enter a valid 6-digit PIN code.');
      return;
    }

    try {
      setSaving(true);
      setSaveError('');
      setSaveSuccess('');

      const payload = {
        firstName: editForm.firstName.trim(),
        lastName: editForm.lastName.trim() || null,
        profileImageUrl: editForm.profileImageUrl.trim() || null,
        dateOfBirth: editForm.dateOfBirth || null,
        gender: editForm.gender || null,
        state: editForm.state.trim() || null,
        pinCode: editForm.pinCode.trim() || null,
        battingStyle: editForm.battingStyle || null,
        bowlingStyle: editForm.bowlingStyle || null,
        playerRole: editForm.playerRole || null,
        // Contact details stay unchanged until OTP re-verification is supported.
        mobileNumber: mobile,
        email,
      };

      const response = await apiClient.put(`/AccountProfile/${userId}`, payload);
      const updatedProfile = response.data ?? {};

      setProfileResponse((previous) => ({
        ...previous,
        ...updatedProfile,
        statistics:
          previous?.statistics ??
          previous?.Statistics ??
          updatedProfile?.statistics ??
          updatedProfile?.Statistics ??
          {},
      }));

      setIsEditing(false);
      setSaveSuccess('Your profile has been updated successfully.');
    } catch (requestError) {
      setSaveError(
        requestError.response?.data?.message ||
          requestError.response?.data?.title ||
          'Unable to update your profile. Please try again.'
      );
    } finally {
      setSaving(false);
    }
  };

  const handleBack = () => {
    if (window.history.length > 1) navigate(-1);
    else navigate('/');
  };

  return (
    <main className="cp-player-profile-page">
      <div className="cp-player-profile-shell">
        <header className="cp-player-profile-topbar">
          <button type="button" className="cp-player-back" onClick={handleBack}>
            <FaArrowLeft aria-hidden="true" />
            <span>Back</span>
          </button>
          <h1>Player Profile</h1>
          <span className="cp-player-topbar-spacer" aria-hidden="true" />
        </header>

        {loading ? (
          <section className="cp-player-state" aria-live="polite">
            <FaSpinner className="cp-player-spinner" aria-hidden="true" />
            <p>Loading your player profile…</p>
          </section>
        ) : error ? (
          <section className="cp-player-state cp-player-state-error" role="alert">
            <div className="cp-player-state-icon"><FaUser aria-hidden="true" /></div>
            <h2>Profile unavailable</h2>
            <p>{error}</p>
            <button type="button" className="cp-player-primary-button" onClick={() => window.location.reload()}>
              <FaRedoAlt aria-hidden="true" /> Try again
            </button>
          </section>
        ) : !player ? (
          <section className="cp-player-state">
            <h2>Profile details unavailable</h2>
            <p>The profile endpoint returned no player details.</p>
          </section>
        ) : (
          <>
            <section className="cp-player-identity">
              <div className="cp-player-avatar-wrap">
                {imageUrl ? (
                  <img className="cp-player-avatar" src={imageUrl} alt={`${playerName} profile`} />
                ) : (
                  <div className="cp-player-avatar cp-player-avatar-fallback" aria-label={`${playerName} initials`}>
                    {initials}
                  </div>
                )}
                <span className="cp-player-avatar-badge" aria-hidden="true"><FaBaseballBall /></span>
              </div>
              <h2>{playerName}</h2>
              <p className="cp-player-identity-caption">CricPulse Player</p>
              <div className="cp-player-contact-row">
                <span className="cp-player-contact"><FaPhone aria-hidden="true" />{display(mobile)}</span>
                <span className="cp-player-contact-divider" aria-hidden="true" />
                <span className="cp-player-contact cp-player-email">
                  <FaEnvelope aria-hidden="true" /><span>{display(email)}</span>
                </span>
              </div>
            </section>

            {saveSuccess && (
              <p className="cp-player-form-message cp-player-form-success" role="status">
                {saveSuccess}
              </p>
            )}

            <section className="cp-player-panel" aria-labelledby="cp-personal-details-title">
              <div className="cp-player-section-heading cp-player-details-heading">
                <div className="cp-player-section-heading-content">
                  <div className="cp-player-section-icon"><FaUser aria-hidden="true" /></div>
                  <div>
                    <p className="cp-player-eyebrow">YOUR PROFILE</p>
                    <h2 id="cp-personal-details-title">Personal Details</h2>
                  </div>
                </div>
                {!isEditing && (
                  <button type="button" className="cp-player-update-button" onClick={handleEdit}>
                    Update Details
                  </button>
                )}
              </div>

              {saveError && (
                <p className="cp-player-form-message cp-player-form-error" role="alert">{saveError}</p>
              )}

              {isEditing ? (
                <form className="cp-player-edit-form" onSubmit={handleSaveProfile}>
                  <div className="cp-player-details-grid">
                    <label className="cp-player-edit-field">
                      First Name *
                      <input name="firstName" value={editForm.firstName} onChange={handleEditChange} required maxLength={100} />
                    </label>
                    <label className="cp-player-edit-field">
                      Last Name
                      <input name="lastName" value={editForm.lastName} onChange={handleEditChange} maxLength={100} />
                    </label>
                    <label className="cp-player-edit-field">
                      Email
                      <input type="email" value={email ?? ''} readOnly />
                    </label>
                    <label className="cp-player-edit-field">
                      Mobile Number
                      <input type="tel" value={mobile ?? ''} readOnly />
                    </label>
                    <label className="cp-player-edit-field">
                      Date of Birth
                      <input type="date" name="dateOfBirth" value={editForm.dateOfBirth} onChange={handleEditChange} />
                    </label>
                    <label className="cp-player-edit-field">
                      Gender
                      <select name="gender" value={editForm.gender} onChange={handleEditChange}>
                        <option value="">Select gender</option>
                        <option value="Male">Male</option>
                        <option value="Female">Female</option>
                        <option value="Other">Other</option>
                        <option value="Prefer not to say">Prefer not to say</option>
                      </select>
                    </label>
                    <label className="cp-player-edit-field">
                      State
                      <input name="state" value={editForm.state} onChange={handleEditChange} maxLength={100} />
                    </label>
                    <label className="cp-player-edit-field">
                      PIN Code
                      <input type="text" name="pinCode" value={editForm.pinCode} onChange={handleEditChange} inputMode="numeric" maxLength={6} pattern="[0-9]{6}" />
                    </label>
                    <label className="cp-player-edit-field">
                      Batting Style
                      <select name="battingStyle" value={editForm.battingStyle} onChange={handleEditChange}>
                        <option value="">Select batting style</option>
                        <option value="Right Hand">Right Hand</option>
                        <option value="Left Hand">Left Hand</option>
                      </select>
                    </label>
                    <label className="cp-player-edit-field">
                      Bowling Style
                      <select name="bowlingStyle" value={editForm.bowlingStyle} onChange={handleEditChange}>
                        <option value="">Select bowling style</option>
                        <option value="Right Arm Fast">Right Arm Fast</option>
                        <option value="Right Arm Medium">Right Arm Medium</option>
                        <option value="Right Arm Spin">Right Arm Spin</option>
                        <option value="Left Arm Fast">Left Arm Fast</option>
                        <option value="Left Arm Medium">Left Arm Medium</option>
                        <option value="Left Arm Spin">Left Arm Spin</option>
                      </select>
                    </label>
                    <label className="cp-player-edit-field">
                      Player Role
                      <select name="playerRole" value={editForm.playerRole} onChange={handleEditChange}>
                        <option value="">Select player role</option>
                        <option value="Batsman">Batsman</option>
                        <option value="Bowler">Bowler</option>
                        <option value="All-Rounder">All-Rounder</option>
                        <option value="Wicket-Keeper">Wicket-Keeper</option>
                      </select>
                    </label>
                    <label className="cp-player-edit-field cp-player-edit-full-width">
                      Profile Image URL
                      <input type="url" name="profileImageUrl" value={editForm.profileImageUrl} onChange={handleEditChange} placeholder="https://example.com/profile.jpg" />
                    </label>
                  </div>
                  <div className="cp-player-edit-actions">
                    <button type="button" className="cp-player-cancel-button" onClick={handleCancelEdit} disabled={saving}>
                      Cancel
                    </button>
                    <button type="submit" className="cp-player-update-button" disabled={saving}>
                      {saving ? 'Saving…' : 'Save Changes'}
                    </button>
                  </div>
                </form>
              ) : (
                <div className="cp-player-details-grid">
                  <Detail label="Age" value={age === null ? null : `${age} years`} />
                  <Detail label="Gender" value={read(player, 'gender', 'Gender')} />
                  <Detail label="PIN Code" value={read(player, 'pinCode', 'PinCode')} />
                  <Detail label="Player Role" value={read(player, 'playerRole', 'PlayerRole')} />
                  <Detail label="Batting Style" value={read(player, 'battingStyle', 'BattingStyle')} />
                  <Detail label="Bowling Style" value={read(player, 'bowlingStyle', 'BowlingStyle')} />
                  <Detail label="State" value={read(player, 'state', 'State')} />
                  <Detail label="Date of Birth" value={formatDate(dateOfBirth) === EMPTY_VALUE ? null : formatDate(dateOfBirth)} />
                </div>
              )}
            </section>

            <section className="cp-player-panel cp-player-stats-section">
              {hasPlayedMatch ? (
                <>
                  <div className="cp-player-section-heading">
                    <div className="cp-player-section-icon cp-player-section-icon-green"><FaTrophy aria-hidden="true" /></div>
                    <div>
                      <p className="cp-player-eyebrow">CAREER RECORD</p>
                      <h2 id="cp-player-stats-title">Player Statistics</h2>
                      <p className="cp-player-section-subtitle">Your overall performance at a glance.</p>
                    </div>
                  </div>
                  <div className="cp-player-common-stats">
                    <StatCard label="Matches Played" value={matchesPlayed} />
                    <StatCard label="Total Runs" value={read(stats, 'runs', 'Runs')} />
                    <StatCard label="Total Wickets" value={read(stats, 'wickets', 'Wickets')} />
                  </div>
                  <div className="cp-player-tabs" role="tablist" aria-label="Batting and bowling statistics">
                    <button type="button" role="tab" id="cp-batting-tab" aria-selected={activeTab === 'batting'} aria-controls="cp-batting-panel" className={activeTab === 'batting' ? 'active' : ''} onClick={() => setActiveTab('batting')}>
                      <FaBaseballBall aria-hidden="true" /> Batting
                    </button>
                    <button type="button" role="tab" id="cp-bowling-tab" aria-selected={activeTab === 'bowling'} aria-controls="cp-bowling-panel" className={activeTab === 'bowling' ? 'active' : ''} onClick={() => setActiveTab('bowling')}>
                      <FaShieldAlt aria-hidden="true" /> Bowling
                    </button>
                  </div>
                  {activeTab === 'batting' ? (
                    <div id="cp-batting-panel" role="tabpanel" aria-labelledby="cp-batting-tab" className="cp-player-stat-grid">
                      {battingStats.map((item) => <StatCard key={item.label} {...item} />)}
                    </div>
                  ) : (
                    <div id="cp-bowling-panel" role="tabpanel" aria-labelledby="cp-bowling-tab" className="cp-player-stat-grid">
                      {bowlingStats.map((item) => <StatCard key={item.label} {...item} />)}
                    </div>
                  )}
                </>
              ) : (
                <div className="cp-player-empty-stats">
                  <div className="cp-player-empty-stats-icon"><FaBaseballBall aria-hidden="true" /></div>
                  <p className="cp-player-eyebrow">YOUR JOURNEY STARTS HERE</p>
                  <h2>Your Cricket Journey Starts Here!</h2>
                  <p className="cp-player-empty-stats-message">You haven't played a match yet.</p>
                  <p className="cp-player-empty-stats-description">
                    Every great player starts somewhere! Play your first match and build your cricket story.
                    Your runs, wickets, and achievements will appear here as you play.
                  </p>
                </div>
              )}
            </section>

            {hasPlayedMatch && (
              <footer className="cp-player-profile-footer">
                <div className="cp-player-mvp-icon"><FaAward aria-hidden="true" /></div>
                <div className="cp-player-mvp-copy">
                  <p className="cp-player-eyebrow">MATCH RECOGNITION</p>
                  <h2>Man of the Match Awards</h2>
                  <p>Every standout performance counts.</p>
                </div>
                <div className="cp-player-mvp-count">{display(read(stats, 'mvpCount', 'MVPCount'))}</div>
              </footer>
            )}

            <section className="cp-player-umpire-link">
              <div className="cp-player-umpire-symbol"><FaShieldAlt aria-hidden="true" /></div>
              <div className="cp-player-umpire-copy">
                <h2>Want to see your umpire stats?</h2>
                <p>Manage matches and view your umpire dashboard.</p>
              </div>
              <button type="button" onClick={() => navigate('/umpire')}>
                Umpire Dashboard <span aria-hidden="true">→</span>
              </button>
            </section>
          </>
        )}
      </div>
    </main>
  );
}

export default PlayerProfile;
