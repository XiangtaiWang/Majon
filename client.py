import pygame



# pygame.image.load("images/stick1.png")
class Tile:
    def __init__(self, type, number, image):
        self.type = type  # Character, Bamboo, etc.
        self.number = number
        self.image = image
        # self.rect = (x, y)


def load_tiles():
    types = ["one", 'stick', 'bing']
    other_types = ['east', 'south', 'west', 'north', 'central', 'fa', 'whiteboard']

    numbers = [i for i in range(1, 10)]
    tiles = [Tile(tp, num, pygame.image.load(f"images/{tp}{num}.png")) for tp in types for num in numbers]
    tiles += [Tile('other', 0, pygame.image.load(f"images/{tp}.png")) for tp in other_types]
    tiles *= 4
    adjust_image_size(tiles)

    return tiles

def adjust_image_size(tiles):
    IMAGE_SIZE = (30, 50)
    for tile in tiles:
        tile.image = pygame.transform.scale(tile.image, IMAGE_SIZE)

    return tiles

def main():
    pygame.init()
    SCREEN_WIDTH = 800
    SCREEN_HEIGHT = int(SCREEN_WIDTH*0.8)
    screen = pygame.display.set_mode((SCREEN_WIDTH, SCREEN_HEIGHT))
    pygame.display.set_caption("Majon")
    
    WHITE = (255, 255, 255)
    screen.fill(WHITE)

    tiles = load_tiles()

    run = True
    while run:
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                run = False
                
        refreshScreen()
        newline = 0
        weight = 0
        for i, tile in enumerate(tiles):
            if i%20==0:
                newline +=1
                weight = 0
            weight += 35
            
            screen.blit(tile.image, (weight, newline*50))

def refreshScreen():
    pygame.display.flip()


main()
pygame.quit()