import json
import math
import random

import numpy as np
import cv2
from mss import mss
from PIL import Image
from pynput.keyboard import Key, Controller
import time

from agent import Agent
from population_manager import Population
from tqdm import tqdm

POPULATION_SIZE = 20
START_LEVEL = 0

dead_dist = 200


def calibrate_screen(sct):
    while True:
        screenShot = sct.grab(mon)
        img = Image.frombytes(
            'RGBA',
            (screenShot.width, screenShot.height),
            screenShot.bgra,
        )
        img = np.array(img)
        cv2.imshow('calibrate', img)
        key = cv2.waitKey(1)
        if key == 27 or key == 113:
            cv2.destroyAllWindows()
            return
            break


def get_position(sct, target_x, target_y, target_radius, show_screen: bool = False):
    target_radius = abs(target_radius)
    screenShot = sct.grab(mon)
    img = Image.frombytes(
        'RGBA',
        (screenShot.width, screenShot.height),
        screenShot.bgra,
    )
    # cv2.imshow('test', np.array(img))
    img = np.array(img)

    hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)

    # Threshold of blue in HSV space
    lower_skin = np.array([2, 75, 255])
    upper_skin = np.array([22, 95, 255])

    mask = cv2.inRange(hsv, lower_skin, upper_skin)

    cnts = cv2.findContours(mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
    cnts = cnts[0] if len(cnts) == 2 else cnts[1]
    box = cv2.cvtColor(mask, cv2.COLOR_GRAY2BGR)

    x, y = x_pos, y_pos
    w, h = 0, 0
    highest_y = 0

    points = []
    for c in cnts:
        tx, ty, tw, th = cv2.boundingRect(c)
        points.append([tx, ty])
        if ty > highest_y:
            x, y, w, h = tx, ty, tw, th

    dead = False
    for e, a in enumerate(points):
        for b in points[e + 1:]:
            dist = math.dist(a, b)
            if dist > dead_dist:
                dead = True

    if show_screen:
        cv2.rectangle(img, (x, y), (x + w, y + h), (255, 100, 255), 6)

        cv2.circle(img, (target_x, target_y), target_radius, (100, 255, 100, 10), 5)
        cv2.circle(img, (target_x, target_y), 2, (100, 255, 100), -1)

        cv2.imshow('img', img)
        key = cv2.waitKey(1)
        if key == 27 or key == 113:
            cv2.destroyAllWindows()
            quit()
    return int(x + w / 2), int(y + h / 2), dead


def generate_actions():
    actions = []
    for horizontal in [Key.left, Key.right, None]:
        for vertical in [Key.up, Key.down, None]:
            for key in ['z', 'x', None]:  # Jump, dash, none
                if key != 'x' and vertical == Key.down:
                    continue
                if key is None and vertical is not None:
                    continue

                actions.append([horizontal, vertical, key])
    return actions


def next_level(keyboard: Controller):
    keyboard.press('f')
    time.sleep(0.2)
    keyboard.release('f')
    time.sleep(0.75)


def reset_level(keyboard: Controller):
    keyboard.press('f')
    time.sleep(0.3)
    keyboard.release('f')
    time.sleep(0.3)
    keyboard.press('s')
    time.sleep(0.3)
    keyboard.release('s')
    time.sleep(0.75)


mon = {'left': 200, 'top': 225, 'width': 800, 'height': 800}
x_pos, y_pos = 0, 0

best_score = -999999
generation_counter = 0
level_counter = START_LEVEL
last_press = time.time()
start = time.time()

actions = generate_actions()
buttons = ['x', 'z', Key.left, Key.right, Key.up, Key.down]

pop = Population(population_size=POPULATION_SIZE)
pop.agents[0].mutation_chance = 1

if __name__ == '__main__':
    levels = json.loads(open('levels.json', 'r').read())
    keyboard = Controller()

    with mss() as sct:
        targets = levels['levels'][level_counter]['targets']
        calibrate_screen(sct)
        get_position(sct, targets[0]['x'], targets[0]['y'], targets[0]['thresh'])
        time.sleep(1)

        level_time = 0
        for target in targets:
            level_time += target['time']
            print('Target:', (target['x'], target['y']))
            pop.add_target(num_choices=len(actions), num_actions=int(11 * target['time'] + 0.5))
            while True:
                print('------')
                print('Generation:', generation_counter)
                generation_counter += 1

                for agent in tqdm(pop.get_agents()):
                    agent: Agent = agent
                    x_pos = 0
                    y_pos = 0
                    reset_level(keyboard)
                    start = time.time()
                    dead = False
                    score = 0
                    target_counter = 0

                    action_counter = 0
                    for a in agent.get_actions():
                        if a == -1:
                            print('targets be like', target['pause'])
                            start_pause = time.time()
                            while time.time() - start_pause < target['pause'] and not dead:
                                x_pos, y_pos, dead = get_position(sct, targets[target_counter]['x'], targets[target_counter]['y'], targets[target_counter]['thresh'])
                                for b in buttons:
                                    keyboard.release(b)
                            target_counter +=1
                            action_counter = 0
                            continue
                        action_counter += 1

                        action = actions[a]
                        for key in action:
                            if key is not None:
                                keyboard.press(key)
                        last_press = time.time()
                        while time.time() - last_press < 0.25:
                            x_pos, y_pos, dead = get_position(sct, targets[target_counter]['x'], targets[target_counter]['y'], targets[target_counter]['thresh'])
                            if dead:
                                break
                        agent.cut_to(action_counter)

                        if dead:
                            break

                        for b in buttons:
                            keyboard.release(b)
                        time.sleep(0.01)

                        if time.time() - start > level_time:
                            break

                    start_pause = time.time()
                    while time.time() - start_pause < target['pause'] and not dead:
                        x_pos, y_pos, dead = get_position(sct, targets[target_counter]['x'], targets[target_counter]['y'], targets[target_counter]['thresh'])

                    score += -math.dist((x_pos, y_pos), (target['x'], target['y']))
                    agent.set_fitness(score)
                    if score > best_score:
                        print(f"Improved from {best_score} to {score}")
                        best_score = score
                print("Best Score:", best_score)

                print(pop.get_agents()[0].get_actions())
                print(len(pop.get_agents()[0].get_actions()))


                if best_score > target['thresh']:
                    print("REACHED CHECKPOINT")
                    best_score = -999999
                    pop.propogate()
                    break
                else:
                    pop.evolve()
            level_time += target['pause']

        cv2.destroyAllWindows()
        print('done')
        # # Replay success
        # print('replaying winner')
        # while True:
        #     agent = pop.get_agents()[0]
        #     x_pos = 0
        #     y_pos = 0
        #     reset_level(keyboard)
        #     start = time.time()
        #     for a in agent.get_actions():
        #         action = actions[a]
        #         for key in action:
        #             if key is not None:
        #                 keyboard.press(key)
        #         last_press = time.time()
        #         while time.time() - last_press < 0.125:
        #             pass
        #
        #         for key in reversed(action):
        #             if key is not None:
        #                 keyboard.release(key)
        #         time.sleep(0.01)
        #         if time.time() - start > level_time:
        #             break
        #
        #     score = -math.dist((x_pos, y_pos), (target['x'], target['y']))
        #     agent.set_fitness(score)
