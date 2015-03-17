# Introduction #

The current version has the following capabilities:

  * Molecular dynamics simulation of uranium dioxide (UO2) and plutonium dioxide (PuO2)
  * Isolated boundary conditions (systems with free surface)
  * Approximation of pair potentials and rigid ions
  * Using GPU for computation of pair forces and energy
  * Computation of lattice period and total energy of the system

# Requirements #

## For execution: ##
  * 64-bit version of Microsoft Windows
  * Microsoft DirectX 11
  * Microsoft .NET Framework 4.0
  * DirectX 11 compatible graphics card (CPU-only computation is also possible)

## For compilation: ##
  * Microsoft Visual C# 2010

# Input and output #

You need to specify parameters of simulation in the source code except the sets of pair potentials and some parameters influencing speed of GPU computation.

The progress of simulation is shown in the title of the window and its single textbox.

Regularly the program save results to a file.

# Articles on our implementation of molecular dynamics on GPU #

  * A.S. Boyarchenkov, S.I. Potashnikov, Molecular dynamics using graphics processors and CUDA technology, Numerical methods and programming 10, 9-23 (2009). http://num-meth.srcc.msu.ru/english/zhurnal/tom_2009/v10r102.html (in Russian)
  * A.S. Boyarchenkov, S.I. Potashnikov, Parallel molecular dynamics with Ewald summation and integration on GPU, Numerical methods and programming 10, 158-175 (2009). http://num-meth.srcc.msu.ru/english/zhurnal/tom_2009/v10r119.html (in Russian)

# Articles with our results obtained using this technology #

  * A.S. Boyarchenkov, S.I. Potashnikov, K.A. Nekrasov, A.Ya. Kupryazhkin. Investigation of cation self-diffusion mechanisms in UO2±x using molecular dynamics. Journal of Nuclear Materials 442, 148-161 (2013). [preprint](http://arxiv.org/abs/1305.2901)
  * S.I. Potashnikov, A.S. Boyarchenkov, K.A. Nekrasov, A.Ya. Kupryazhkin. High-precision molecular dynamics simulation of UO2-PuO2: Anion self-diffusion in UO2 Journal of Nuclear Materials 433, 215-226 (2013). [preprint](http://arxiv.org/abs/1206.4429)
  * A.S. Boyarchenkov, S.I. Potashnikov, K.A. Nekrasov, A.Ya. Kupryazhkin. Molecular dynamics simulation of UO2 nanocrystals melting under isolated and periodic boundary conditions. Journal of Nuclear Materials 427, 311-322 (2012). http://dx.doi.org/10.1016/j.jnucmat.2012.05.023 [preprint](http://arxiv.org/abs/1103.6277)
  * A.S. Boyarchenkov, S.I. Potashnikov, K.A. Nekrasov, A.Ya. Kupryazhkin. Molecular dynamics simulation of UO2 nanocrystals melting. Rasplavy 2, 32-44 (2012). (in Russian)
  * A.S. Boyarchenkov, S.I. Potashnikov, K.A. Nekrasov, A.Ya. Kupryazhkin. Molecular dynamics simulation of UO2 nanocrystals surface. Journal of Nuclear Materials 421, 1-8 (2012). http://dx.doi.org/10.1016/j.jnucmat.2011.11.030 [preprint](http://arxiv.org/abs/1103.6010)
  * S.I. Potashnikov, A.S. Boyarchenkov, K.A. Nekrasov, A.Ya. Kupryazhkin. High-precision molecular dynamics simulation of UO2–PuO2: pair potentials comparison in UO2. Journal of Nuclear Materials 419, 217-225 (2011). http://dx.doi.org/10.1016/j.jnucmat.2011.08.033 [preprint](http://arxiv.org/abs/1102.1529)
  * S.I. Potashnikov, A.S. Boyarchenkov, K.A. Nekrasov, A.Ya. Kupryazhkin. High-precision molecular dynamics simulation of UO2-PuO2 : superionic transition in uranium dioxide (2011). [preprint](http://arxiv.org/abs/1102.1553)
  * S.I. Potashnikov, A.S. Boyarchenkov, K.A. Nekrasov, A.Ya. Kupryazhkin, Molecular dynamics fitting of interatomic pair potentials in uranium dioxide by thermal expansion. ISJAEE 8(52), 43-52 (2007). http://isjaee.hydrogen.ru/pdf/AEE0807/AEE08-07_Potashnikov.pdf (in Russian)
  * S.I. Potashnikov, A.S. Boyarchenkov, K.A. Nekrasov, A.Ya. Kupryazhkin, Molecular dynamics simulation of mass transport in uranium dioxide using graphics processors. ISJAEE 5(49), 86-93 (2007). http://isjaee.hydrogen.ru/pdf/AEE0507/ISJAEE05-07_Potashnikov.pdf (in Russian)

# See also #

  * [Version history](VersionHistory.md)
  * [Calculation time](Benchmark.md)