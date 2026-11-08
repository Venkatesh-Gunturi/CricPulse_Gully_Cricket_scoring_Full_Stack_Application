
import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ||
  'https://localhost:7066/api';

const HUB_URL = `${API_BASE_URL.replace(/\/api\/?$/, '')}/hubs/match`;

let connection = null;
let startPromise = null;

const joinedMatchIds = new Set();

export const createMatchHubConnection = () => {
  if (connection) return connection;

  connection = new HubConnectionBuilder()
    .withUrl(HUB_URL)
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Information)
    .build();

  connection.onreconnected(async () => {
    for (const matchId of joinedMatchIds) {
      try {
        await connection.invoke('JoinMatch', matchId);
      } catch (error) {
        console.error(
          `Failed to rejoin match ${matchId}:`,
          error
        );
      }
    }
  });

  return connection;
};

export const startMatchHubConnection = async () => {
  const hub = createMatchHubConnection();

  if (hub.state === HubConnectionState.Connected) {
    return hub;
  }

  if (startPromise) return startPromise;

  startPromise = (async () => {
    if (hub.state === HubConnectionState.Disconnected) {
      await hub.start();
    }

    return hub;
  })();

  try {
    return await startPromise;
  } finally {
    startPromise = null;
  }
};

export const onMatchStateUpdated = (callback) => {
  const hub = createMatchHubConnection();

  hub.on('MatchStateUpdated', callback);

  return () => {
    hub.off('MatchStateUpdated', callback);
  };
};

export const joinMatchGroup = async (matchId) => {
  const id = String(matchId);

  if (joinedMatchIds.has(id)) return;

  const hub = await startMatchHubConnection();

  await hub.invoke('JoinMatch', id);
  joinedMatchIds.add(id);
};

export const leaveMatchGroup = async (matchId) => {
  const id = String(matchId);

  if (!joinedMatchIds.has(id)) return;

  joinedMatchIds.delete(id);

  if (connection?.state === HubConnectionState.Connected) {
    await connection.invoke('LeaveMatch', id);
  }
};
