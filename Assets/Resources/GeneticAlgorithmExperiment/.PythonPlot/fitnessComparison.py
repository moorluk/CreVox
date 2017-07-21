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
	data = pd.read_csv(inputFolder + "score.csv")
	data['score'] = data['score'].astype(float)
	numRun = data['run'].max()
	numGeneration = data['generation'].max()
	chromosomeCount = data['chromosome'].max()

	# Read setting file.
	settingData = pd.read_csv(inputFolder + "setting.csv")

	# Get all of label in this csv file.
	fitnessLabels = set(data.label.values)
	# Get absolute maximum for normalizing.
	fitnessMaximum = {}
	for fitnessName in fitnessLabels:
		scores = data[(data.label == fitnessName)].score.values
		fitnessMaximum[fitnessName] = (max(abs(s) for s in scores))

	# Normalize and weight.
	for idx, val in enumerate(data.values):
		_label = val[3]
		_score = val[4]
		_max = fitnessMaximum[_label]
		if _max > 0.0:
			# Constant c.
			c = 2
			_score = (float(_score) / float(_max)) ** (1.0 / c)
		else:
			_score = 0.0
		weight = settingData[(settingData.label == _label)].weight.values[0]
		_score = float(_score) * float(weight)
		data.set_value(idx,'score',_score)

	# Get input file
	for nR in range(1, numRun + 1, 1):
		for ng in range(1, numGeneration + 1):
			#calculate all of chromosome score.
			chromosomes = []
			for chromosomeNumber in range(1, chromosomeCount + 1):
				chromosomeScore = data[(data.run == nR) & (data.generation == ng) & (data.chromosome == chromosomeNumber)].score.sum()
				chromosomes.append(chromosomeScore)
			#find index of the best score chromosome.
			#+1 is fixed the index is from 0,but data is from 1.
			chromosomeID = chromosomes.index(max(chromosomes)) + 1
			for idx, fitnessName in enumerate(fitnessLabels):
				labelScore = data[(data.run == nR) & (data.generation == ng) & (data.chromosome == chromosomeID) & (data.label == fitnessName)].score.sum()
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
	fitnessLabels = set(data.label.values)
	numRun = max(data['run'])
	numGeneration = max(data['generation'])
	color = ['r-','g-','b-','o-','y-']
	for run in range(1,numRun+1):
		plt.figure(figsize = (16, 9), dpi = 120)
		for idx, fitnessName in enumerate(fitnessLabels):
			#fitnessScore = data[data.label == fitnessName].score
			#plt.plot(range(1,len(fitnessScore)+1), fitnessScore, color[idx])
			fitnessScore = []
			for generation in range(1,numGeneration+1):
				fitnessScore.append(data[(data.run == run) & (data.generation == generation) & (data.label == fitnessName)].score.sum())
			plt.plot(range(1,numGeneration+1), fitnessScore, color[idx])

		plt.legend(fitnessLabels, fontsize = 25, loc = 4)
		plt.xlabel('Generation', fontsize = 25)
		plt.ylabel('Score', fontsize = 25)
		plt.tick_params(labelsize = 25)
		plt.savefig(outputFolder + experimentLabel + "_" + str(run) +'_result2.png')


if __name__ == "__main__":
	try:
		root        = os.path.dirname(os.path.realpath(__file__))
		datasetPath = os.path.realpath(root + "./datasets/")

		main(root, os.listdir(datasetPath))

		input("Press Enter to continue...")
		sys.exit(0)

	except Exception as ex:
		print ex
