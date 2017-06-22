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

			positionCount = len(chromosomes) + 1
			geneBound     = chromosomes.totalCount.max()
			enemyBound    = chromosomes.enemyCount.max()

			# Create the empty 2D list with the columns and rows.
			heatmapData = [([0] * (geneBound + 1)) for _ in range(geneBound + 1)]

			for idx, chromosome in chromosomes.iterrows():
				heatmapData[chromosome.enemyCount][chromosome.emptyCount] += 1.0 / positionCount

			# Export the heatmap figure.
			figure = plt.figure()
			plt.imshow(heatmapData, cmap='coolwarm', interpolation='nearest')
			plt.savefig("{}/{}_{}_result.png".format(os.getcwd(), runNumber, generationNumber))
			plt.close(figure)
			print ('Rendered the heatmap figure. Run ({}), generation ({}).'.format(runNumber, generationNumber));


def getAmountRecord(data):
	amountRecord = pd.DataFrame()

	for runNumber in range(1, data['run'].max() + 1):
		for generationNumber in range(1, 10): #range(1, data['generation'].max() + 1):
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