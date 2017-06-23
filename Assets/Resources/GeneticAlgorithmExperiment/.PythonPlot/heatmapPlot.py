# -*- coding: utf-8 -*-

# When you first execute the program, please install these packages below.
# C:/Python27/python.exe -m pip install -U pip pandas numpy matplotlib

import sys
import os
import gc
import shutil
from pandas import Series, DataFrame
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
from mpl_toolkits.axes_grid1 import make_axes_locatable

# Our program.
def main(root, experiments):
	# Root of folder.
	root = os.path.dirname(root)

	for experiment in experiments:
		# Load the csv.
		data = pd.read_csv(root + "/datasets/" + experiment + "/experiment_1.csv")
		# Remove the useless columns and duplicated rows.
		data = data.drop(['label', 'score'], 1)
		data = data.drop_duplicates(subset=['run', 'generation', 'chromosome', 'position'], keep='first')
		# Plot the heatmaps.
		plotHeatmap(root, getAmountRecord(data), experiment)

# Filtering and cleaning the data about the gene amount information.
def getAmountRecord(data):
	amountRecord = pd.DataFrame()

	for runNumber in range(1, data['run'].max() + 1):
		for generationNumber in range(1, data['generation'].max() + 1):
			print "Current state: run is {}, generation is {}.".format(runNumber, generationNumber)
			for chromosomeNumber in range(1, data['chromosome'].max() + 1):
				# Get the chromosome info
				chromosome = getChromosome(data, runNumber, generationNumber, chromosomeNumber)
				# Calculate the number based on gene type.
				row = pd.DataFrame({
					"run"           : [ runNumber ],
					"generation"    : [ generationNumber ],
					"emptyCount"    : [ sum(chromosome.type == 'Empty') ],
					"enemyCount"    : [ sum(chromosome.type == 'Enemy') ],
					"treasureCount" : [ sum(chromosome.type == 'Treasure') ],
					"trapCount"     : [ sum(chromosome.type == 'Trap') ],
					"totalCount"    : [ len(chromosome) ]
				})
				amountRecord = amountRecord.append(row)

	# Reset the index of rows, and reorder the column.
	amountRecord = amountRecord.reset_index(drop=True)
	amountRecord = amountRecord[['run', 'generation', 'emptyCount', 'enemyCount', 'treasureCount', 'trapCount', 'totalCount']]

	# Release the memory.
	gc.collect()
	return amountRecord

# Plot the heatmap.
def plotHeatmap(root, amountRecord, experiment):
	# Export to the file.
	exportPath = root + "/output/" + experiment + "/heatmap/"
	exportFile = exportPath + "export.csv"
	if os.path.exists(exportPath):
		shutil.rmtree(exportPath)
	os.makedirs(exportPath)
	amountRecord.to_csv(exportFile, sep=',', index=False)
	print ( "Export the file to {}.".format(exportFile) )

	# Render the heatmap figures.
	for runNumber in range(1, amountRecord['run'].max() + 1):
		for generationNumber in range(1, amountRecord['generation'].max() + 1):
			chromosomes = amountRecord[(amountRecord['run'] == runNumber) & (amountRecord['generation'] == generationNumber)]
			chromosomes = chromosomes[['run', 'generation', 'emptyCount', 'enemyCount', 'totalCount']]

			positionCount = len(chromosomes)
			geneBound     = chromosomes['totalCount'].max()
			enemyBound    = amountRecord['enemyCount'].max()

			# Create the empty 2D list with the columns and rows.
			heatmapData = np.array([([0] * (geneBound + 1)) for _ in range(enemyBound + 1)], dtype=np.float)
			# Update the appearence times.
			for idx, chromosome in chromosomes.iterrows():
				heatmapData[chromosome.enemyCount][chromosome.emptyCount] += 1.0
			# Make the zero to the NaN.
			heatmapData[heatmapData == 0] = np.nan

			# Create the heatmap figure.
			fig, ax = plt.subplots(figsize=(7.0, 3.0))
			im = ax.imshow(heatmapData, cmap='rainbow', interpolation='nearest', vmin=0, vmax=positionCount)
			# Set the title and labels.
			ax.set_title('run ({}), generation ({})'.format(runNumber, generationNumber))
			ax.set_xlabel("Empty tile amount")
			ax.set_ylabel("Enemy tile amount")
			ax.set_yticks(np.arange(0, enemyBound + 1, 2))
			# Colorbar.
			divider = make_axes_locatable(ax)
			cax = divider.append_axes("right", size="5%", pad=0.1)
			cbar = fig.colorbar(im, cax=cax, ticks=[int(i) for i in np.linspace(0, positionCount, 5)])
			## cbar.ax.set_yticklabels(['Low', 'Medium', 'High'])
			# Save the figure and close it.
			plt.savefig("{}/{}_{}_result.png".format(exportPath, runNumber, generationNumber), dpi=300)
			plt.close(fig)
			print ('Rendered the heatmap figure. Run ({}), generation ({}).'.format(runNumber, generationNumber));

def getChromosome(data, runNum, genNum, chmNum):
	return data[(data.run == runNum) & (data.generation == genNum) & (data.chromosome == chmNum)]

if __name__ == "__main__":
	if (len(sys.argv) <= 2):
		print ("Sorry, the number of experiment is not enough.")
	else:
		main(sys.argv[1], sys.argv[2:])
