// Native.
const fs      = require('fs');
// Packages.
const _       = require('underscore');
const open    = require('open');
const express = require('express');
const parser  = require('csv-parse');
const app     = express();

let LISTEN_PORT = 80;
let rawDatasetPath = "C:/Users/Salmon/AppData/LocalLow/DefaultCompany/Voxel2/Experiments/Exports/BestChromosome/";

// // Open the browser.
// open('http://localhost:' + LISTEN_PORT + '/fitnessfunctions');

// Set the HTML template engine.
app.set('view engine', 'pug');
// Set the static folders.
app.use(express.static('public'));

fs.readdirSync(rawDatasetPath).forEach(file => {
	let matched = file.match(/^bestChromosome_(\w+)_(\d+)_(\d+).csv$/);
	if (matched) {
		let metrics     = matched[1];
		let runs        = matched[2];
		let individuals = matched[3];

		console.log(metrics, runs, individuals);
	}
});

app.get('/fitnessfunctions/', (req, res) => {
	res.send(rawDatasetPath);
});

app.get('/fitnessfunctions/:metrics', (req, res) => {
	const params = req.params;
	// Fields.
	let metrics = req.params['metrics'];
	let csvPath = rawDatasetPath;

	// Select the experiments.
	csvPath += './bestChromosome_' + metrics + '.csv';
	fs.readFile(csvPath, 'utf8', (err, input) => {
		if (err) {
			res.render('index', { 'fitnessscores': [] });
		} else {
			parser(input, (err, output) => res.render('index', { 'fitnessscores': output }));
		}
	});
});

app.listen(LISTEN_PORT, () => {
	// Start listening.
	console.log('Listening on port ' + LISTEN_PORT + '.');
});