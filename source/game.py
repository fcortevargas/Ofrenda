from sys import exit

import pygame

# Initialize PyGame
pygame.init()

# Set window's width and height
width = 1080  # px
height = 720  # px

# Set game's frame rate
framerate = 60  # Hz

# Define ground/window height ratio
ground_ratio = 0.20

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
ground_surf = pygame.Surface((width, height * ground_ratio))
ground_surf.fill('pink4')

# Create sky surface
sky_surf = pygame.Surface((width, height * sky_ratio))
sky_surf.fill('plum4')

# Create and scale player
player_surf = pygame.image.load('../graphics/characters/happy.png').convert_alpha()

# Create and scale skull (enemy)
skull_surf = pygame.image.load('../graphics/enemies/skull.png').convert_alpha()

# Get player dimensions
player_width, player_height = player_surf.get_size()
print(player_width, player_height)

# Get player rectangle
player_rect = player_surf.get_rect(bottomleft=(20, ground_y_pos))

# Get skull rectangle
skull_rect = skull_surf.get_rect(bottomleft=(800, ground_y_pos))

# Define player's acceleration
player_x_acc = 0
player_y_acc = 0.5

# Define player's velocity
player_x_vel = 0
player_y_vel = 0

player_x_dir = 1

# Initialize game window
running = True

# Initialize game
game_active = True

while running:
    for event in pygame.event.get():
        # If close button has been pressed, quit PyGame
        if event.type == pygame.QUIT:
            pygame.quit()
            exit()

        if event.type == pygame.MOUSEBUTTONDOWN and player_rect.bottom >= ground_y_pos:
            if player_rect.collidepoint(event.pos):
                player_y_vel = -10

        if event.type == pygame.KEYDOWN:
            if event.key == pygame.K_SPACE and player_rect.bottom >= ground_y_pos:
                player_y_vel = -10
            if event.key == pygame.K_RIGHT:
                player_x_vel = 5
                player_surf = pygame.transform.flip(player_surf, True, False) if player_x_dir == -1 else player_surf
                player_x_dir = 1
            if event.key == pygame.K_LEFT:
                player_x_vel = -5
                player_surf = pygame.transform.flip(player_surf, True, False) if player_x_dir == 1 else player_surf
                player_x_dir = -1

        if event.type == pygame.KEYUP:
            if event.key == pygame.K_RIGHT or event.key == pygame.K_LEFT:
                player_x_vel = 0

    if game_active:
        # Blit surfaces on screen
        screen.blit(sky_surf, sky_pos)
        screen.blit(ground_surf, ground_pos)

        # Do stuff with the player
        player_y_vel += player_y_acc
        player_x_vel += player_x_acc

        player_rect.y += player_y_vel
        player_rect.x += player_x_vel

        if player_rect.bottom >= ground_y_pos:
            player_rect.bottom = ground_y_pos
            player_y_vel = 0

        # Blit the player on screen
        screen.blit(player_surf, player_rect)

        # Blit the skull on screen
        screen.blit(skull_surf, skull_rect)

        # Collision
        if player_rect.colliderect(skull_rect):
            game_active = False
    else:
        screen.fill('Black')

    # Draw all elements, update everything
    pygame.display.update()
    clock.tick(framerate)
