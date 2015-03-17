# Features (already implemented, but not all are put in order) #

  * Simulation of ionic systems with long-range Coulomb interaction and short-range interaction (in various forms).
  * Classical Molecular Dynamics and Lattice Statics Simulations.
  * Periodic and Isolated Boundary Conditions using Ewald summation and direct summation, correspondingly.
  * Processing of simulation results (temperature dependences, size dependences).
  * Interatomic potentials fitting using methods of global optimization
  * Visualization of saved time evolution.
  * Distributed computing.

# Programming platform: #

  * C# for the most of the code
  * DirectCompute platform and HLSL shaders for GPU calculations.
  * C++ for interface to GPU and faster CPU calculations.