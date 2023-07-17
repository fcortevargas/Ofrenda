import pygame
from sys import exit

# Initialize PyGame
pygame.init()

# Set window's width and height
width = 800
height = 400

# Initialize window
screen = pygame.display.set_mode((width, height))

while True:
    for event in pygame.event.get():
        # If close button has been pressed, quit PyGame
        if event.type == pygame.QUIT:
            pygame.quit()
            exit()

    # Draw all elements, update everything
    pygame.display.update()