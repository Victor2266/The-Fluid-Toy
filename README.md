# The Fluid Toy
**CPU-GPU Fluid Simulation in Unity: Optimizing Collision Detection with GPU Acceleration for Real-Time Particle-Based Fluid Simulations**  

https://github.com/user-attachments/assets/d0044c6f-0194-4cee-95ac-1c2d5e176ef4


## 👥 Team Members
- Victor Do  
- Davis Cheung  
- Cameron Tuffner-Lyons  
- JJ Marr  

## 🕹 Download and Play Here:
- [Itch.io](https://awasete.itch.io/the-fluid-toy)
- [Our Website Redirect](http://thefluidtoy.com/)

## 🖥️💻 Supported platforms
- Windows
- Mac
- Linux
- Android
- WebGPU
---

## 🚀 Key Features  

### 🧊 Collision Systems  
- **Box Colliders**: Combine multiple static or dynamic oriented boxes to create complex shapes (e.g., pinwheels)  
- **Hybrid Circle Colliders**:  
  - Small colliders: GPU-accelerated using spatial hashing grids  
  - Large colliders: CPU-managed to prevent GPU performance degradation  
- **Complex Colliders**:  
  - Texture-based density maps for complex shapes  
  - Runtime-alterable collider geometry via alpha threshold sampling  
  - Player-editable collider deformation through brush tools  

### ⚡ Dynamic Particle Management  
- **Activation/Deactivation**:  
  - Particle structs with enable flags for conditional processing  
  - Memory-optimized allocation using pre-allocated buffers
  - Draw Brush which uses interlocked add operation to avoid race condition
  - Eraser Brush which randomizes particle position for even density distribution
  - **Source/Drain Objects**:  
    - Source: Spawns particles with configurable initial velocities  
    - Drain: Disables particles using spatial triggers  
    - Automatic buffer management with particle recycling
- **Multi-Fluid Support**:  
  - Data-oriented design with fluid property tables  
  - 64px×1px gradient textures per fluid type stitched into 2D atlas  
  - Shader-driven visual differentiation using dynamic branching  

### 🌐 Simulation Properties
- **All States of Matter**:
  - *Gasses*
  - *Liquids*
  - *Powders*  
- **Edge Behavior Modes**:  
  - *Solid*: Acts as immovable wall (default)  
  - *Void*: Disables particles and randomizes positions
    - Position randomization avoids spatial hash collisions in void mode  
  - *Loop*: Warps particles to opposite boundary  
- **Gravity Behavior Modes**:
  - *Normal*
  - *Reversed*
  - *Radial*
  - *Left*
  - *Right*
  - *Zero*
- **Temperature Simulation**:
  - Particles have individual temperatures
  - When close together, particle temperatures will diffuse and reach equilibrium temp at a rate determined by the particle type's diffusivity
  - Certain particle types will lose temperature to the ambient environment based on entropy values
  - Temperature is used for certain visual shaders such as for Lava and Fire.
  - Thermal Boxes will heat/cool particles which touch them to a set temperature, the speed can be controlled by the box's thermal conductivity.
- **State Change Behaviors**:
  - Particles can change states after reaching temperture thresholds
    
### 🎮 Game Systems  
- **Fluid Density Detection**:  
  - AsyncGPUReadback for non-blocking particle position queries  
  - Spatial density calculations for level event triggers
- **Temperature Detection**:  
  - AsyncGPUReadback for non-blocking particle temp queries  
  - Spatial temperature calculations for level event triggers  
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
- **Contextual Tooltips**:  
  - Hover-sensitive help system  
  - Mobile-optimized touch-and-hold activation  
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

### CPU-GPU Architecture  
- **Memory Bridges**:  
  - Compute buffers for particle data transfer  
  - Structured buffers for collider information  
  - Constant buffers for simulation setting properties  
- **Command Execution**:  
  - ComputeShader.Dispatch for kernel launches  
  - AsyncGPUReadback for non-blocking data retrieval  
  - Graphics.DrawMeshInstancedIndirect for rendering  

### Compute Shader Optimization  
- **Dispatch Strategy**:  
  - Thread groups sized to GPU wavefront (32-64 threads)  
  - Particle property tables loaded per-threadgroup  
- **Thread Synchronization**:  
  - InterlockedAdd for controlled particle spawning  
  - Position randomization seed generated using frame number and atomic counter value  
    
### Visual Pipeline  
- Dual-pass rendering with custom alpha blending  
- Vertex/fragment shaders using fluid-type flags  
- Dynamic texture atlas for fluid gradients  

### Hybrid Workload Distribution  
- **CPU Responsibilities**:  
  - Large collider transformations  
  - Game state management  
  - UI/input processing  
- **GPU Pipeline**:  
  - SPH fluid solver (with density/pressure/viscosity/friction/temperture kernels)
  - Spatial hashing for neighbor detection  
  - Collision resolution using boundary textures  

---

## 🙏 Acknowledgments  
- Special thanks to **Sebastian Lague** for inspirational fluid simulation content.
- Thanks to [The Powder Toy](https://github.com/The-Powder-Toy/The-Powder-Toy) for physics sim inspiration.
- **Testing**: Validated through family/friend playtests  
---

*Note: Screenshots referenced in original document have been omitted for brevity. Full visual documentation available in development whitepapers.*
