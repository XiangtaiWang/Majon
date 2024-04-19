import pygame
import random


# pygame.image.load("images/stick1.png")
class Tile:
    def __init__(self, type, number, image):
        self.type = type  # Character, Bamboo, etc.
        self.number = number
        self.image = image
        self.expose = False


def load_tiles():
    types = ["one", 'stick', 'bing']
    other_types = [('east', 0), ('south', 1), ('west', 2), ('north', 3), ('central', 4), ('fa', 5), ('whiteboard',6)]

    numbers = [i for i in range(1, 10)]
    tiles = [Tile(tp, num, pygame.image.load(f"images/{tp}{num}.png")) for tp in types for num in numbers]
    tiles += [Tile('other', tp[1], pygame.image.load(f"images/{tp[0]}.png")) for tp in other_types]
    tiles *= 4
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
    tiles = load_tiles()

    players_tiles = allocate_tiles_to_players(tiles)

    run = True
    while run:
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                run = False
                
        refreshScreen()

        display_player1(screen, players_tiles[0])
        display_player2(screen, players_tiles[1])
        display_player3(screen, players_tiles[2])
        display_player4(screen, players_tiles[3])

        newline = 3
        weight = 100

        for i, tile in enumerate(tiles):
            if i%16==0:
                newline +=1
                weight = 100
            weight += 30
            
            screen.blit(tile.image, (weight, newline*50))

def refreshScreen():
    pygame.display.flip()

def allocate_tiles_to_players(tiles):
    random.shuffle(tiles)

    tiles_of_players = [[] for i in range(4)]

    for i in range(4):
        for j in range(4):
            for k in range(4):
                tiles_of_players[j].append(tiles.pop())
    return tiles_of_players

def display_player1(screen, tiles):
    weight = 130
    for tile in tiles:
        screen.blit(tile.image, (weight, 580))
        weight += 30

def display_player2(screen, tiles):
    for i, tile in enumerate(tiles):
        height = 90 + i*30
        screen.blit(pygame.transform.rotate(tile.image, 90), (700, height))

def display_player3(screen, tiles):
    for i, tile in enumerate(tiles):
        weight = 130 + i*30
        screen.blit(tile.image, (weight, 30))

def display_player4(screen, tiles):
    for i, tile in enumerate(tiles):
        height = 90 + i*30
        screen.blit(pygame.transform.rotate(tile.image, 270), (30, height))
main()
pygame.quit()