import pygame

from constants import *


class Player(pygame.sprite.Sprite):
    def __init__(self):
        super().__init__()

        # Create player surface
        self.image = pygame.image.load('../graphics/characters/neutral.png').convert_alpha()

        # Get player rectangle
        self.rect = self.image.get_rect(bottomleft=(PLAYER_START_X, PLAYER_START_Y))

        self.ground_y = PLAYER_START_Y

        # Define player's direction
        self.dir = 1

        # Define player's velocity
        self.vel_x = 0
        self.vel_y = 0

        # Define player's acceleration
        self.acc_x = 0
        self.acc_y = 0.4

    def input(self):
        keys = pygame.key.get_pressed()

        if keys[pygame.K_SPACE] and self.rect.bottom >= PLAYER_START_Y:
            # Impulse velocity which makes character jump
            self.vel_y = -10

        if keys[pygame.K_RIGHT]:
            self.vel_x = 5
            self.image = pygame.transform.flip(self.image, True, False) if self.dir == -1 else self.image
            self.dir = 1

        if keys[pygame.K_LEFT]:
            self.vel_x = -5
            self.image = pygame.transform.flip(self.image, True, False) if self.dir == 1 else self.image
            self.dir = -1

    def set_velocity(self, vel_x, vel_y):
        self.vel_x = vel_x if vel_x is not None else self.vel_x
        self.vel_y = vel_y if vel_y is not None else self.vel_y

    def move(self):
        self.rect.x += self.vel_x
        self.rect.y += self.vel_y

        self.apply_gravity()
            
    def apply_gravity(self):
        # Accelerate player
        self.vel_y += self.acc_y
        self.rect.y += self.vel_y

        # If player has touched the ground, stop the motion
        if self.rect.bottom >= self.ground_y:
            self.rect.bottom = self.ground_y
            self.vel_y = 0

    def update(self):
        self.move()
        self.input()
            