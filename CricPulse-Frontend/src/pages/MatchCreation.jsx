import { useEffect, useMemo, useState } from 'react';

import { useNavigate, useParams } from 'react-router-dom';

import {

  getMatchById,

  updateMatch,

} from '../services/matchService';


import {



  FaArrowLeft,






  FaCalendarAlt,



  FaCheckCircle,



  FaMapMarkerAlt,



  FaSearch,



  FaShieldAlt,



  FaUsers,



} from 'react-icons/fa';



import apiClient from '../services/apiClient';



import { useAuth } from '../context/AuthContext';



import '../styles/match-creation.css';



const MIN_PLAYERS = 4;



const MAX_PLAYERS = 11;



const MIN_OVERS = 1;



const MAX_OVERS = 90;







const TEAM_LOGOS = [



  { value: 'crest-blue', label: 'Royal Blue', symbol: '🛡️', tone: 'blue' },



  { value: 'crest-red', label: 'Crimson', symbol: '🦅', tone: 'red' },



  { value: 'crest-green', label: 'Emerald', symbol: '🐉', tone: 'green' },



  { value: 'crest-purple', label: 'Violet', symbol: '⚡', tone: 'purple' },



  { value: 'crest-orange', label: 'Orange', symbol: '🔥', tone: 'orange' },



  { value: 'crest-black', label: 'Midnight', symbol: '🏏', tone: 'black' },



  { value: 'crest-gold', label: 'Gold', symbol: '⭐', tone: 'gold' },



  { value: 'crest-teal', label: 'Teal', symbol: '🌊', tone: 'teal' },



];







const getLogo = (value) => TEAM_LOGOS.find((logo) => logo.value === value) || TEAM_LOGOS[0];



const getValue = (object, ...keys) => {



  for (const key of keys) {



    if (object?.[key] !== undefined && object[key] !== null) return object[key];



  }



  return null;



};



const getTodayLocalDate = () => {

  const today = new Date();

  const year = today.getFullYear();

  const month = String(today.getMonth() + 1).padStart(2, '0');

  const day = String(today.getDate()).padStart(2, '0');



  return `${year}-${month}-${day}`;

};



const getErrorMessage = (error) =>



  error.response?.data?.message ||



  error.response?.data?.title ||



  (typeof error.response?.data === 'string' ? error.response.data : '') ||



  error.message ||



  'Something went wrong. Please try again.';



const emptySlot = () => ({



  identifier: '',



  mobileNumber: '',



  email: '',



  firstName: '',



  lastName: '',



  userId: null,



  playerNumber: null,



  lookupState: 'idle',



  lookupMessage: '',



  resolved: false,



  isNewPlayer: false,



});



const makeSlots = (count) => Array.from({ length: count }, emptySlot);



