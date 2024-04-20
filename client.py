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

    tiles = [Tile(tp, num, pygame.image.load(f"images/{tp}{num}.png")) for tp in types for num in numbers]
    tiles += [Tile(tp, 0, pygame.image.load(f"images/{tp}.png")) for tp in other_types]

    for tile in tiles:
        tile.image = adjust_image_size(tile.image)

    return tiles

def adjust_image_size(image):
    IMAGE_SIZE = (30, 50)
    return pygame.transform.scale(image, IMAGE_SIZE)

def init_game():
    
    pygame.init()
    SCREEN_WIDTH = 800
    SCREEN_HEIGHT = int(SCREEN_WIDTH*0.8)
    screen = pygame.display.set_mode((SCREEN_WIDTH, SCREEN_HEIGHT))
    pygame.display.set_caption("Majon")

    WHITE = (255, 255, 255)
    screen.fill(WHITE)

    return screen

def load_sides_tiles():
    next_player_tile_image = adjust_image_size(pygame.image.load('images/tile_rightside.png'))
    opposit_player_tile_image = adjust_image_size(pygame.image.load('images/tile_back.png'))
    last_player_tile_image = adjust_image_size(pygame.image.load('images/tile_leftside.png'))
    hidden_side_tiles = adjust_image_size(pygame.image.load('images/tile_hidden_side.png'))
    return next_player_tile_image, opposit_player_tile_image, last_player_tile_image, hidden_side_tiles 

def refreshScreen():
    pygame.display.flip()

def get_my_and_hidden_tile_obj(mytiles, hidden_tiles, tiles_obj):
    return get_my_tile_obj(mytiles, tiles_obj), get_hidden_tile_obj(hidden_tiles, tiles_obj)

def get_my_tile_obj(mytiles, tiles_obj):
    return [obj for obj in tiles_obj for tile in mytiles if json.loads(tile)['type']==obj.type and json.loads(tile)['number']==obj.number]

def get_hidden_tile_obj(hidden_tiles, tiles_obj):
    return [obj for obj in tiles_obj for tile in hidden_tiles if json.loads(tile)['type']==obj.type and json.loads(tile)['number']==obj.number]

def get_players_seat(my_direction, seats):
    directions = ['east', 'south', 'west', 'north']
    direction_index = directions.index(my_direction)
    directions_start_from_myself = directions[direction_index:] + directions[:direction_index]
    next_seat_direction = directions_start_from_myself[1]
    opposit_seat_direction = directions_start_from_myself[2]
    last_seat_direction = directions_start_from_myself[3]

    for player, direction in seats.items():
        if direction == next_seat_direction:
            next_player = int(player)
        elif direction == opposit_seat_direction:
            opposit_player = int(player)
        elif direction == last_seat_direction:
            last_player = int(player)
    
    return next_player, opposit_player, last_player

def display_hidden_tiles(screen, len_tiles, image):
    newline = 3
    weight = 100

    for i in range(len_tiles):
        if i%16==0:
            newline +=1
            weight = 100
        weight += 30
        screen.blit(image, (weight, newline*50))

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

def main():
    screen = init_game()
    clock = pygame.time.Clock()
    tiles_obj = load_tiles()

    n = Network()
    print(f"I am {n.playerId}")
    
    next_player_tile_image, opposit_player_tile_image, last_player_tile_image, hidden_tile_image = load_sides_tiles()

    run = True
    while run:
        clock.tick(1)
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                run = False

        player = {'action': f"player{n.playerId} waiting"}
        game_info = n.send(data=player)

        if game_info['game_running']:
            player_index = game_info['players'].index(n.playerId)

            next_player, opposit_player, last_player = get_players_seat(game_info['seats'][f"{n.playerId}"], game_info['seats'])

            my_tiles, hidden_tiles = get_my_and_hidden_tile_obj(game_info['tiles_of_players'][player_index], game_info['rest_tiles'], tiles_obj)
     
            display_myself(screen, my_tiles)
            display_hidden_tiles(screen, len(hidden_tiles), hidden_tile_image)
            disply_next_player(screen, len(game_info['tiles_of_players'][game_info['players'].index(next_player)]), next_player_tile_image)
            display_oppisit_player(screen, len(game_info['tiles_of_players'][game_info['players'].index(opposit_player)]), opposit_player_tile_image)
            display_last_player(screen, len(game_info['tiles_of_players'][game_info['players'].index(last_player)]), last_player_tile_image)

        refreshScreen()

main()
pygame.quit()