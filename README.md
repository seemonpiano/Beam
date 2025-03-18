Il main è stato modificato per permettere la generazione della matrice del sistema e il vettore dei termini noti in funzione della scelta di vincoli e carichi.


La costruzione della matrice si basa, per motivi pratici di costruzione, sull'integrazione della linea elastica prendendo una sola origine per entrambi i segmenti della trave.


Il Solver non è stato modificato.


Il Plotter è stato modificato, in modo tale da conformarlo alla scelta di una sola origine per entrambi i tratti.

L'esempio riportato nel main rappresenta una trave iperstatica con un incastro nell'estremo sinistro, una cerniera nel nodo in posizione 0.5, e una coppia applicata nell'estremo destro.
