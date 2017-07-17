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
	for experiment in experiments:
		print ("Current experiment {}.".format(experiment))
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
	outputData = DataFrame(columns =  ["run","generation","chromosome",'label','score'])
	# Read file.
	data = pd.read_csv(inputFolder + "score_1.csv")
	numRun = data['run'].max()
	numGeneration = data['generation'].max()
	chromosomeCount = data['chromosome'].max()

	# Read setting file.
	settingData = pd.read_csv(inputFolder + "setting.csv")


	# Get input file
	for nR in range(1, numRun + 1, 1):
		# get all of label in this csv file.
		fitnessLabels = set(data.label.values)
		# Get absolute maximum for normalizing.
		fitnessMaximum = []
		for fitnessName in fitnessLabels:
			scores = data[(data.label == fitnessName)].score.values
			fitnessMaximum.append(max(abs(s) for s in scores))

		for ng in range(1, numGeneration + 1):
			#calculate all of chromosome score.
			chromosomes = []
			for chromosomeNumber in range(1, chromosomeCount + 1):
				chromosomeLabels = data[(data.run == nR) & (data.generation == ng) & (data.chromosome == chromosomeNumber)].label.values
				chromosomeScores = data[(data.run == nR) & (data.generation == ng) & (data.chromosome == chromosomeNumber)].score.values
				chromosomeScore = 0.0
				for idx, nowlabel in enumerate(chromosomeLabels):
					labelName = chromosomeLabels[idx]
					weight = settingData[(settingData.label == labelName)].weight.values[0]
					chromosomeScore += chromosomeScores[idx] * weight
				#chromosomeScore = data[(data.run == nR) & (data.generation == ng) & (data.chromosome == chromosomeNumber)].score.sum()
				chromosomes.append(chromosomeScore)
			#find index of the best score chromosome.
			#+1 is fixed the index is from 0,but data is from 1.
			chromosomeID = chromosomes.index(max(chromosomes)) + 1
			for idx, fitnessName in enumerate(fitnessLabels):
				labelScore = data[(data.run == nR) & (data.generation == ng) & (data.chromosome == chromosomeID) & (data.label == fitnessName)].score.sum()
				# Normalize.
				if fitnessMaximum[idx] > 0:
					# Constant c.
					c = 2
					labelScore = (float(labelScore) / float(fitnessMaximum[idx])) ** (1.0 / c)
				else:
					labelScore = 0.0
				weight = settingData[(settingData.label == fitnessName)].weight.values[0]
				labelScore = labelScore * weight
				info = [nR,ng,chromosomeID,fitnessName,labelScore] #,data.iloc[chromosomeID].volume]
				# add info to the last.
				outputData.loc[-1] = info
				# shifting index
				outputData.index = outputData.index + 1
	#fixed [run,generation,chromosome] are float,not int.
	outputData['run'] = outputData['run'].astype(int)
	outputData['generation'] = outputData['generation'].astype(int)
	outputData['chromosome'] = outputData['chromosome'].astype(int)
	# output result table
	outputData.to_csv(outputFolder + "bestChromosome_" + label + ".csv", index = False)
	return outputData

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
	try:
		root        = os.path.dirname(os.path.realpath(__file__))
		datasetPath = os.path.realpath(root + "./datasets/")

		main(root, os.listdir(datasetPath))

		input("Press Enter to continue...")
		sys.exit(0)

	except Exception as ex:
		print ex
