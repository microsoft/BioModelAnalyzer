/*
 * main.cpp
 *
 *  Created on: 14 Feb 2014
 *      Author: np183
 */


#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#include <utility>
#include <iterator>
#include <numeric>
#include "Simulation.h"

using std::cout;
using std::cin;
using std::endl;
using std::string;
using std::ifstream;
using std::vector;
using std::pair;

void options() {
	cout << "Please choose:" << endl;
	cout << "(0) Load a new program file." << endl;
	cout << "(1) Check for the time overlap of two cells." << endl;
}

int main() {
	Simulation* s{nullptr};

	while (true) {
		char which(0);
		options();
		cin >> which;
		switch (which) {
		case '0': {
			cout << "Please enter the name of a program file:" << endl;
			string file;
			cin >> file;
			ifstream infile(file);
			if (!infile) {
				cout << "Failed to open file " << file << ". Please try again." << endl;
				break;
			}
			if (s) {
				delete s;
			}
			s = new Simulation(file);
			s->run("P0");
			cout << "Would you like to see a simulation?" << endl;
			char answer;
			cin >> answer;
			if (answer == 'y') {
				cout << *s;
			}
			break;
		}
		case '1': {
			if (!s) {
				cout << "Please upload a program first." << endl;
				break;
			}
			string name1;
			while (!s->program(name1)) {
				cout << "Please enter the name of the first cell:" << endl;
				cin >> name1;
			}
			string name2;
			while (!s->program(name2)) {
				cout << "Please enter the name of the second cell:" << endl;
				cin >> name2;
			}
			unsigned int repetitions{0};
			cout << "How many simulations would you like to run?" << endl;
			cin >> repetitions;

			vector<float> results1; // cell1 born before cell2
			vector<float> results2; // cell2 born before cell1
			for (unsigned int i{0} ; i<repetitions ; ++i) {
				s->clear();
				s->run("P0");
				pair<float,bool> p(s->overlap(name1,name2));
				if (p.second)
					results1.push_back(p.first);
				else
					results2.push_back(p.first);
			}

			std::ostream_iterator<float> out_it (std::cout,", ");
			if (results1.size()) {
				cout << name1 << " born before " << name2 << ":" << endl;
				std::copy (results1.begin(), results1.end(),out_it);
				cout << endl;
				cout << "Average: " << std::accumulate(results1.begin(),results1.end(),0.0)/results1.size() << endl;
			}

			if (results2.size()) {
				cout << name2 << " born before " << name1 << ":" << endl;
				std::copy (results2.begin(),results2.end(),out_it);
				cout << endl;
				cout << "AVerage: " << std::accumulate(results2.begin(),results2.end(),0.0)/results2.size() << endl;

			}
			break;
		}
		default:
			cout << "Bad option. Please try again." << endl;
		}
	}
	}

