# MAD MLAgent Drift

**Project Link:** [GitHub - batuhancetinkaya1/MAD-MLAgentDrift](https://github.com/batuhancetinkaya1/MAD-MLAgentDrift)

This project trains a 2D drifting car agent using Unity & ML-Agents. The AI is developed to operate across multiple racetracks simultaneously, enhancing its driving performance and adaptability. Initially, the plan was to train on one track, test on another, and finally evaluate on a third. However, this approach led to overfitting and high collision rates (~16 per lap). With the multi-run strategy—incorporating checkpoint distance and angular difference into the agent's observations—performance improved significantly.

---

## Observations
- **Distance to Next Checkpoint:** Informs the agent of the gap to its target.
- **Angular Difference to Checkpoint:** Helps the agent align its heading properly.
- **Additional Data:** Velocity, steering, and collision feedback also contribute to decision-making.

---

## Table of Contents
- [Features](#features)
- [Tracks](#tracks)
  - [Catalunya](#catalunya)
  - [Istanbul Park](#istanbul-park)
  - [Monaco](#monaco)
- [Model & Training](#model--training)
- [Car Prefab](#car-prefab)
- [Installation](#installation)
- [Usage](#usage)
- [Screenshots](#screenshots)
- [Contributing](#contributing)
- [License](#license)

---

## Features
- **Multi-Track Training:**  
  The agent is trained concurrently on three different tracks:
  - **Catalunya:** Easiest for the agent with an average of ~6 collisions per lap.
  - **Istanbul Park:** Moderate difficulty with ~8 collisions per lap.
  - **Monaco:** Most challenging with ~10 collisions per lap.
- **Realistic Drift Physics:**  
  Custom 2D physics for true drift simulation.
- **Checkpoint System:**  
  Approximately 27 checkpoints per track guide the agent.
- **Scalable Car Prefab:**  
  The car is manually scaled for each track using two different edge colliders for optimal fit.
- **Best Model:**  
  The top-performing model is saved as `5499989.onnx`.
- **Easy Integration:**  
  The Car prefab automatically adapts its scale to match each racetrack.

---

## Tracks

### Catalunya
Located near Barcelona, **Catalunya** features long straights and smooth curves. It is the track where the agent performs best (avg. ~6 collisions per lap).  
![Catalunya](PLACEHOLDER_IMG_PATH)

### Istanbul Park
Known for its famous Turn 8, **Istanbul Park** combines high speeds with sharp turns. This track offers a moderate challenge with an average of ~8 collisions per lap.  
![Istanbul Park](PLACEHOLDER_IMG_PATH)

### Monaco
**Monaco** is a tight circuit with very close walls. With numerous checkpoints (~27 per lap) and sharp corners, it averages ~10 collisions per lap. Its smaller scale relative to the car adds an extra layer of challenge.  
![Monaco](PLACEHOLDER_IMG_PATH)

---

## Model & Training
- **Approach:**  
  - **Multi-Run Training:** Simultaneous training on all three tracks reduces overfitting and collision rates.
  - **Enhanced Observations:** The agent now receives both distance and angular difference to the next checkpoint.
- **Performance Improvements:**  
  - Collision rates dropped from ~16 to ~8 per lap.
  - Initial training required separate models and manual tuning; the multi-run method streamlined this process.
- **Monitoring:**  
  - Training metrics are visualized in real time with TensorBoard.
  - Users can adjust configuration files to review all training runs.
- **Best Model:**  
  `5499989.onnx` represents the best-performing model.
  
![Training Graph](PLACEHOLDER_IMG_PATH)

---

## Car Prefab
The single `CarAgent` prefab is used across all tracks and includes:
- **Drift Physics Components:** Custom scripts for realistic drift dynamics.
- **Sensor Components:** For detecting walls, boundaries, and checkpoints.
- **ML-Agents Integration:** Behavior parameters, observation settings, and action definitions.

---

## Installation
1. **Unity & ML-Agents:**  
   - Download **Unity 6000.0.35f1** (or later).  
   - Install/update the `ML-Agents` package via the Unity Package Manager.
2. **Clone the Project:**  
   ```bash
   git clone https://github.com/batuhancetinkaya1/MAD-MLAgentDrift.git
   ```

---

## Usage

### Opening the Project
- Open Unity and navigate to **File > Open Project**.
- Select the cloned project folder.

### Starter Code & Environment Setup
- The project includes starter code files.
- Set up a Python virtual environment (`venv` or `env`) with the ML-Agents CLI installed.

### Running the Sample Scene
- Open the **SampleScene** in Unity.
- The best model (`5499989.onnx`) is pre-assigned to the car.
- Press **Play** in the Unity Editor to observe the agent's performance.

### Training the Agent
- Use the provided YAML configuration to start training:
  ```bash
  mlagents-learn CarAgentPPO.yaml --run-id=CarRun101
  ```

### Adjusting Training Settings
Modify the YAML file as needed to adjust hyperparameters and training conditions.

### Monitoring with TensorBoard
Run:
```bash
tensorboard --logdir=results --port=6006
```
TensorBoard will display graphs and metrics for all training runs.

---

## Screenshots

#### Track Overviews:
![Catalunya, Istanbul Park, Monaco](PLACEHOLDER_IMG_PATH)

#### Car Prefab in Unity:
![CarAgent Prefab in Editor](PLACEHOLDER_IMG_PATH)

#### TensorBoard Training Graphs:
![TensorBoard Graphs](PLACEHOLDER_IMG_PATH)

*(Replace `PLACEHOLDER_IMG_PATH` with your actual image paths.)*

---

## Contributing
Contributions are welcome!

Please open an Issue for bug reports or feature requests, and submit a Pull Request with your improvements.

---

## License
This project is licensed under the MIT License.  
See the [LICENSE](LICENSE) file for more details.
