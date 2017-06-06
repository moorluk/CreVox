// Native.
const fs      = require('fs');
// Packages.
const _       = require('underscore');
const open    = require('open');
const express = require('express');
const app     = express();

let LISTEN_PORT = 80;

// // Open the browser.
// open('http://localhost:' + LISTEN_PORT + '/fitnessfunctions');

fs.readdirSync("C:/Users/Salmon/AppData/LocalLow/DefaultCompany/Voxel2/Experiments/Exports/BestChromosome").forEach(file => {
	let matched = file.match(/^bestChromosome_(\w+)_(\d+)_(\d+).csv$/);
	if (matched) {
		let metrics     = matched[1];
		let runs        = matched[2];
		let chromosomes = matched[3];
		console.log(metrics, runs, chromosomes);
	}
})

app.get('/fitnessfunctions/', function(req, res) {
	res.send("hello world.");
});

app.get('/fitnessfunctions/:metrics', function(req, res) {
	const params = req.params;
	// Fields.
	let metrics = req.params['metrics'];

	// Select the experiments.
	switch (metrics) {
	case 'A':
		break;
	default:
		break;
	}
	res.send("done");
});

app.listen(LISTEN_PORT, function () {
	// Start listening.
	console.log('Listening on port ' + LISTEN_PORT + '.');
});