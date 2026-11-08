
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
    .withAutomaticReconnect({
  nextRetryDelayInMilliseconds: (retryContext) => {
    return Math.min(
      1000 * Math.pow(2, retryContext.previousRetryCount),
      10000
    );
  },
})
    .configureLogging(LogLevel.Information)
    .build();

    connection.onreconnecting((error) => {
  console.warn('[SignalR] Reconnecting...', error);
});

connection.onreconnected(async (connectionId) => {
  console.log('[SignalR] Reconnected:', connectionId);

  for (const matchId of joinedMatchIds) {
    try {
      await connection.invoke('JoinMatch', matchId);
      console.log('[SignalR] Rejoined match:', matchId);
    } catch (error) {
      console.error(
        `[SignalR] Failed to rejoin match ${matchId}:`,
        error
      );
    }
  }
});

connection.onclose((error) => {
  console.error('[SignalR] Connection closed:', error);
});

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
