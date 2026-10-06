import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5104/realtime/orders")
  .withAutomaticReconnect()
  .build();

export const startSignalRConnection = async () => {
  try {
    if (
      connection.state ===
      signalR.HubConnectionState.Disconnected
    ) {
      await connection.start();

      console.log("SignalR Connected");
    }
  } catch (err) {
    console.error(
      "SignalR Connection Error:",
      err
    );

    setTimeout(startSignalRConnection, 5000);
  }
};

export const subscribeToOrderUpdates = (
  callback: () => void
) => {
  connection.off("OrderPlaced");

  connection.on("OrderPlaced", () => {
    console.log(
      "Realtime order update received"
    );

    callback();
  });
};

connection.onreconnected(() => {
  console.log("SignalR Reconnected");
});

connection.onclose(() => {
  console.log("SignalR Closed");

  setTimeout(startSignalRConnection, 5000);
});

export default connection;