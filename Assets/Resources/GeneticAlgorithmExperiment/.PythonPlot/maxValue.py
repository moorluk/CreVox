# When you first execute the program, please install these packages below.
# C:/Python27/python.exe -m pip install -U pip pandas numpy matplotlib

import sys
import os
import shutil
#get the max value of table
from pandas import Series, DataFrame
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import random

# Our program.
def main(root, experiments):
	# Root of folder.
	root = os.path.dirname(root)

	for experiment in experiments:
		# Import and export to the file.
		importPath = root + "/datasets/" + experiment + "/"
		exportPath = root + "/output/" + experiment + "/fitnessComparison/"
		# Clean the export path.
		if os.path.exists(exportPath):
			shutil.rmtree(exportPath)
		os.makedirs(exportPath)
		# Calculate and plot.
		theBestChromosome = exportTheBestChromosome(experiment, importPath, exportPath)
		newPlot(experiment, exportPath, theBestChromosome)
		newPlot2(experiment, exportPath, theBestChromosome)

# Export the best chromosomes table (single fitness score / all / run / class).
def exportTheBestChromosome(label, inputFolder, outputFolder):
	outputDate = DataFrame(columns =  ["run","generation","chromosome",'label','score','volume'])
	# Read file.
	data = pd.read_csv(inputFolder + "experiment_1.csv")
	numRun = data['run'].max()
	numGeneration = data['generation'].max()
	chromosomeCount = data['chromosome'].max()

	# Get input file
	for nR in range(1, numRun + 1, 1):
		# get all of label in this csv file.
		fitnessLabels = set(data.label.values)
		# Get absolute maximum for normalizing.
		fitnessMaximum = [0,0,0,0,0]
		for idx, fitnessName in enumerate(fitnessLabels):
			scores = data[(data.label == fitnessName)].score.values
			fitnessMaximum[idx] = max(abs(s) for s in scores)

		for ng in range(1, numGeneration + 1):
			#calculate all of chromosome score.
			chromosomes = []
			for chromosomeNumber in range(1, chromosomeCount + 1):
				scores = data[(data.run == nR) & (data.generation == ng) & (data.chromosome == chromosomeNumber)].score.values;
				# Only calc first score.
				chromosomeScore = sum(scores[:len(fitnessLabels)])
				chromosomes.append(chromosomeScore)
			#find index of the best score chromosome.
			#+1 is fixed the index is from 0,but data is from 1.
			chromosomeID = chromosomes.index(max(chromosomes)) + 1
			for idx, fitnessName in enumerate(fitnessLabels):
				# Only calc first score.
				labelScore = data[(data.run == nR) & (data.generation == ng) & (data.chromosome == chromosomeID) & (data.label == fitnessName)].score.values[0]
				# Normalize.
				labelScore = labelScore / fitnessMaximum[idx]
				info = [nR,ng,chromosomeID,fitnessName,labelScore,data.iloc[chromosomeID].volume]
				# add info to the last.
				outputDate.loc[-1] = info
				# shifting index
				outputDate.index = outputDate.index + 1
	#fixed [run,generation,chromosome] are float,not int.
	outputDate['run'] = outputDate['run'].astype(int)
	outputDate['generation'] = outputDate['generation'].astype(int)
	outputDate['chromosome'] = outputDate['chromosome'].astype(int)
	# output result table
	outputDate.to_csv(outputFolder + "bestChromosome_" + label + ".csv", index = False)
	return outputDate

def newPlot(experimentLabel, outputFolder, data):
	plt.figure(figsize = (16, 9), dpi = 120)
	numRun = max(data['run'])
	numGeneration = max(data['generation'])

	generationMeanList = list()
	generationStdList = list()
	for generation in range(1,numGeneration+1):
		plotData = []
		for run in range(1,numRun+1):
			generationsScore = sum(data[(data.run == run) & (data.generation == generation)].score)
			plotData.append(generationsScore)
		generationStdList.insert(generation, np.std(plotData))
		generationMeanList.insert(generation, np.mean(plotData))
	
	plt.errorbar(range(1,len(generationMeanList)+1), generationMeanList, generationStdList, marker = 'o', alpha = 0.3)
	plt.xlabel('Generation', fontsize = 25)
	plt.ylabel('Score', fontsize = 25)
	plt.tick_params(labelsize = 25)
	plt.savefig(outputFolder + experimentLabel +'_result.png')

def newPlot2(experimentLabel, outputFolder, data):
	plt.figure(figsize = (16, 9), dpi = 120)
	fitnessLabels = set(data.label.values)

	color = ['r-','g-','b-','o-','y-']
	for idx, fitnessName in enumerate(fitnessLabels):
		fitnessScore = data[data.label == fitnessName].score
		plt.plot(range(1,len(fitnessScore)+1), fitnessScore, color[idx])

	plt.legend(fitnessLabels, fontsize = 25, loc = 4)
	plt.xlabel('Generation', fontsize = 25)
	plt.ylabel('Score', fontsize = 25)
	plt.tick_params(labelsize = 25)
	plt.savefig(outputFolder + experimentLabel +'_result2.png')


if __name__ == "__main__":
	if (len(sys.argv) <= 2):
		print ("Sorry, the number of experiment is not enough.")
	else:
		main(sys.argv[1], sys.argv[2:])