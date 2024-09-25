import React, { useState, useEffect, useRef } from 'react';
import './Game.css';
const App = () => {
  const [messages, setMessages] = useState([]);
  const [rooms, setRooms] = useState([]);
  const [place, setPlace] = useState('Lobby');
  const [myPlayerId, setMyPlayerId] = useState();
  const [gameState, setGameState] = useState(null);
  const [timer, setTimer] = useState(null);
  const ws = useRef(null);

  useEffect(() => {
    ws.current = new WebSocket('ws://localhost:7777/ws/');
    ws.current.onmessage = (event) => {
      const data = JSON.parse(event.data);
      if (data.MessageType===1) {
        setRooms(data.Rooms);
        setPlace('Lobby')
      }
      else if(data.MessageType===2){
        setRooms([]);
        const place = "Room " + data.RoomId
        setPlace(place)
        setGameState(data)
        setTimer(null);
      }
      else if(data.MessageType===3){
        setMyPlayerId(data.PlayerId)
      }
      else if (data.MessageType === 4) {
        setTimer(data.LeftSeconds);
      }
      setMessages((prevMessages) => [...prevMessages, event.data]);
    };

    return () => {
      if (ws.current) {
        ws.current.close();
      }
    };
  }, []);
  const sendMessage = (message)=>{
    if ( ws.current.readyState === WebSocket.OPEN) {
      ws.current.send(message);
      console.log(`Message sent: ${message}`);
    }
  }
  const getImageFileName = (tile)=>{
    let fileName=""
    if(tile.TileType>3){
      switch (tile.TileType) {
        case 4:
          fileName = "RedCenter"
          break;
        case 5:
          fileName = "EarnMoney"
          break;
        case 6:
          fileName = "WhiteBoard"
          break;
        case 7:
          fileName = "EastWind"
          break;
        case 8:
          fileName = "SouthWind"
          break;
        case 9:
          fileName = "WestWind"
          break;
        case 10:
          fileName = "NorthWind"
          break;
        default:
          break;
      }
    }
    else{
        switch (tile.TileType) {
          case 1:
            fileName = `One${tile.TileNumber}`
            break;
          case 2:
            fileName = `Tiao${tile.TileNumber}`
            break;
          case 3:
            fileName = `Tong${tile.TileNumber}`
            break;
          default:
            break;
        }
        // console.log(tile);
        // console.log(fileName);
        
        
    }
    return fileName
  }
  const renderAvailableActions = (availableActions) => {
    return (
      <div>
        {availableActions.includes(1) && (
          // todo: send ??
          <button onClick={() => sendMessage('4|')}>Eat</button>
        )}
        {availableActions.includes(2) && (
          <button onClick={() => sendMessage('5|')}>Pong</button>
        )}
      </div>
    );
  };
  const renderHandTiles = (player) => {
    return (
      <div>
        {/* Show EatOrPongTiles for the current player */}
        <div className="player-eat-pong-tiles" style={{ display: 'flex', marginBottom: '5px' }}>
          {player.EatOrPongTiles && player.EatOrPongTiles.map((tile, index) => (
            <img
              key={index}
              src={`/images/${getImageFileName(tile)}.png`}
              alt={`EatOrPongTile ${tile.TileType}-${tile.TileNumber}`}
              style={{ width: '40px', height: '60px', margin: '2px' }}
            />
          ))}
        </div>
        
        {/* Player's hand tiles */}
        <div style={{ display: 'flex' }}>
          {player.HandTiles.map((tile, index) => (
            <img
              key={index}
              src={`/images/${getImageFileName(tile)}.png`}
              style={{
                width: '50px',
                height: '75px',
                margin: '2px',
                cursor: player.IsThisPlayerTurn ? 'pointer' : 'default',
                opacity: player.IsThisPlayerTurn ? 1 : 0.6,
              }}
              onClick={() => {
                if (player.IsThisPlayerTurn) {
                  const message = `1|${tile.TileType}|${tile.TileNumber}`;
                  sendMessage(message);
                }
              }}
              alt={`Tile ${tile.TileType}-${tile.TileNumber}`}
            />
          ))}
        </div>
      </div>
    );
  };

  const renderOtherPlayerHand = (player, position) => {
    let hiddenTileImage;
    
    // Choose the appropriate hidden tile image based on the player's position
    switch (position) {
      case 'left':
        hiddenTileImage = '/images/HiddenTileLeft.png';
        break;
      case 'top':
        hiddenTileImage = '/images/HiddenTileOppisite.png';
        break;
      case 'right':
        hiddenTileImage = '/images/HiddenTileRight.png';
        break;
      default:
        hiddenTileImage = '/images/HiddenTileLeft.png'; // Default case if position is undefined
    }
  
    return (
      <div>
        {/* <h4>Player {player.Seat}</h4> */}
              {/* Show the player's EatOrPongTiles */}
      <div className={`player-eat-pong-tiles ${position}`} style={{ display: 'flex', flexDirection: position === 'top' ? 'row' : 'column' }}>
        {player.EatOrPongTiles && player.EatOrPongTiles.map((tile, index) => (
          <img
            key={index}
            src={`/images/${getImageFileName(tile)}.png`}
            alt={`EatOrPongTile ${tile.TileType}-${tile.TileNumber}`}
            style={{
              width: '40px', // Smaller size for EatOrPongTiles
              height: '60px',
              margin: '2px',
            }}
          />
        ))}
      </div>
        <div className={`player-tiles ${position}`} style={{ display: 'flex', flexDirection: position === 'top' ? 'row' : 'column' }}>
          {player.HandTiles.map((_, index) => (
            <img
              key={index}
              src={hiddenTileImage}
              alt={`Hidden tile for ${position}`}
              style={{
                width: position === 'top' ? '50px' : '75px', // Adjust size based on orientation
                height: position === 'top' ? '75px' : '50px',
                margin: '2px',
              }}
            />
          ))}
        </div>
      </div>
    );
  };
  const renderLoading = () =>{
    return <div>Loading...</div>;
  }
  const renderSentTiles = (sentTiles) => {
    return sentTiles.map((tile, index) => (
      <img
        key={index}
        src={`/images/${getImageFileName(tile)}.png`}
        style={{ width: '30px', height: '50px', margin: '2px' }}
        alt={`Sent Tile ${tile.TileType}-${tile.TileNumber}`}
      />
    ));
  };
  const renderTable = () => {
    const currentPlayer = gameState.PlayersInfo.find(player => player.PlayerId === myPlayerId);
    const playerBySeat = {};
    gameState.PlayersInfo.forEach((player) => {
      playerBySeat[player.Seat] = player;
    });
  
    const seatOrder = {
      left: playerBySeat[(currentPlayer.Seat + 2) % 4 + 1] || null,
      top: playerBySeat[(currentPlayer.Seat + 1) % 4 + 1] || null,
      right: playerBySeat[(currentPlayer.Seat) % 4 + 1] || null,
    };
  
    return (
      <div className="game-container">
        {/* Top player */}
        <div className="top-player">
          {seatOrder.top && renderOtherPlayerHand(seatOrder.top, 'top')}
        </div>

        {/* Left player */}
        <div className="left-player">
          {seatOrder.left && renderOtherPlayerHand(seatOrder.left, 'left')}
        </div>

        {/* Right player */}
        <div className="right-player">
          {seatOrder.right && renderOtherPlayerHand(seatOrder.right, 'right')}
        </div>

        {/* Center (played tiles area) */}
        <div className="center">
          <div className="sent-tiles-container">
            {/* Render each player's sent tiles */}
            <div className="sent-tiles top">{renderSentTiles(seatOrder.top.SentTiles || [])}</div>
            <div className="sent-tiles left">{renderSentTiles(seatOrder.left.SentTiles || [])}</div>
            <div className="sent-tiles right">{renderSentTiles(seatOrder.right.SentTiles || [])}</div>
            <div className="sent-tiles bottom">{renderSentTiles(currentPlayer.SentTiles)}</div>
          </div>
          {timer !== null && (
            <div className="timer">
              <p>Time Left: {timer} seconds</p>
            </div>
          )}
        </div>

        {/* Bottom (current player) */}
        <div className="bottom-player">
        {/* {renderHandTiles(currentPlayer.HandTiles, currentPlayer.IsThisPlayerTurn)} */}
        {renderHandTiles(currentPlayer)}
        {renderAvailableActions(currentPlayer.AvailableActions)}
        </div>
      </div>
    );
  };


  const createRoom = () => {
    if ( ws.current.readyState === WebSocket.OPEN) {
      const msg = "1|";
      ws.current.send(msg);
      console.log(`Message sent: ${msg}`);
    }
  };
  const joinRoom = (room) => {
    const message = `2|${room}`; 
    if (ws.current && ws.current.readyState === WebSocket.OPEN) {
      ws.current.send(message); // Send the message via WebSocket
      console.log(`Message sent: ${message}`);
    } else {
      console.log('WebSocket is not open');
    }
  };

  return (
    <div style={{ padding: '20px' }}>
      <div style={{ marginBottom: '10px' }}>
        <h1>Your ID: {myPlayerId}; Place: {place}</h1>
        <button onClick={createRoom}>Create Room</button>
      </div>
      <div>
      <h1>Majon Game</h1>
      {gameState?gameState.PlayersInfo?renderTable():renderLoading():renderLoading()}
      </div>
      <div style={{ padding: '20px' }}>
      <h1>Room List</h1>
      <div style={{ display: 'flex', gap: '10px', flexWrap: 'wrap' }}>
        {rooms.map((room, index) => (
          <div
            key={index}
            onClick={() => joinRoom(room)}
            onMouseEnter={(e) => (e.target.style.backgroundColor = '#45a049')}
            onMouseLeave={(e) => (e.target.style.backgroundColor = '#4CAF50')}
          >
            Room {room}
          </div>
        ))}
      </div>
    </div>
      <div>
        <h2>Received Messages</h2>
        <div style={{ border: '1px solid black', padding: '10px', height: '200px', overflowY: 'scroll' }}>
          {messages.length > 0 ? (
            messages.map((msg, index) => <p key={index}>{msg}</p>)
          ) : (
            <p>No messages received yet.</p>
          )}
        </div>
      </div>
    </div>
  );
};

export default App;