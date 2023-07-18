import pygame
from sys import exit

# Initialize PyGame
pygame.init()

# Set window's width and height
width = 1080  # px
height = 720  # px

# Set game's frame rate
framerate = 60  # Hz

# Define ground/window height ratio
ground_ratio = 0.15

# Define ground position
ground_x_pos = 0
ground_y_pos = height * (1 - ground_ratio)
ground_pos = (ground_x_pos, ground_y_pos)

# Define sky/window height ratio
sky_ratio = 1 - ground_ratio

# Define sky position
sky_x_pos = 0
sky_y_pos = 0
sky_pos = (sky_x_pos, sky_y_pos)

# Initialize window
screen = pygame.display.set_mode((width, height))

# Initialize PyGame clock
clock = pygame.time.Clock()

# Set window caption "Ofrenda"
pygame.display.set_caption('Ofrenda')

# Create ground surface
ground_surf = pygame.Surface((width, height*ground_ratio))
ground_surf.fill('pink4')

# Create sky surface
sky_surf = pygame.Surface((width, height*sky_ratio))
sky_surf.fill('plum4')

# Create and scale player
player_surf = pygame.image.load('../graphics/characters/neutral.png').convert_alpha()
player_surf = pygame.transform.scale_by(player_surf, 0.25)

# Get player dimensions
player_width, player_height = player_surf.get_size()

# Get player rectangle
player_rect = player_surf.get_rect(bottomleft=(0, ground_y_pos))

# Define player's velocity
player_x_vel = 4

# Initialize game window
running = True

while running:
    for event in pygame.event.get():
        # If close button has been pressed, quit PyGame
        if event.type == pygame.QUIT:
            pygame.quit()
            exit()

    # Blit surfaces on screen
    screen.blit(sky_surf, sky_pos)
    screen.blit(ground_surf, ground_pos)

    # Blit the player on screen
    screen.blit(player_surf, player_rect)

    # Draw all elements, update everything
    pygame.display.update()
    clock.tick(framerate)