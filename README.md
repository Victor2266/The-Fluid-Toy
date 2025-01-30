# CPU-GPU Fluid Simulation in Unity  
**Optimizing Collision Detection with GPU Acceleration for Real-Time Particle-Based Fluid Simulations**  

![Project Banner](https://github.com/user-attachments/assets/516de58e-c79b-4065-b4ed-0b03a61b7f06)  

## 👥 Team Members
- Victor Do  
- Davis Cheung  
- Cameron Tuffner-Lyons  
- Jj Marr  

---

## 🚀 Key Features  

### 🧊 Collision Systems  
- **Box Colliders**: Combine multiple oriented boxes to create complex shapes (e.g., pinwheels)  
- **Hybrid Circle Colliders**:  
  - Small colliders: GPU-accelerated using spatial hashing grids  
  - Large colliders: CPU-managed to prevent GPU performance degradation  

### ⚡ Dynamic Particle Management  
- **Activation/Deactivation**:  
  - Particle structs with enable flags for conditional processing  
  - Memory-optimized allocation using pre-allocated buffers  
- **Multi-Fluid Support**:  
  - Data-oriented design with fluid property tables  
  - 64px×1px gradient textures per fluid type stitched into 2D atlas  
  - Shader-driven visual differentiation using dynamic branching  

### 🎮 Game Systems  
- **Fluid Density Detection**:  
  - AsyncGPUReadback for non-blocking particle position queries  
  - Spatial density calculations for level event triggers  
- **Audio System**:  
  - Dual-channel mixer with independent SFX/music control  
- **Progression System**:  
  - Time-based star ratings (1-3 stars per level)  
  - Cross-platform save data in OS-specific registries  

### 🖥️ UI/UX Features  
- **Menu System**:  
  - Main menu with Play, Sandbox, Settings, and Quit  
  - Level selection screen with progress visualization  
  - Pause menu with real-time settings adjustment  
- **Graphics Settings**:  
  - Resolution, refresh rate, and fullscreen controls  
  - Developer-level unlock shortcuts  
- **Cross-Platform Controls**:  
  - Unified input system for mouse/touchscreen  
  - Contextual UI trays with brush/level controls  

---

## 🎮 Game Content  

### Sandbox Mode  
- **Dual Purpose**:  
  1. Player experimentation with all fluid/types  
  2. Performance benchmarking environment  
- **Continuous Integration**: Always updated with latest features
  
### Level 1: Tavern Challenge  
- **Objective**: Fill a beer mug using gravity manipulation  
- **Tech Stack**:  
  - CPU-managed cup physics → GPU collision resolution  
  - Async particle position feedback for completion detection  

---

## ⚙️ Technical Highlights  

### Compute Shader Optimization  
- Wavefront-optimized memory access patterns (32-64 threads)  
- Particle property tables loaded per-threadgroup  

### Visual Pipeline  
- Dual-pass rendering with custom alpha blending  
- Vertex/fragment shaders using fluid-type flags  

### Hybrid Architecture  
- CPU: Manages large colliders and game state  
- GPU: Parallelized particle physics (SPH solver)  

---

## 🙏 Acknowledgments  
Special thanks to **Sebastian Lague** for inspirational fluid simulation content.
- **Testing**: Validated through family/friend playtests  
---

*Note: Screenshots referenced in original document have been omitted for brevity. Full visual documentation available in development whitepapers.*
