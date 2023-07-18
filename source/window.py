import pygame
from sys import exit

# Initialize PyGame
pygame.init()

# Set window's width and height
width = 1080  # px
height = 720  # px

# Set game's frame rate
framerate = 60  # Hz

# Define ground and sky ratios
ground_ratio = 0.15
sky_ratio = 1 - ground_ratio

# Initialize window
screen = pygame.display.set_mode((width, height))

# Initialize PyGame clock
clock = pygame.time.Clock()

# Initialize title font
title_font = pygame.font.Font('../font/Scarville-Free.otf', 150)

# Set window caption "Ofrenda"
pygame.display.set_caption('Ofrenda')

# Create ground_surface
ground_surface = pygame.Surface((width, height*ground_ratio))
ground_surface.fill('pink4')

# Create sky_surface
sky_surface = pygame.Surface((width, height*sky_ratio))
sky_surface.fill('plum4')

# Create title_surface
title_surface = title_font.render('Ofrenda', False, 'hotpink4')

# Create and scale character
character_surface = pygame.image.load('../graphics/characters/neutral.png')
character_surface.convert()
character_surface = pygame.transform.scale_by(character_surface, 0.25)
character_width, character_height = character_surface.get_size()

# Define character's position
character_x_pos = 540
character_y_pos = height * (1 - ground_ratio) - character_height

character_x_vel = 4

while True:
    for event in pygame.event.get():
        # If close button has been pressed, quit PyGame
        if event.type == pygame.QUIT:
            pygame.quit()
            exit()

    # Blit surfaces on screen
    screen.blit(sky_surface, (0, 0))
    screen.blit(ground_surface, (0, height*(1-ground_ratio)))
    screen.blit(title_surface, (340, 100))

    if character_x_pos >= width - character_width and character_x_vel > 0:
        character_x_vel *= -1
        character_surface = pygame.transform.flip(character_surface, True, False)
    elif character_x_pos <= 0 and character_x_vel < 0:
        character_x_vel *= -1
        character_surface = pygame.transform.flip(character_surface, True, False)

    character_x_pos += character_x_vel

    screen.blit(character_surface, (character_x_pos, character_y_pos))

    # Draw all elements, update everything
    pygame.display.update()
    clock.tick(framerate)
