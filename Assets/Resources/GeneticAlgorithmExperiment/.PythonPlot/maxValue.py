# When you first execute the program, please install these packages below.
# C:/Python27/python.exe -m pip install -U pip pandas numpy matplotlib

import sys
import os
#get the max value of table
from pandas import Series, DataFrame
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt

# Our program.
def main(root, experiments):
    # Root of folder.
    root = os.path.dirname(root)
    # Number of file.
    numRun = 1
    # Num of generation.
    numGeneration = 2
    # Count of chromosome each generation.
    chromosomeCount = 6
    # The path of dataset.
    nameOfRun = ["Block_100_100", "Graud_100_100"]
    inputFolderRoot  = root + "./datasets/"
    outputFolderRoot = root + "./Exports/BestChromosome/"

    if not os.path.exists(outputFolderRoot):
        os.makedirs(outputFolderRoot)
    else:
        for file in os.listdir(outputFolderRoot):
            os.remove(outputFolderRoot + file)

    for experiment in experiments:
        inputFolder = inputFolderRoot + experiment + "/"
        exportTheBestChromosome(experiment, numRun, numGeneration, chromosomeCount, inputFolder, outputFolderRoot)
        # plotGenerations(outputFolderRoot)
    # ...
    # dataA = pd.read_csv(outputFolderRoot + "bestChromosome_Block_100_100.csv")
    # dataB = pd.read_csv(outputFolderRoot + "bestChromosome_Graud_100_100.csv")
    # plotGenerationsWithSD([dataA, dataB], numRun, numGeneration, outputFolderRoot)

# Export the best chromosomes table (single fitness score / all / run / class).
def exportTheBestChromosome(label, numRun, numGeneration, chromosomeCount, inputFolder, outputFolder):
    run = DataFrame(columns =  ["Run no.","Gen no.","Chm no.",'label','score','volume'])
    # Get input file
    for i in range(1, numRun + 1, 1):
        # Read file.
        fitnessScores = pd.read_csv(inputFolder + "experiment_" + str(i) + ".csv")
        fitnessNames = set()
        for name in fitnessScores.label.values:
            had = name in fitnessNames
            if(had):
                break
            fitnessNames.add(name)

        # for ng in range(0, numGeneration, 1):
        #     locals()['fitnessScores_run' + str(i) + '_gen' + str(ng + 1)] = fitnessScores[ng * chromosomeCount : ng * chromosomeCount + chromosomeCount]
        # Create table that stores the best chromosome each generation.
        # newColumns = list(fitnessScores)[0 : len(fitnessScores.columns)]
        # newColumns.extend(['run'])
        # bestChromosomes = DataFrame(columns = newColumns)
        # Get the max value from every Generations.
        # e.g. 100 numGeneration has 100 best chromosome each generation.
        # for ng in range(0, numGeneration, 1):
        #     #Copy the fitnessScores each generation
        #     fitnessScoresInOneGeneration = locals()['fitnessScores_run' + str(i) + '_gen' + str(ng + 1)]
        #     #Get the index of max value from fitnessScores each generation
        #     indexOfMaxValue = fitnessScoresInOneGeneration['score'].argmax() # % chromosomeCount
        #     bestChromosome = fitnessScoresInOneGeneration.loc[[indexOfMaxValue]]
        #     bestChromosome['run'] = i
        #     bestChromosomes = bestChromosomes.append(bestChromosome)
        # run = pd.concat([run, bestChromosomes])

        for ng in range(1,numGeneration+1):
            chromosomes = []
            for chromosomeNumber in range(1,chromosomeCount+1):
                chromosomeScore = fitnessScores[(fitnessScores.generation == ng) & (fitnessScores.chromosome == chromosomeNumber)].score.sum()
                chromosomes.append(chromosomeScore)
            chromosomeID = chromosomes.index(max(chromosomes))
            for fitnessName in fitnessNames:
                info = [i,ng,chromosomeID+1,fitnessName,max(chromosomes),'volume A']
                run.loc[-1] = info
                run.index = run.index + 1  # shifting index
 

    # output result table
    run.to_csv(outputFolder + "bestChromosome_" + label + ".csv",index = False)

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
    plt.savefig(outputFolder + 'result.png')

def plotGenerationsWithSD(dataset, numRun, numGeneration, outputFolder):
    #setup the figure
    plt.figure(figsize = (16, 9), dpi = 120)
    # Get input file
    for idx, data in enumerate(dataset): 
        generationMeanList = list()
        generationStdList = list()
        for i in range(1, numRun + 1):
            generation = np.array([data.ix[j * numGeneration + i - 1, :]['all'] for j in range(numRun)])
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