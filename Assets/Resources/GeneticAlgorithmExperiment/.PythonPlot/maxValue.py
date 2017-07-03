# When you first execute the program, please install these packages below.
# C:/Python27/python.exe -m pip install -U pip pandas numpy matplotlib

import sys
import os
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
    # Number of file.
    numRun = 1
    # Num of generation.
    numGeneration = 1
    # Count of chromosome each generation.
    chromosomeCount = 6
    # The path of dataset.
    inputFolderRoot  = root + "./datasets/"
    outputFolderRoot = root + "./Exports/BestChromosome/"

    if not os.path.exists(outputFolderRoot):
        os.makedirs(outputFolderRoot)
    else:
        for file in os.listdir(outputFolderRoot):
            os.remove(outputFolderRoot + file)

    for experiment in experiments:
        inputFolder = inputFolderRoot + experiment + "/"
        theBestChromosome = exportTheBestChromosome(experiment, inputFolder, outputFolderRoot)
        newPlot(experiment,outputFolderRoot,theBestChromosome)
        newPlot2(experiment,outputFolderRoot,theBestChromosome)
        # plotGenerations(outputFolderRoot)
        # ...
        # dataA = pd.read_csv(outputFolderRoot + "bestChromosome_"+experiment+".csv")
        # plotGenerationsWithSD(dataA, numRun, numGeneration, outputFolderRoot)

# Export the best chromosomes table (single fitness score / all / run / class).
def exportTheBestChromosome(label, inputFolder, outputFolder):
    run = DataFrame(columns =  ["run","generation","chromosome",'label','score','volume'])
    # Read file.
    data = pd.read_csv(inputFolder + "experiment_1.csv")
    numRun = data['run'].max()
    numGeneration = data['generation'].max()
    chromosomeCount = data['chromosome'].max()
    # Get input file
    for i in range(1, numRun + 1, 1):
        # get all of label in this csv file.
        fitnessNames = set()
        for name in data.label.values:
            had = name in fitnessNames
            if(had):
                break
            fitnessNames.add(name)
        #
        for ng in range(1,numGeneration+1):
            #calculate all of chromosome score.
            chromosomes = []
            for chromosomeNumber in range(1,chromosomeCount+1):
                chromosomeScore = data[(data.run == i) & (data.generation == ng) & (data.chromosome == chromosomeNumber)].score.sum()
                chromosomes.append(chromosomeScore)
            #find index of the best score chromosome.
            #+1 is fixed the index is from 0,but data is from 1.
            chromosomeID = chromosomes.index(max(chromosomes)) +1
            for fitnessName in fitnessNames:
                labelScore = data[(data.run == i) & (data.generation == ng) & (data.chromosome == chromosomeID) & (data.label == fitnessName)].score.sum()
                info = [i,ng,chromosomeID,fitnessName,labelScore,data.iloc[chromosomeID].volume]
                # add info to the last.
                run.loc[-1] = info
                # shifting index
                run.index = run.index + 1

    # output result table
    run.to_csv(outputFolder + "bestChromosome_" + label + ".csv",index = False)
    return run

def newPlot(experimentLabel,outputFolder,data):
    plt.figure()
    numRun = max(data['run'])
    numGeneration = max(data['generation'])

    generationMeanList = list()
    generationStdList = list()
    for generation in range(numGeneration):
        plotData = []
        for run in range(numRun):
            generationsScore = sum(data[(data.run == run) & (data.generation == generation)].score)
            plotData.append(generationsScore)
        generationStdList.insert(generation, np.std(plotData))
        generationMeanList.insert(generation, np.mean(plotData))
    
    plt.errorbar(range(len(generationMeanList)), generationMeanList, generationStdList, marker = 'o', alpha = 0.3)

    plt.xlabel('Generation')
    plt.ylabel('Score')
    plt.savefig(outputFolder + experimentLabel +'_result.png')

def newPlot2(experimentLabel,outputFolder,data):
    plt.figure()
    fitnessNames = set()
    for name in data.label.values:
        had = name in fitnessNames
        if(had):
            break
        fitnessNames.add(name)

    color = ['r-','g-','b-','o-','y-']
    i=0
    for fitnessName in fitnessNames:
        tt = data[data.label == fitnessName].score
        plt.plot(range(len(tt)), tt, color[i])
        i+=1

    plt.legend(fitnessNames)
    plt.xlabel('Generation')
    plt.ylabel('Score')
    plt.savefig(outputFolder + experimentLabel +'_result2.png')

def plotGenerations(outputFolder):
    plt.figure()
    # Read file.
    data = pd.read_csv(outputFolder + "bestChromosome.csv")
    # Get input file
    for i in range(1, numRun + 1, 1):
        bestChromosomes = data[data['run'] == i]['score']
        plt.plot(range(len(bestChromosomes)), bestChromosomes, 'b-')

    plt.legend(['generation'])
    plt.xlabel('Generation')
    plt.ylabel('Score')
    plt.savefig(outputFolder + '_result.png')

def plotGenerationsWithSD(dataset, numRun, numGeneration, outputFolder):
    #setup the figure
    plt.figure(figsize = (16, 9), dpi = 120)
    # Get input file
    for idx, data in enumerate(dataset): 
        generationMeanList = list()
        generationStdList = list()
        for i in range(1, numRun + 1):
            generation = np.array([data.ix[j * numGeneration + i - 1, :]['score'] for j in range(numRun)])
            generationStdList.insert(i - 1, np.std(generation))
            generationMeanList.insert(i - 1, generation.mean())
        plt.errorbar(range(len(generationMeanList)), generationMeanList, generationStdList, marker = 'o', alpha = 0.3)
    plt.legend(['50', '100', '200'], fontsize = 25, loc = 4)
    plt.xlabel('Generation', fontsize = 25)
    plt.ylabel('Score', fontsize = 25)
    plt.tick_params(labelsize = 25)
    plt.savefig(outputFolder + 'resultPlot.png')

if __name__ == "__main__":
    if (len(sys.argv) <= 2):
        print ("Sorry, the number of experiment is not enough.")
    else:
        main(sys.argv[1], sys.argv[2:])