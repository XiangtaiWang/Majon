import socket
import json
import pickle
class Network:
    def __init__(self):
        self.playerId = 0
        self.client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.server = "100.92.111.210"
        self.port = 7777
        self.addr = (self.server, self.port)
        # print("---before connect")
        self.data = self.connect()
        # print("---after connect")
        

    def connect(self):
        try:
            self.client.connect(self.addr)
            data = self.client.recv(2048*4).decode()
            self.playerId = int(data)
    
            return data
        except socket.error as e:
            print(e)
    def fetch_game_info(self):
        
        return json.loads(self.client.recv(2048*4))
    
    def send(self, data):
        try:
            self.client.send(str.encode(json.dumps(data)))
            return json.loads(self.client.recv(2048*4).decode())
        except socket.error as e:
            print(e)

