# Введение #

Текущая версия имеет следующие возможности:

  * Молекулярно-динамическое моделирование диоксидов урана (UO2) и плутония (PuO2)
  * Нулевые граничные условия (системы со свободной поверхностью)
  * Приближения парных потенциалов и точечных ионов
  * Использование GPU для расчетов парных сил и энергии
  * Расчет периода решетки и энергии системы, среднеквадратичного смещения частиц

# Минимальные требования #

## Для запуска: ##
  * 64-битная версия ОС Microsoft Windows
  * Microsoft DirectX 11
  * Microsoft .NET Framework 4.0
  * DirectX 11 совместимая видеокарта (возможно также проведение расчета полностью на CPU)

## Для компиляции: ##
  * Microsoft Visual C# 2010

# Входные и выходные данные #

Параметры моделирования необходимо задавать в исходном коде программы за исключением наборов парных потенциалов и некоторых параметров, влияющих на скорость GPU-расчетов.

Прогресс моделирования отображается в заголовке и текстовом поле окна.

Программа регулярно сохраняет результаты в файл.

# Статьи по нашей реализации молекулярной динамики на GPU #

  * А.С. Боярченков, С.И. Поташников, Использование графических процессоров и технологии CUDA для задач молекулярной динамики.  Вычислительные методы и программирование 10, 9-23 (2009). http://num-meth.srcc.msu.ru/zhurnal/tom_2009/v10r102.html
  * А.С. Боярченков, С.И. Поташников, Параллельная молекулярная динамика с суммированием Эвальда и интегрированием на графических процессорах. Вычислительные методы и программирование 10, 158-175 (2009). http://num-meth.srcc.msu.ru/zhurnal/tom_2009/v10r119.html

# Статьи с нашими результатами, полученными при помощи этой технологии #

  * A.S. Boyarchenkov, S.I. Potashnikov, K.A. Nekrasov, A.Ya. Kupryazhkin. Investigation of cation self-diffusion mechanisms in UO2±x using molecular dynamics. Journal of Nuclear Materials 442, 148-161 (2013). [препринт](http://arxiv.org/abs/1305.2901) (на английском)
  * S.I. Potashnikov, A.S. Boyarchenkov, K.A. Nekrasov, A.Ya. Kupryazhkin. High-precision molecular dynamics simulation of UO2-PuO2: Anion self-diffusion in UO2. Journal of Nuclear Materials 433, 215-226 (2013). [препринт](http://arxiv.org/abs/1206.4429) (на английском)
  * A.S. Boyarchenkov, S.I. Potashnikov, K.A. Nekrasov, A.Ya. Kupryazhkin. Molecular dynamics simulation of UO2 nanocrystals melting under isolated and periodic boundary conditions. Journal of Nuclear Materials 427, 311-322 (2012). http://dx.doi.org/10.1016/j.jnucmat.2012.05.023 [препринт](http://arxiv.org/abs/1103.6277) (на английском)
  * А.С. Боярченков, С.И. Поташников, К.А. Некрасов, А.Я. Купряжкин, Молекулярно-динамическое моделирование плавления нанокристаллов диоксида урана. Расплавы 2, 32-44 (2012).
  * A.S. Boyarchenkov, S.I. Potashnikov, K.A. Nekrasov, A.Ya. Kupryazhkin. Molecular dynamics simulation of UO2 nanocrystals surface. Journal of Nuclear Materials 421, 1-8 (2012). http://dx.doi.org/10.1016/j.jnucmat.2011.11.030 [препринт](http://arxiv.org/abs/1103.6010) (на английском)
  * S.I. Potashnikov, A.S. Boyarchenkov, K.A. Nekrasov, A.Ya. Kupryazhkin. High-precision molecular dynamics simulation of UO2-PuO2: pair potentials comparison in UO2. Journal of Nuclear Materials 419, 217-225 (2011). http://dx.doi.org/10.1016/j.jnucmat.2011.08.033 [препринт](http://arxiv.org/abs/1102.1529) (на английском)
  * S.I. Potashnikov, A.S. Boyarchenkov, K.A. Nekrasov, A.Ya. Kupryazhkin. High-precision molecular dynamics simulation of UO2-PuO2 : superionic transition in uranium dioxide (2011). [препринт](http://arxiv.org/abs/1102.1553) (на английском)
  * С.И. Поташников, А.С. Боярченков, К.А. Некрасов, А.Я. Купряжкин, Молекулярно-динамическое восстановление межчастичных потенциалов в диоксиде урана по тепловому расширению. Международный научный журнал «Альтернативная энергетика и экология»  8(52), 43-52 (2007). http://isjaee.hydrogen.ru/pdf/AEE0807/AEE08-07_Potashnikov.pdf
  * С.И. Поташников, А.С. Боярченков, К.А. Некрасов, А.Я. Купряжкин, Моделирование массопереноса в диоксиде урана методом молекулярной динамики с использованием графических процессоров. Международный научный журнал «Альтернативная энергетика и экология» 5, 86-93 (2007). http://isjaee.hydrogen.ru/pdf/AEE0507/ISJAEE05-07_Potashnikov.pdf

# См. также #

  * [История изменений](VersionHistoryRussian.md)
  * [Скорость расчетов](BenchmarkRussian.md)