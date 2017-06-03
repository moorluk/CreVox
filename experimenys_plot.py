import pandas as pd
import numpy as np
import matplotlib.pyplot as plt

def main():
	data = pd.read_csv('expriment.csv')
	trap = data['FitnessTrap']
	treasure = data['FitnessTreasure']
	dominator = data['FitnessDominator']
	allScore = data['all']

	plt.figure(1)
	plt.plot(range(len(data)),trap,'r-')
	plt.plot(range(len(data)),treasure,'b-')
	plt.plot(range(len(data)),dominator,'g-')
	plt.legend(['trap','treasure','dominator'])
	plt.xlabel('gene')
	plt.ylabel('Score')
	plt.savefig('result.png')

	plt.figure(2)
	plt.plot(range(len(data)),allScore,'r-')
	plt.legend(['all'])
	plt.xlabel('gene')
	plt.ylabel('Score')
	plt.savefig('result2.png')

if __name__ == '__main__':
    main()