import React, { useState, useEffect, useRef } from 'react';
import './Game.css';
const App = () => {
  const [messages, setMessages] = useState([]);
  const [rooms, setRooms] = useState([]);
  const [place, setPlace] = useState('Lobby');
  const [myPlayerId, setMyPlayerId] = useState();
  const [gameState, setGameState] = useState(null);
  const [timer, setTimer] = useState(null);
  const [winnerPlayerId, setWinnerPlayerId] = useState(null); 
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
      else if (data.MessageType === 5) {
        // 新增 WinnerPlayerId 的處理邏輯
        setWinnerPlayerId(data.WinnerPlayerId);
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
        {/* {availableActions.includes(1) && (
          <button onClick={() => sendMessage('4|')}>吃Eat</button>
        )} */}
        {availableActions.includes(2) && (
          <button onClick={() => sendMessage('5|')}>碰Pong</button>
        )}
        {availableActions.includes(3) && (
          <button onClick={() => sendMessage('6|')}>槓Gang</button>
        )}
        {availableActions.includes(4) && (
          <button onClick={() => sendMessage('3|')}>胡WIN!</button>
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
        {/* Show the player's EatOrPongTiles */}
        <div className={`player-eat-pong-tiles ${position}`} style={{ display: 'flex', flexDirection: position === 'top' ? 'row' : 'column' }}>
          {player.EatOrPongTiles && player.EatOrPongTiles.map((tile, index) => (
            <img
              key={index}
              src={`/images/${getImageFileName(tile)}.png`}
              alt={`EatOrPongTile ${tile.TileType}-${tile.TileNumber}`}
              style={{
                width: '40px',
                height: '60px',
                margin: '2px',
              }}
            />
          ))}
        </div>
  
        {/* Conditionally show the player's hand tiles (revealed if there's a winner) */}
        <div className={`player-tiles ${position}`} style={{ display: 'flex', flexDirection: position === 'top' ? 'row' : 'column' }}>
          {player.HandTiles.map((tile, index) => (
            <img
              key={index}
              src={winnerPlayerId ? `/images/${getImageFileName(tile)}.png` : hiddenTileImage}  // Show actual tiles if there's a winner
              alt={`Tile ${tile.TileType}-${tile.TileNumber}`}
              style={{
                width: position === 'top' ? '50px' : '75px',
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

        <div className="center">
          {winnerPlayerId && (
            <div className="winner-display">
              <h2>Player {winnerPlayerId} Wins!!</h2>
            </div>
          )}

          <div className="sent-tiles-container">

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

        <div className="bottom-player">
        {renderHandTiles(currentPlayer)}
        {renderAvailableActions(currentPlayer.AvailableActions)}
        {currentPlayer.EatOptions && currentPlayer.EatOptions.length > 0 && renderEatOptions(currentPlayer.EatOptions)}
        </div>
      </div>
    );
  };
  const renderEatOptions = (eatOptions) => {
    if (!eatOptions || eatOptions.length === 0) return null;
  
    return (
      <div className="eat-options-container">
        <h4>Choose an Eat Option</h4>
        <div className="eat-options">
          {eatOptions.map((option, index) => (
            <div key={index} className="eat-option" style={{ display: 'flex', cursor: 'pointer' }}>
              {option.map((tile, tileIndex) => (
                <img
                  key={tileIndex}
                  src={`/images/${getImageFileName(tile)}.png`}
                  alt={`EatOption Tile ${tile.TileType}-${tile.TileNumber}`}
                  style={{ width: '50px', height: '75px', margin: '2px' }}
                  onClick={() => handleEatOptionClick(option)}
                />
              ))}
            </div>
          ))}
        </div>
      </div>
    );
  };
  const handleEatOptionClick = (option) => {
    if (option.length === 2) {
      const [tile1, tile2] = option;
      const message = `4|${tile1.TileType}|${tile1.TileNumber}|${tile2.TileType}|${tile2.TileNumber}`;
      sendMessage(message);
      console.log(`Eat option selected: ${message}`);
    }
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
        {place=="Lobby"?<button onClick={createRoom}>Create Room</button>:<h1></h1>}
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