from sys import exit
from constants import *
from player import Player
from obstacle import Obstacle
import pygame


def collisions():
    if pygame.sprite.spritecollide(player.sprite, obstacles, False):
        player.sprite.vel_x = 0
        player.sprite.vel_x = 0
        player.sprite.rect.bottomleft = (PLAYER_START_X, PLAYER_START_Y)
        obstacles.empty()
        return False
    else:
        return True


# Initialize PyGame
pygame.init()

# Initialize PyGame clock
clock = pygame.time.Clock()

# Initialize game window
running = True

# Initialize game
game_active = True

# Initialize window
screen = pygame.display.set_mode((WINDOW_WIDTH, WINDOW_HEIGHT))

# Set window caption "Ofrenda"
pygame.display.set_caption('Ofrenda')

# Create ground surface
ground_surf = pygame.Surface((GROUND_WIDTH, GROUND_HEIGHT))
ground_surf.fill('pink4')

# Create sky surface
sky_surf = pygame.Surface((SKY_WIDTH, SKY_HEIGHT))
sky_surf.fill('plum4')

# Create player
player = pygame.sprite.GroupSingle()
player.add(Player())

# Create obstacles
obstacles = pygame.sprite.Group()

# Obstacle imer
obstacle_timer = pygame.USEREVENT + 1
pygame.time.set_timer(obstacle_timer, 1500)

while running:
    for event in pygame.event.get():
        # If close button has been pressed, quit PyGame
        if event.type == pygame.QUIT:
            pygame.quit()
            exit()
        if game_active:
            if event.type == pygame.MOUSEBUTTONDOWN and player.sprite.rect.bottom >= GROUND_POS_Y:
                if player.sprite.rect.collidepoint(event.pos):
                    player.sprite.vel_y = -12

            if event.type == pygame.KEYUP:
                if event.key == pygame.K_RIGHT or event.key == pygame.K_LEFT:
                    player.sprite.set_velocity(0, None)

            if event.type == obstacle_timer:
                # obstacle_rect_list.append(skull_surf.get_rect(bottomleft=(randint(1100, 1800), ground_y_pos)))
                obstacles.add(Obstacle())

        else:
            if event.type == pygame.KEYDOWN and event.key == pygame.K_SPACE:
                game_active = True

    if game_active:
        # Blit surfaces on screen
        screen.blit(sky_surf, (SKY_POS_X, SKY_POS_Y))
        screen.blit(ground_surf, (GROUND_POS_X, GROUND_POS_Y))

        player.draw(screen)
        player.update()

        obstacles.draw(screen)
        obstacles.update()

        game_active = collisions()

    else:
        screen.fill('pink4')

    # Draw all elements, update everything
    pygame.display.update()
    clock.tick(FRAMERATE)
