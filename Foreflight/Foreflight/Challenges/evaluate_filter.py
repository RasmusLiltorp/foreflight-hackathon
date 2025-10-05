import os
import sys
import random
import argparse
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt

MIN_GRADIENT = 2
MAX_GRADIENT = 20
MIN_DISTANCE = 1000
MAX_DISTANCE = 100000
MAX_ALLOWED_DISTANCE = 105000
MAX_FILTERED_POINTS = 10

def generate_random_line():
    initial_climb_gradient = random.uniform(MIN_GRADIENT, MAX_GRADIENT)
    final_climb_gradient = random.uniform(MIN_GRADIENT, MAX_GRADIENT)
    initial_climb_distance = min(random.uniform(MIN_DISTANCE, MAX_DISTANCE), MAX_ALLOWED_DISTANCE)
    acceleration_segment_distance = min(random.uniform(MIN_DISTANCE, MAX_DISTANCE), MAX_ALLOWED_DISTANCE - initial_climb_distance)
    final_climb_distance = max(MAX_ALLOWED_DISTANCE - (initial_climb_distance + acceleration_segment_distance), 0)

    x = [0, initial_climb_distance, initial_climb_distance + acceleration_segment_distance, initial_climb_distance + acceleration_segment_distance + final_climb_distance]
    y = [0, initial_climb_distance * (initial_climb_gradient / 100), initial_climb_distance * (initial_climb_gradient / 100), (initial_climb_distance * (initial_climb_gradient / 100)) + (final_climb_distance * (final_climb_gradient / 100))]
    return (x, y)

def line_intersects(line, terrain):
    (l_x, l_y) = line
    (t_x, t_y) = terrain

    for (x, y) in zip(t_x, t_y):
        if x <= l_x[1]:
            if y > x * (l_y[1] / l_x[1]):
                return True
        elif x <= l_x[2]:
            if y > l_y[2]:
                return True
        else:
            if y > l_y[2] + ((x - l_x[2]) * ((l_y[3] - l_y[2]) / (l_x[3] - l_x[2]))):
                return True
    return False

def read_obstacle_data(path):
    data = np.genfromtxt(path, delimiter=',', skip_header=1)
    if data.ndim == 1:
        data = np.expand_dims(data, axis=0)
    x = data[:, 0] 
    y = data[:, 1]
    return (x, y)

def evaluate_filtering(ground_truth, filtered_data, validation_count=1000, plot=False):
    valid = 0
    non_conservative = 0
    limiting = 0

    for _ in range(validation_count):
        (x,y) = generate_random_line()
        intersects_ground_truth = line_intersects((x,y), ground_truth)
        intersects_filtered = line_intersects((x,y), filtered_data)

        if (intersects_ground_truth and (not intersects_filtered)):
            # Filter is not conservative
            non_conservative += 1
            if plot:
                plt.plot(x, y, color="purple", alpha=0.2)

        if ((not intersects_ground_truth) and intersects_filtered):
            # flight path is limited by the filtered obstacles
            limiting += 1
            if plot: 
                plt.plot(x, y, color="green", alpha=0.2)

        if (not intersects_ground_truth):
            valid += 1
    return (valid, non_conservative, limiting)

def process_file(file_path, filename, validation_count, plot, results):
    try:
        (x, y) = read_obstacle_data(file_path)
        filtered_obstacles_folder = 'filtered_obstacles'
        filtered_file_path = os.path.join(filtered_obstacles_folder, filename)
        if os.path.exists(filtered_file_path):
            (fx, fy) = read_obstacle_data(filtered_file_path)

            icao = filename.split(".")[0]
            print(f"Evaluating {icao}...")

            if (len(fx) != len(fy)):
                print("ERROR: Length of coordinate lists do not match. Skipping")
                return
            if (len(fx) > MAX_FILTERED_POINTS):
                print(f"ERROR: Amount of filtered points ({len(fx)}) is greater than the maximum {MAX_FILTERED_POINTS}. Skipping")
                return

            if plot:
                plt.scatter(x, y, color='cornflowerblue', alpha=0.2)
                plt.scatter(fx, fy, color='blue')          
                plt.xlim(-6500, 100000)
                plt.ylim(0, max(y) * 1.1)

            (valid, non_conservative, limiting) = evaluate_filtering((x, y), (fx, fy), validation_count, plot)

            results['icao'].append(icao)
            results['validation_count'].append(validation_count)
            results['valid'].append(valid)
            results['valid_pct'].append(round((valid / validation_count) * 100, 5) if validation_count else 0)
            results['non_conservative'].append(non_conservative)
            results['non_conservative_pct'].append(round((non_conservative / valid) * 100, 5) if valid else 0)
            results['limiting'].append(limiting)
            results['limiting_pct'].append(round((limiting / valid) * 100, 5) if valid else 0)

            if plot:
                plt.show()
        else:
            print(f"Filtered obstacle file {filename} does not exist in {filtered_obstacles_folder}.")
    except Exception as e:
        print(f"Error reading {file_path}: {e}")

def parse_args():
    """Parse command line arguments."""
    parser = argparse.ArgumentParser(description="Script to evaluate obstacle filtering")
    parser.add_argument("--validation_count", type=int, default=10000, help="Number of generated flight paths (default 10000)")
    parser.add_argument("--plot", action="store_true", help="Enable plotting")
    parser.add_argument("--seed", type=int, help="Random seed (default is random)")

    return parser.parse_args()

if __name__ == "__main__":
    args = parse_args()  

    if args.seed is not None:
        seed = args.seed
    else:
        seed = random.randrange(sys.maxsize)
    random.seed(seed)
    print(f"Using seed {seed}")

    validation_count = args.validation_count
    plot = args.plot

    obstacles_folder = 'challenge_5'
    if os.path.exists(obstacles_folder):
        results = {
            'icao': [], 
            'validation_count': [], 
            'valid': [], 
            'valid_pct': [], 
            'non_conservative': [], 
            'non_conservative_pct': [], 
            'limiting': [], 
            'limiting_pct': []
        }
        for filename in os.listdir(obstacles_folder):
            file_path = os.path.join(obstacles_folder, filename)
            if file_path.endswith('.csv'):
                process_file(file_path, filename, validation_count, plot, results)

        totals = {key: sum(results[key]) for key in ['validation_count', 'valid', 'non_conservative', 'limiting']}
        totals['valid_pct'] = round((totals['valid'] / totals['validation_count']) * 100, 2) if totals['validation_count'] else 0
        totals['non_conservative_pct'] = round((totals['non_conservative'] / totals['valid']) * 100, 2) if totals['valid'] else 0
        totals['limiting_pct'] = round((totals['limiting'] / totals['valid']) * 100, 2) if totals['valid'] else 0

        output_results = {key: value for key, value in results.items() if key not in ['valid', 'valid_pct']}
        
        for key in output_results.keys():
            output_results[key].append(totals.get(key, "TOTAL"))

        df = pd.DataFrame(output_results)
        df.to_csv("results.csv", index=False)
        print()
        print("Results:")
        print()
        print(df.to_string(index=False))

    else:
        print(f"The folder {obstacles_folder} does not exist.")