function MatchCreation() {



  const navigate = useNavigate();

  const { matchId } = useParams();

const isEditMode = Boolean(matchId);



  const { user } = useAuth();





const organizerUserId = getValue(

  user,

  'userId',

  'UserId',

  'id',

  'userID'

);




  const [form, setForm] = useState({



    name: '',



    teamAName: '',



    teamALogo: 'crest-blue',



    teamBName: '',



    teamBLogo: 'crest-red',



    scheduledAt: '',



    venue: '',



    locationAddress: '',



    latitude: '',



    longitude: '',



    overs: '10',



    playersPerTeam: '11',



  });



  const [teamAPlayers, setTeamAPlayers] = useState(() => makeSlots(11));



  const [teamBPlayers, setTeamBPlayers] = useState(() => makeSlots(11));



  const [error, setError] = useState('');



  const [saving, setSaving] = useState(false);



  const [locationLoading, setLocationLoading] = useState(false);



  const locationCaptured = form.latitude !== '' && form.longitude !== '';

  const [scheduleDate, setScheduleDate] = useState('');

  const [scheduleTime, setScheduleTime] = useState('');



useEffect(() => {

  if (!isEditMode || !matchId || !organizerUserId) return;



  let cancelled = false;



  const loadMatch = async () => {

    setSaving(true);

    setError('');



    try {

      const match = await getMatchById(matchId, organizerUserId);



      if (cancelled) return;



      const scheduledAt = getValue(match, 'scheduledAt', 'ScheduledAt');

      const date = scheduledAt ? new Date(scheduledAt) : null;



      if (!date || Number.isNaN(date.getTime())) {

        throw new Error('The match has an invalid scheduled date.');

      }



      const pad = (value) => String(value).padStart(2, '0');



      const localDate =

        `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;



      const localTime =

        `${pad(date.getHours())}:${pad(date.getMinutes())}`;



      setForm({

        name: getValue(match, 'name', 'Name') || '',

        teamAName: getValue(match, 'teamAName', 'TeamAName') || '',

        teamALogo: getValue(match, 'teamALogo', 'TeamALogo') || 'crest-blue',

        teamBName: getValue(match, 'teamBName', 'TeamBName') || '',

        teamBLogo: getValue(match, 'teamBLogo', 'TeamBLogo') || 'crest-red',

        scheduledAt: `${localDate}T${localTime}`,

        venue: getValue(match, 'venue', 'Venue') || '',

        locationAddress: getValue(match, 'locationAddress', 'LocationAddress') || '',

        latitude: String(getValue(match, 'latitude', 'Latitude') ?? ''),

        longitude: String(getValue(match, 'longitude', 'Longitude') ?? ''),

        overs: String(getValue(match, 'overs', 'Overs') ?? 10),

        playersPerTeam: String(getValue(match, 'playersPerTeam', 'PlayersPerTeam') ?? 11),

      });



      setScheduleDate(localDate);

      setScheduleTime(localTime);



      const players = getValue(match, 'players', 'Players') || [];

      const count = Number(

        getValue(match, 'playersPerTeam', 'PlayersPerTeam') ?? 11

      );



      const toSlot = (player) => {

        const playerName = getValue(player, 'playerName', 'PlayerName') || '';

        const nameParts = playerName.trim().split(/\s+/);



        const mobileNumber =

          getValue(player, 'mobileNumber', 'MobileNumber') || '';

        const email = getValue(player, 'email', 'Email') || '';



        return {

          ...emptySlot(),

          identifier: mobileNumber || email,

          mobileNumber,

          email,

          firstName: nameParts[0] || '',

          lastName: nameParts.slice(1).join(' '),

          userId: getValue(player, 'userId', 'UserId'),

          resolved: true,

          lookupState: 'verified',

          lookupMessage: 'Existing match player',

          isNewPlayer: false,

        };

      };



      const teamA = players.filter(

        (player) => getValue(player, 'team', 'Team') === 'TeamA'

      );



      const teamB = players.filter(

        (player) => getValue(player, 'team', 'Team') === 'TeamB'

      );



      if (teamA.length !== count || teamB.length !== count) {

        throw new Error(

          'The existing roster does not match the configured squad size.'

        );

      }



      setTeamAPlayers(teamA.map(toSlot));

      setTeamBPlayers(teamB.map(toSlot));

    } catch (requestError) {

      if (!cancelled) setError(getErrorMessage(requestError));

    } finally {

      if (!cancelled) setSaving(false);

    }

  };



  loadMatch();



  return () => {

    cancelled = true;

  };

}, [isEditMode, matchId, organizerUserId]);



  const allPlayers = useMemo(



    () => [



      ...teamAPlayers.map((player, index) => ({ ...player, team: 'TeamA', slot: index + 1 })),



      ...teamBPlayers.map((player, index) => ({ ...player, team: 'TeamB', slot: index + 1 })),



    ],



    [teamAPlayers, teamBPlayers],



  );



  const updateField = (event) => {



    const { name, value } = event.target;



    if (name === 'playersPerTeam') {



      const nextCount = Number(value);



      if (!Number.isInteger(nextCount) || nextCount < MIN_PLAYERS || nextCount > MAX_PLAYERS) {



        setForm((current) => ({ ...current, playersPerTeam: value }));



        return;



      }



      const currentCount = Number(form.playersPerTeam);



      if (nextCount < currentCount) {



        const removedPlayers = [...teamAPlayers.slice(nextCount), ...teamBPlayers.slice(nextCount)];



        const hasEnteredData = removedPlayers.some(



          (player) =>



            player.identifier ||



            player.mobileNumber ||



            player.email ||



            player.firstName ||



            player.lastName ||



            player.resolved,



        );



        if (



          hasEnteredData &&



          !window.confirm(



            `Reducing the squad to ${nextCount} players per team will remove data from slots ${nextCount + 1} onward. Continue?`,



          )



        ) {



          return;



        }



      }



      const resize = (slots) =>



        nextCount > slots.length



          ? [...slots, ...makeSlots(nextCount - slots.length)]



          : slots.slice(0, nextCount);



      setTeamAPlayers(resize(teamAPlayers));



      setTeamBPlayers(resize(teamBPlayers));



    }



    setForm((current) => ({ ...current, [name]: value }));



  };



  const setTeamSlots = (team, updater) => {



    const setter = team === 'TeamA' ? setTeamAPlayers : setTeamBPlayers;



    setter((current) => updater(current));



  };



  const updateSlot = (team, index, field, value) => {



    setTeamSlots(team, (current) =>



      current.map((player, playerIndex) =>



        playerIndex === index



          ? {



              ...player,



              [field]: value,



              ...(field === 'identifier'



                ? {



                    mobileNumber: '',



                    email: '',



                    firstName: '',



                    lastName: '',



                    userId: null,



                    playerNumber: null,



                    lookupState: 'idle',



                    lookupMessage: '',



                    resolved: false,



                    isNewPlayer: false,



                  }



                : {}),



            }



          : player,



      ),



    );



  };



  const setSlot = (team, index, changes) => {



    setTeamSlots(team, (current) =>



      current.map((player, playerIndex) =>



        playerIndex === index ? { ...player, ...changes } : player,



      ),



    );



  };



  const clearSlot = (team, index) => {



    setSlot(team, index, emptySlot());



  };







  const normalizeEmail = (value) => value.trim().toLowerCase();



  const normalizeMobile = (value) => value.trim();







  const validateRosterPlayer = async (identifier, existingPlayerUserIds) => {



    try {



      const response = await apiClient.post(



        '/PlayerLookup/validate-roster-player',



        {

          createdByUserId: organizerUserId, 

          identifier,

          existingUserIds: existingPlayerUserIds,

        },



      );







      // The API returns { isValid, requiresOnboarding, player }.



      return response.data?.player ?? null;



    } catch (requestError) {



      // A 404 means no account was found, so onboarding is offered.



      if (requestError.response?.status === 404) {



        return null;



      }







      throw requestError;



    }



  };







  const handleFindPlayer = async (team, index) => {



    const players = team === 'TeamA' ? teamAPlayers : teamBPlayers;



    const player = players[index];



    const identifier = player.identifier.trim();







    if (!identifier) {



      setSlot(team, index, {



        lookupState: 'error',



        lookupMessage: 'Enter a mobile number or email address.',



        resolved: false,



      });



      return;



    }







    const isEmail = identifier.includes('@');







    if (isEmail && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(identifier)) {



      setSlot(team, index, {



        lookupState: 'error',



        lookupMessage: 'Enter a valid email address.',



        resolved: false,



      });



      return;



    }







    if (!isEmail && !/^[6-9]\d{9}$/.test(identifier)) {



      setSlot(team, index, {



        lookupState: 'error',



        lookupMessage: 'Enter a valid 10-digit Indian mobile number or email address.',



        resolved: false,



      });



      return;



    }







    // Catch repeated identifiers immediately, including identifiers that



    // have not been registered as accounts yet.



    const normalizedIdentifier = isEmail



      ? normalizeEmail(identifier)



      : normalizeMobile(identifier);







    const duplicateSlot = [



      ...teamAPlayers.map((item, slotIndex) => ({



        ...item,



        team: 'TeamA',



        index: slotIndex,



      })),



      ...teamBPlayers.map((item, slotIndex) => ({



        ...item,



        team: 'TeamB',



        index: slotIndex,



      })),



    ].find((item) => {



      if (item.team === team && item.index === index) return false;







      const otherIdentifier = item.identifier.trim();



      if (



        otherIdentifier &&



        (isEmail



          ? normalizeEmail(otherIdentifier) === normalizedIdentifier



          : normalizeMobile(otherIdentifier) === normalizedIdentifier)



      ) {



        return true;



      }







      return isEmail



        ? item.email && normalizeEmail(item.email) === normalizedIdentifier



        : item.mobileNumber &&



            normalizeMobile(item.mobileNumber) === normalizedIdentifier;



    });







    if (duplicateSlot) {



      setSlot(team, index, {



        lookupState: 'error',



        lookupMessage: 'This player identifier has already been entered in another roster slot.',



        userId: null,



        playerNumber: null,



        resolved: false,



        isNewPlayer: false,



      });



      return;



    }







    // Exclude this slot. Only send players already resolved in other slots.



    const existingPlayerUserIds = [



      ...teamAPlayers.map((item, slotIndex) => ({



        ...item,



        team: 'TeamA',



        index: slotIndex,



      })),



      ...teamBPlayers.map((item, slotIndex) => ({



        ...item,



        team: 'TeamB',



        index: slotIndex,



      })),



    ]



      .filter(



        (item) =>



          !(item.team === team && item.index === index) &&



          item.resolved &&



          item.userId,



      )



      .map((item) => item.userId);







    setSlot(team, index, {



      lookupState: 'checking',



      lookupMessage: 'Checking player account…',



      userId: null,



      playerNumber: null,



      resolved: false,



      isNewPlayer: false,



    });







    try {



      const result = await validateRosterPlayer(



        identifier,



        existingPlayerUserIds,



      );







      if (!result) {



        setSlot(team, index, {



          identifier,



          mobileNumber: isEmail ? '' : identifier,



          email: isEmail ? normalizeEmail(identifier) : '',



          firstName: '',



          lastName: '',



          userId: null,



          playerNumber: null,



          lookupState: 'new-player',



          lookupMessage:



            'No account found. Enter the player’s mobile number, email, and name to create an account.',



          resolved: false,



          isNewPlayer: false,



        });



        return;



      }







      const userId = getValue(result, 'userId', 'UserId');



      if (!userId) {



        throw new Error('The player lookup returned no user ID. Please try again.');



      }



      setSlot(team, index, {



        identifier,



        mobileNumber:



          getValue(result, 'mobileNumber', 'MobileNumber') || '',



        email: getValue(result, 'email', 'Email') || '',



        firstName: getValue(result, 'firstName', 'FirstName') || '',



        lastName: getValue(result, 'lastName', 'LastName') || '',



        userId,



        playerNumber: getValue(result, 'playerNumber', 'PlayerNumber'),



        lookupState: 'verified',



        lookupMessage: 'Player verified and added to this roster.',



        resolved: true,



        isNewPlayer: false,



      });



    } catch (requestError) {



      setSlot(team, index, {



        lookupState: 'error',



        lookupMessage: getErrorMessage(requestError),



        userId: null,



        playerNumber: null,



        resolved: false,



        isNewPlayer: false,



      });



    }



  };



  const handleOnboardPlayer = async (team, index) => {



    const player = (team === 'TeamA' ? teamAPlayers : teamBPlayers)[index];



    const mobileNumber = normalizeMobile(player.mobileNumber);



    const email = normalizeEmail(player.email);



    const firstName = player.firstName.trim();



    const lastName = player.lastName.trim();



    if (!/^[6-9]\d{9}$/.test(mobileNumber)) {



      setSlot(team, index, { lookupState: 'new-player', lookupMessage: 'Enter a valid 10-digit Indian mobile number.' });



      return;



    }



    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {



      setSlot(team, index, { lookupState: 'new-player', lookupMessage: 'Enter a valid email address.' });



      return;



    }



    if (!firstName) {



      setSlot(team, index, { lookupState: 'new-player', lookupMessage: 'First name is required.' });



      return;



    }



    const duplicateContact = [

      ...teamAPlayers.map((item, slotIndex) => ({ ...item, team: 'TeamA', index: slotIndex })),

      ...teamBPlayers.map((item, slotIndex) => ({ ...item, team: 'TeamB', index: slotIndex })),

    ].find((item) => {

      if (item.team === team && item.index === index) return false;



      return (

        (item.mobileNumber && normalizeMobile(item.mobileNumber) === mobileNumber) ||

        (item.email && normalizeEmail(item.email) === email)

      );

    });



    if (duplicateContact) {

      setSlot(team, index, {

        lookupState: 'new-player',

        lookupMessage: 'This mobile number or email is already entered in another roster slot.',

        resolved: false,

      });

      return;

    }



    setSlot(team, index, { lookupState: 'checking', lookupMessage: 'Creating player account…' });



    try {



      const response = await apiClient.post('/PlayerLookup/onboard', {



        mobileNumber,



        email,



        firstName,



        lastName: lastName || null,



      });



     const result = response.data;

const userId = getValue(result, 'userId', 'UserId');



if (!userId) {

  throw new Error(

    'The player account was created, but the response did not include a user ID. Please verify the player before continuing.'

  );

}



setSlot(team, index, {

  mobileNumber: getValue(result, 'mobileNumber', 'MobileNumber') || mobileNumber,

  email: getValue(result, 'email', 'Email') || email,

  firstName: getValue(result, 'firstName', 'FirstName') || firstName,

  lastName: getValue(result, 'lastName', 'LastName') || lastName,

  userId,

  playerNumber: getValue(result, 'playerNumber', 'PlayerNumber'),

  lookupState: 'onboarded',

  lookupMessage: 'Player account created and added to this roster.',

  resolved: true,

  isNewPlayer: true,

});



    } catch (requestError) {



      setSlot(team, index, {



        lookupState: 'new-player',



        lookupMessage: getErrorMessage(requestError),



      });



    }



  };



  const useCurrentLocation = () => {



    setError('');



    if (!navigator.geolocation) {



      setError('This browser does not support location detection. Use a device/browser with location access to create this match.');



      return;



    }



    setLocationLoading(true);



    navigator.geolocation.getCurrentPosition(



      ({ coords }) => {



        setForm((current) => ({



          ...current,



          latitude: String(coords.latitude),



          longitude: String(coords.longitude),



        }));



        setLocationLoading(false);



      },



      () => {



        setError('Could not access your location. Allow location permission and try capturing the venue location again.');



        setLocationLoading(false);



      },



      { enableHighAccuracy: true, timeout: 15000 },



    );



  };



  const validateRosterBeforeContinue = () => {

    const missingPlayer = allPlayers.find(

      (player) => !player.resolved || !player.userId,

    );



    if (missingPlayer) {

      const message = missingPlayer.lookupState === 'new-player'

        ? 'Create the player account before continuing.'

        : !missingPlayer.identifier.trim()

          ? 'Enter a mobile number or email address.'

          : 'Verify this player or complete their account registration before continuing.';



      setSlot(missingPlayer.team, missingPlayer.slot - 1, {

        // Keep the onboarding panel visible if this is an unknown player.

        lookupState: missingPlayer.lookupState === 'new-player' ? 'new-player' : 'error',

        lookupMessage: message,

        resolved: false,

      });



      return false;

    }



    const seenUserIds = new Set();

    const seenMobiles = new Set();

    const seenEmails = new Set();



    for (const player of allPlayers) {

      if (seenUserIds.has(player.userId)) {

        setSlot(player.team, player.slot - 1, {

          lookupState: 'error',

          lookupMessage: 'This player has already been added to another roster slot.',

          userId: null,

          resolved: false,

        });

        return false;

      }

      seenUserIds.add(player.userId);



      const mobile = normalizeMobile(player.mobileNumber || '');

      const email = normalizeEmail(player.email || '');



      if (mobile && seenMobiles.has(mobile)) {

        setSlot(player.team, player.slot - 1, {

          lookupState: 'error',

          lookupMessage: 'This mobile number is already used in another roster slot.',

          userId: null,

          resolved: false,

        });

        return false;

      }



      if (email && seenEmails.has(email)) {

        setSlot(player.team, player.slot - 1, {

          lookupState: 'error',

          lookupMessage: 'This email address is already used in another roster slot.',

          userId: null,

          resolved: false,

        });

        return false;

      }



      if (mobile) seenMobiles.add(mobile);

      if (email) seenEmails.add(email);

    }



    return true;

  };



  const validateStep = (stepToValidate) => {



if (!organizerUserId) {

  return 'Your user ID is missing. Please log in again.';

}

    if (stepToValidate === 1) {



      if (!form.name.trim() || !form.teamAName.trim() || !form.teamBName.trim()) {



        return 'Enter the match name and both team names.';



      }



      if (form.teamAName.trim().toLowerCase() === form.teamBName.trim().toLowerCase()) {



        return 'Team A and Team B must have different names.';



      }



    }



    if (stepToValidate === 2) {



      if (!form.scheduledAt || !form.venue.trim()) return 'Enter the match date, time, and venue.';



      if (Number.isNaN(new Date(form.scheduledAt).getTime())) return 'Enter a valid date and time.';



      if (form.latitude.trim() === '' || form.longitude.trim() === '') {



        return 'Capture the venue location before continuing.';



      }



      const selectedDateTime = new Date(form.scheduledAt);



      if (Number.isNaN(selectedDateTime.getTime())) {

        return 'Enter a valid match date and time.';

      }



if (selectedDateTime.getTime() <= Date.now()) {

  return 'Match date and time must be in the future.';

}



      const latitude = Number(form.latitude);



      const longitude = Number(form.longitude);



      if (!Number.isFinite(latitude) || latitude < -90 || latitude > 90) return 'Latitude must be between -90 and 90.';



      if (!Number.isFinite(longitude) || longitude < -180 || longitude > 180) return 'Longitude must be between -180 and 180.';



    }



    if (stepToValidate === 3) {



      const overs = Number(form.overs);



      const playersPerTeam = Number(form.playersPerTeam);



      if (!Number.isInteger(overs) || overs < MIN_OVERS || overs > MAX_OVERS) {



        return `Overs must be a whole number from ${MIN_OVERS} to ${MAX_OVERS}.`;



      }



      if (!Number.isInteger(playersPerTeam) || playersPerTeam < MIN_PLAYERS || playersPerTeam > MAX_PLAYERS) {



        return `Players per team must be between ${MIN_PLAYERS} and ${MAX_PLAYERS}.`;



      }



    }



    if (stepToValidate === 4) return '';



    return '';



  };



const handleSubmit = async (event) => {

  event.preventDefault();



  // Prevent duplicate submissions.

  if (saving) return;



  setError('');



  // Validate every form section before saving.

  const validationError = [1, 2, 3]

    .map(validateStep)

    .find(Boolean);



  if (validationError) {

    setError(

      `${validationError} Please use Back to correct the details.`

    );

    return;

  }



  // Do not navigate away from Review if the roster is invalid.

  if (!validateRosterBeforeContinue()) {

    setError(

      'The player roster is incomplete or contains duplicate players. Check the highlighted player slots before saving.'

    );

    return;

  }



  setSaving(true);



  try {

    const payload = {

      createdByUserId: organizerUserId,

      name: form.name.trim(),

      teamAName: form.teamAName.trim(),

      teamALogo: form.teamALogo.trim() || null,

      teamBName: form.teamBName.trim(),

      teamBLogo: form.teamBLogo.trim() || null,

      scheduledAt: new Date(form.scheduledAt).toISOString(),

      venue: form.venue.trim(),

      locationAddress: form.locationAddress.trim() || null,

      latitude: Number(form.latitude),

      longitude: Number(form.longitude),

      overs: Number(form.overs),

      playersPerTeam: Number(form.playersPerTeam),

      players: [

        ...teamAPlayers.map((player) => ({

          team: 'TeamA',

          mobileNumber: player.mobileNumber.trim(),

          email: player.email.trim(),

          firstName: player.firstName.trim() || null,

          lastName: player.lastName.trim() || null,

        })),

        ...teamBPlayers.map((player) => ({

          team: 'TeamB',

          mobileNumber: player.mobileNumber.trim(),

          email: player.email.trim(),

          firstName: player.firstName.trim() || null,

          lastName: player.lastName.trim() || null,

        })),

      ],

    };



const response = isEditMode

  ? await updateMatch(matchId, organizerUserId, {

      name: payload.name,

      teamAName: payload.teamAName,

      teamALogo: payload.teamALogo,

      teamBName: payload.teamBName,

      teamBLogo: payload.teamBLogo,

      scheduledAt: payload.scheduledAt,

      venue: payload.venue,

      locationAddress: payload.locationAddress,

      latitude: payload.latitude,

      longitude: payload.longitude,

      overs: payload.overs,

      playersPerTeam: payload.playersPerTeam,

      players: payload.players,

    })

  : await apiClient.post('/Match/create', payload);

    const savedMatchId = isEditMode
      ? matchId
      : getValue(response.data, 'id', 'matchId', 'Id', 'MatchId');

    if (!savedMatchId) {
      throw new Error('The match was saved, but the server did not return its match ID.');
    }

    // Both create and update return to the read-only review route.
    // The review page displays the success toast and offers Update / Dashboard actions.
    navigate(`/umpire/matches/${savedMatchId}`, {
      state: {
        toastMessage: isEditMode
          ? 'Match updated successfully!'
          : 'Match created successfully!',
      },
    });

  } catch (requestError) {

    // API failure: remain on the form and preserve all entered data.

    setError(getErrorMessage(requestError));

  } finally {

    setSaving(false);

  }

};



  const renderPlayerSlot = (team, player, index) => {



    const isCaptain = index === 0;



    return (







      <article className={`roster-slot ${player.resolved ? 'roster-slot-resolved' : ''}`} key={`${team}-${index}`}>





        <div className="roster-slot-heading">



          <div>



            <span className="roster-slot-number">Slot {index + 1}</span>



            <strong>{isCaptain ? 'Captain' : `Player ${index + 1}`}</strong>



          </div>



          {player.resolved ? (



            <span className="roster-state roster-state-good"><FaCheckCircle /> {player.isNewPlayer ? 'New account' : 'Verified'}</span>



          ) : (



            <span className="roster-state">Not added</span>



          )}



        </div>



        {player.resolved ? (



          <div className="roster-confirmed-player">



            <div className="roster-player-avatar">{(player.firstName || '?').charAt(0).toUpperCase()}</div>



            <div className="roster-player-info">



              <strong>{`${player.firstName} ${player.lastName || ''}`.trim()}</strong>



              <span>{player.mobileNumber}</span>



              <span>{player.email}</span>



              {player.playerNumber != null && <small>Player #{player.playerNumber}</small>}



            </div>



            <button

              type="button"

              className="roster-clear-button"

              onClick={() => clearSlot(team, index)}

            >

              Replace player

            </button>



          </div>



        ) : (



          <>



            <div className="roster-input-grid roster-identifier-grid">



              <label className="match-field">



                Mobile number or email



                <input



                  type="text"



                  autoComplete="off"



                  value={player.identifier}



                  placeholder="Enter mobile number or email"



                  aria-invalid={player.lookupState === 'error'}



                  onChange={(event) => updateSlot(team, index, 'identifier', event.target.value)}



                  onBlur={() => {



                    if (player.identifier.trim() && player.lookupState !== 'checking') {



                      handleFindPlayer(team, index);



                    }



                  }}



                  onKeyDown={(event) => { if (event.key === 'Enter') { event.preventDefault(); handleFindPlayer(team, index); } }}



                />



              </label>



            </div>



            {player.lookupMessage && (



              <p



                className={`roster-message ${player.lookupState === 'error' ? 'roster-message-error' : player.resolved ? 'roster-message-good' : ''}`}



                role={player.lookupState === 'error' ? 'alert' : 'status'}



              >



                {player.lookupMessage}



              </p>



            )}



            <div className="roster-actions">



              <button



                type="button"



                className="match-location-button"



                onClick={() => handleFindPlayer(team, index)}



                disabled={player.lookupState === 'checking'}



                onMouseDown={(event) => event.preventDefault()}



              >



                <FaSearch /> {player.lookupState === 'checking' ? 'Checking…' : 'Find / verify player'}



              </button>



            </div>



            {(player.lookupState === 'new-player' || (player.lookupState === 'checking' && player.lookupMessage === 'Creating player account…')) && (



              <div className="roster-onboard-panel">



                <p><strong>New player account</strong> — the backend generates a temporary password that is not shown to the umpire.</p>



                <div className="roster-input-grid">



                  <label className="match-field">



                    Mobile number *



                    <input type="tel" inputMode="numeric" maxLength={10} value={player.mobileNumber} placeholder="10-digit mobile" onChange={(event) => updateSlot(team, index, 'mobileNumber', event.target.value)} />



                  </label>



                  <label className="match-field">



                    Email address *



                    <input type="email" value={player.email} placeholder="player@example.com" onChange={(event) => updateSlot(team, index, 'email', event.target.value)} />



                  </label>



                  <label className="match-field">



                    First name *



                    <input



                      value={player.firstName}



                      maxLength={15}



                      onChange={(event) => updateSlot(team, index, 'firstName', event.target.value)}



                    />



                  </label>



                  <label className="match-field">



                    Last name



                    <input



                      value={player.lastName}



                      maxLength={15}



                      onChange={(event) => updateSlot(team, index, 'lastName', event.target.value)}



                    />



                  </label>



                </div>



                <button



                  type="button"



                  className="match-submit-button roster-onboard-button"



                  onClick={() => handleOnboardPlayer(team, index)}



                  disabled={player.lookupState === 'checking'}



                >



                  Create player account



                </button>



              </div>



            )}



          </>



        )}



      </article>



    );



  };



  return (



    <main className="match-creation-page">



      <header className="match-creation-topbar">



        <button type="button" className="match-back-button" onClick={() => navigate('/umpire')}>



          <FaArrowLeft /> Back to Dashboard



        </button>



        <span className="match-creation-kicker">CRICPULSE · UMPIRE</span>



      </header>



      <section className="match-creation-heading">

  <span className="match-creation-eyebrow">

    {isEditMode ? 'EDIT MATCH' : 'NEW MATCH'}

  </span>



  <h1>{isEditMode ? 'Update match' : 'Create a match'}</h1>



  <p>

    {isEditMode

      ? 'Edit all match details and both team rosters on this page.'

      : 'Enter all match details and both team rosters on this page, then create the match when you are ready.'}

  </p>

</section>







      <form
        className="match-creation-form"
        onSubmit={handleSubmit}
        onKeyDown={(event) => {
          if (event.key === 'Enter' && event.target.tagName !== 'TEXTAREA') {
            event.preventDefault();
          }
        }}
      >



        {error && <div className="match-form-alert error" role="alert">{error}</div>}



        {true && (



          <section className="match-form-section">



            <div className="match-form-section-title">



              <span className="match-form-icon"><FaUsers /></span>



              <div><h2>Match and teams</h2><p>Give your match and both teams a name.</p></div>



            </div>



            <div className="match-form-grid">



              <label className="match-field full-width">Match name *



                <input name="name" value={form.name} onChange={updateField} maxLength={100} placeholder="e.g. Sunday Friendly" required />



              </label>



              <label className="match-field">Team A name *



                <input name="teamAName" value={form.teamAName} onChange={updateField} maxLength={100} placeholder="e.g. Warriors" required />



              </label>



              <label className="match-field">Team B name *



                <input name="teamBName" value={form.teamBName} onChange={updateField} maxLength={100} placeholder="e.g. Strikers" required />



              </label>



              <div className="match-field logo-picker-field">



                <label htmlFor="teamALogo">Team A logo</label>



                <div className="logo-picker-control">



                  <span className={`team-logo-preview logo-${getLogo(form.teamALogo).tone}`}>{getLogo(form.teamALogo).symbol}</span>



                  <select id="teamALogo" name="teamALogo" value={form.teamALogo} onChange={updateField}>



                    {TEAM_LOGOS.map((logo) => <option key={logo.value} value={logo.value}>{logo.label}</option>)}



                  </select>



                </div>



              </div>



              <div className="match-field logo-picker-field">



                <label htmlFor="teamBLogo">Team B logo</label>



                <div className="logo-picker-control">



                  <span className={`team-logo-preview logo-${getLogo(form.teamBLogo).tone}`}>{getLogo(form.teamBLogo).symbol}</span>



                  <select id="teamBLogo" name="teamBLogo" value={form.teamBLogo} onChange={updateField}>



                    {TEAM_LOGOS.map((logo) => <option key={logo.value} value={logo.value}>{logo.label}</option>)}



                  </select>



                </div>



              </div>



            </div>



          </section>



        )}



        {true && (



          <section className="match-form-section">



            <div className="match-form-section-title">



              <span className="match-form-icon"><FaCalendarAlt /></span>



              <div><h2>Schedule and venue</h2><p>Choose when and where the match will take place.</p></div>



            </div>



            <div className="match-form-grid">





            <div className="match-schedule-fields">

              <label className="match-field">

                <span className="schedule-field-label">

                  <FaCalendarAlt />

                  Match date *

                </span>



                <input

                  className="match-date-input"

                  type="date"

                  lang="en-IN"

                  value={scheduleDate}

                  min={getTodayLocalDate()}

                  onChange={(event) => {

                    const date = event.target.value;

                    setScheduleDate(date);



                    setForm((current) => ({

                      ...current,

                      scheduledAt:

                        date && scheduleTime

                          ? `${date}T${scheduleTime}`

                          : '',

                    }));

                  }}

                  required

                />



                <small>Choose the match date.</small>

              </label>



              <label className="match-field">

                <span className="schedule-field-label">

                  <FaCalendarAlt />

                  Match time *

                </span>



                <input

                  className="match-time-input"

                  type="time"

                  value={scheduleTime}

                  onChange={(event) => {

                    const time = event.target.value;

                    setScheduleTime(time);



                    setForm((current) => ({

                      ...current,

                      scheduledAt:

                        scheduleDate && time

                          ? `${scheduleDate}T${time}`

                          : '',

                    }));

                  }}

                  required

                />



                <small>Choose the match start time.</small>

              </label>

            </div>





              <label className="match-field">Venue *



                <input name="venue" value={form.venue} onChange={updateField} maxLength={200} placeholder="Ground or stadium name" required />



              </label>



              <label className="match-field full-width">Full address (optional)



                <input name="locationAddress" value={form.locationAddress} onChange={updateField} maxLength={500} placeholder="Area, city, state, PIN code" />



              </label>



              <div className={`match-location-panel full-width ${locationCaptured ? 'is-captured' : ''}`}>



                <div className="match-location-action">
                  <button
                    type="button"
                    className={`match-location-button ${locationCaptured ? 'location-captured-button' : ''}`}
                    onClick={useCurrentLocation}
                    disabled={locationLoading || locationCaptured}
                  >
                    {locationCaptured ? <FaCheckCircle /> : <FaMapMarkerAlt />}
                    {locationLoading ? 'Capturing location…' : locationCaptured ? 'Location captured' : 'Capture venue location'}
                  </button>
                  <small className="match-location-hint">
                    {locationCaptured
                      ? 'Coordinates saved for this match. Latitude and longitude are shown below.'
                      : 'Stand at the venue and allow browser location access to capture its GPS coordinates. Manual coordinates are not supported.'}
                  </small>
                </div>



                {locationCaptured && (



                  <div className="captured-coordinates" aria-live="polite">



                    <div><span>Latitude</span><strong>{Number(form.latitude).toFixed(6)}°</strong></div>



                    <div><span>Longitude</span><strong>{Number(form.longitude).toFixed(6)}°</strong></div>



                    <span className="location-captured-status"><FaCheckCircle /> Location captured</span>



                  </div>



                )}



              </div>



            </div>



            <p className="match-info-note">The backend uses the captured GPS coordinates to resolve the Indian state and PIN code. Capture the location while you are at the venue.</p>



          </section>



        )}



        {true && (



          <section className="match-form-section">



            <div className="match-form-section-title">



              <span className="match-form-icon"><FaShieldAlt /></span>



              <div><h2>Match settings</h2><p>Choose match length and squad size.</p></div>



            </div>



            <div className="match-form-grid">



              <label className="match-field">Overs per innings *



                <select name="overs" value={form.overs} onChange={updateField} required>



                  {Array.from({ length: MAX_OVERS - MIN_OVERS + 1 }, (_, index) => index + MIN_OVERS).map((overs) => (



                    <option key={overs} value={overs}>{overs} {overs === 1 ? 'over' : 'overs'}</option>



                  ))}



                </select>



                <small>Allowed: {MIN_OVERS}–{MAX_OVERS} overs.</small>



              </label>



              <label className="match-field">Players per team *



                <select name="playersPerTeam" value={form.playersPerTeam} onChange={updateField} required>



                  {Array.from({ length: MAX_PLAYERS - MIN_PLAYERS + 1 }, (_, index) => index + MIN_PLAYERS).map((count) => (



                    <option key={count} value={count}>{count} players</option>



                  ))}



                </select>



                <small>Allowed: {MIN_PLAYERS}–{MAX_PLAYERS}. The roster updates to this exact number.</small>



              </label>



            </div>



            <p className="match-info-note">Each team’s first roster slot is the captain. If you reduce the squad size, the form asks before removing any filled slots.</p>



          </section>



        )}



        {true && (



          <section className="match-form-section">



            <div className="match-form-section-title">



              <span className="match-form-icon"><FaUsers /></span>



              <div>



                <h2>Player roster</h2>



                <p>Verify registered players or onboard new players. All {Number(form.playersPerTeam) * 2} slots must be complete.</p>



              </div>



            </div>



            <div className="roster-teams-grid">



              <div className="roster-team-section">



                <div className="roster-team-heading"><h3>{form.teamAName || 'Team A'}</h3><span>{teamAPlayers.filter((player) => player.resolved).length}/{teamAPlayers.length} ready</span></div>



                {teamAPlayers.map((player, index) => renderPlayerSlot('TeamA', player, index))}



              </div>



              <div className="roster-team-section">



                <div className="roster-team-heading"><h3>{form.teamBName || 'Team B'}</h3><span>{teamBPlayers.filter((player) => player.resolved).length}/{teamBPlayers.length} ready</span></div>



                {teamBPlayers.map((player, index) => renderPlayerSlot('TeamB', player, index))}



              </div>



            </div>



            <p className="match-info-note">Clearing a player leaves the slot in place. Players are never shifted into another slot. The first slot of each team is the captain.</p>



          </section>



        )}



        <footer className="match-form-footer match-form-footer-single">
          <button
            type="button"
            className="match-cancel-button"
            onClick={() => navigate(isEditMode ? `/umpire/matches/${matchId}` : '/umpire')}
            disabled={saving}
          >
            Cancel
          </button>
          <button type="submit" className="match-submit-button" disabled={saving || (isEditMode && !form.name)}>
            {saving ? (isEditMode ? 'Saving changes…' : 'Creating match…') : (isEditMode ? 'Update Match' : 'Create Match')}
          </button>
        </footer>



      </form>



    </main>



  );



}



export default MatchCreation;
