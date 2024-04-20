import socket
from _thread import *
import sys
import random
import json
import pickle
from json import JSONEncoder

server = "100.92.111.210"
port = 7777

s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

try:
    s.bind((server, port))

except socket.error as e:
    print(str(e))

s.listen(4)
print("waiting connection...")

class Tile:
    def __init__(self, type, number):
        self.type = type 
        self.number = number
        # should only show only eat, pong, self's tiles, now for debug
        # self.expose = True
        
class TileEncoder(JSONEncoder):# make class serializable
        def default(self, o):
            return o.__dict__
        
def random_seats(players):
    directions = ['east', 'south', 'west', 'north']
    # players = [i for i in range(1, 5)]
    random.shuffle(directions)
    random.shuffle(players)
    
    seats = {}
    for index, player_id in enumerate(players):
        seats[f'{player_id}'] = directions[index]
    return seats

def init_tiles():
    types = ["one", 'stick', 'bing']
    other_types = ['east', 'south', 'west', 'north', 'central', 'fa', 'whiteboard']

    tiles = ([Tile(tp, i)for tp in types for i in range(1, 10)]+ ([Tile(tp, 0) for tp in other_types])) * 4
    tiles = [TileEncoder().encode(obj) for obj in tiles]
    return tiles



def allocate_tiles_to_players(tiles):
    random.shuffle(tiles)
    tiles_of_players = [[] for i in range(4)]

    for i in range(4):
        for j in range(4):
            for k in range(4):
                tiles_of_players[j].append(tiles.pop())
    return tiles_of_players, tiles

def threaded_client(conn, currentPlayerId):
    # conn.send(str.encode(f"player{currentPlayerId} connected"))
    global game
    msg = json.dumps(game)
    # print(msg)
    # print("Sending: ", len(msg), msg)
    # conn.sendall(str.encode(msg))
    conn.sendall(str.encode(str(currentPlayerId)))
    while True:
        try:
            data = conn.recv(2048*4)
            # action = data.decode("utf-8")
            # process action

            msg = json.dumps(game)
            
            if not data:
                print("Disconnected")
                break
            # else:
            #     print("Received: ", data.decode("utf-8"))
                # print("Sending: ", len(msg))


            conn.sendall(str.encode(msg))
            
        except socket.error as e:
            print(e)
            break
    print("Lost connection")
    conn.close()
    game['players'].remove(currentPlayerId)


currentPlayerId = 1
current_players = []
while True:
    conn, addr = s.accept()
    print("connected to: ", addr)
    current_players.append(currentPlayerId)
    game = {
        'game_running': False,
        'players': current_players,
        'seats': [],
        'tiles_of_players':[],
        'rest_tiles':[]
    }
    if len(current_players) == 4:
        all_tiles = init_tiles()
        game['game_running'] = True
        game['seats'] = random_seats(current_players)
        game['tiles_of_players'], game['rest_tiles'] = allocate_tiles_to_players(all_tiles)
        print(game)
    start_new_thread(threaded_client, (conn, currentPlayerId))
    currentPlayerId += 1
    # print(currentPlayerId)