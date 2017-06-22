# -*- coding: utf-8 -*-

# When you first execute the program, please install these packages below.
# C:/Python27/python.exe -m pip install -U pip pandas numpy matplotlib

import sys
import os
from pandas import Series, DataFrame
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt

# Our program.
def main():
	data = pd.read_csv(os.getcwd() + "/experiment_1.csv")

	# Remove the useless columns and duplicated rows.
	data = data.drop(['label', 'score'], 1)
	data = data.drop_duplicates(subset=['run', 'generation', 'chromosome', 'position'], keep='first')

	amountRecord = getAmountRecord(data)

	# Export to the file.
	exportPath = os.getcwd() + "/export.csv"
	amountRecord.to_csv(exportPath, sep=',', index=False)
	print ( "Export the file to {}.".format(exportPath) )

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
			plt.subplots_adjust(right=1.0)
			cax = ax.imshow(heatmapData, cmap='coolwarm', interpolation='nearest', vmin=0, vmax=positionCount)
			# Set the title and labels.
			ax.set_title('run ({}), generation ({})'.format(runNumber, generationNumber))
			ax.set_xlabel("Empty tile amount")
			ax.set_ylabel("Enemy tile amount")
			# Horizontal colorbar.
			cbar = fig.colorbar(cax, ticks=[0, positionCount / 2, positionCount]) #, orientation='horizontal')
			cbar.ax.set_xticklabels(['Low', 'Medium', 'High'])
			# Save the figure and close it.
			plt.savefig("{}/{}_{}_result.png".format(os.getcwd(), runNumber, generationNumber), dpi=300)
			plt.close(fig)
			print ('Rendered the heatmap figure. Run ({}), generation ({}).'.format(runNumber, generationNumber));

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
	return amountRecord

def getChromosome(data, runNum, genNum, chmNum):
	return data[(data.run == runNum) & (data.generation == genNum) & (data.chromosome == chmNum)]

if __name__ == "__main__":
    main()