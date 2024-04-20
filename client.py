import pygame
from network import Network
import json

class Tile:
    def __init__(self, type, number, image):
        self.type = type  # Character, Bamboo, etc.
        self.number = number
        self.image = image
        self.expose = False


def load_tiles():
    types = ["one", 'stick', 'bing']
    other_types = ['east', 'south', 'west', 'north', 'central', 'fa', 'whiteboard']
    numbers = [i for i in range(1, 10)]
    # tiles = [Tile(tp, num, pygame.image.load(f"images/{tile}.png")) for tile in tiles]
    tiles = [Tile(tp, num, pygame.image.load(f"images/{tp}{num}.png")) for tp in types for num in numbers]
    tiles += [Tile(tp, 0, pygame.image.load(f"images/{tp}.png")) for tp in other_types]
    # tiles *= 4
    adjust_image_size(tiles)

    return tiles

def adjust_image_size(tiles):
    IMAGE_SIZE = (30, 50)
    for tile in tiles:
        tile.image = pygame.transform.scale(tile.image, IMAGE_SIZE)

    return tiles

def init_game():
    
    pygame.init()
    SCREEN_WIDTH = 800
    SCREEN_HEIGHT = int(SCREEN_WIDTH*0.8)
    screen = pygame.display.set_mode((SCREEN_WIDTH, SCREEN_HEIGHT))
    pygame.display.set_caption("Majon")

    WHITE = (255, 255, 255)
    screen.fill(WHITE)
    return screen

def main():


    screen = init_game()
    clock = pygame.time.Clock()
    tiles_obj = load_tiles()
    n = Network()
    print(f"I am {n.playerId}")
    # print(n.fetch_game_info())
    
    next_player_tile_image = pygame.image.load('images/tile_rightside.png')
    opposit_player_tile_image = pygame.image.load('images/tile_back.png')
    last_player_tile_image = pygame.image.load('images/tile_leftside.png')
    IMAGE_SIZE = (30, 50)
    next_player_tile_image = pygame.transform.scale(next_player_tile_image, IMAGE_SIZE)
    opposit_player_tile_image = pygame.transform.scale(opposit_player_tile_image, IMAGE_SIZE)
    last_player_tile_image = pygame.transform.scale(last_player_tile_image, IMAGE_SIZE)

    run = True
    while run:
        # print("run")
        clock.tick(1)
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                run = False

        player = {'action': f"player{n.playerId} waiting"}

        game_info = n.send(data=player)
        # print(game_info)

        if game_info['game_running']:
            player_index = game_info['players'].index(n.playerId)
            
            player_seat_direction = game_info['seats'][f"{n.playerId}"]
            directions = ['east', 'south', 'west', 'north']
            direction_index = directions.index(player_seat_direction)
            directions = directions[direction_index:] + directions[:direction_index]
            next_seat_direction = directions[1]
            opposit_seat_direction = directions[2]
            last_seat_direction = directions[3]
            print(player_seat_direction, next_seat_direction, opposit_seat_direction, last_seat_direction)

            for player, direction in game_info['seats'].items():
                print(player, direction)
                if direction == next_seat_direction:
                    next_player = int(player)
                elif direction == opposit_seat_direction:
                    opposit_player = int(player)
                elif direction == last_seat_direction:
                    last_player = int(player)
                else:
                    print('myself',player, direction)
                    # raise Exception("not mapping")




            tiles = [obj for obj in tiles_obj for tile in game_info['tiles_of_players'][player_index] if json.loads(tile)['type']==obj.type and json.loads(tile)['number']==obj.number]
            rest_tiles = [obj for obj in tiles_obj for tile in game_info['rest_tiles'] if json.loads(tile)['type']==obj.type and json.loads(tile)['number']==obj.number]
            display_myself(screen, tiles)
            display_hidden_tiles(screen, rest_tiles)
            disply_next_player(screen, len(game_info['tiles_of_players'][game_info['players'].index(next_player)]), next_player_tile_image)
            display_oppisit_player(screen, len(game_info['tiles_of_players'][game_info['players'].index(opposit_player)]), opposit_player_tile_image)
            display_last_player(screen, len(game_info['tiles_of_players'][game_info['players'].index(last_player)]), last_player_tile_image)


            
        refreshScreen()


def refreshScreen():
    pygame.display.flip()



def display_hidden_tiles(screen, tiles):
    newline = 3
    weight = 100

    for i, tile in enumerate(tiles):
        if i%16==0:
            newline +=1
            weight = 100
        weight += 30
        screen.blit(tile.image, (weight, newline*50))

def display_myself(screen, tiles):
    weight = 130
    for tile in tiles:
        screen.blit(tile.image, (weight, 580))
        weight += 30

def disply_next_player(screen, len_tiles, image):
    
    for i in range(len_tiles):
        height = 90 + i*30
        screen.blit(image, (700, height))

def display_oppisit_player(screen, len_tiles, image):
    for i in range(len_tiles):
        weight = 130 + i*30
        screen.blit(image, (weight, 30))

def display_last_player(screen, len_tiles, image):
    for i in range(len_tiles):
        height = 90 + i*30
        screen.blit(image, (30, height))


main()
pygame.quit()