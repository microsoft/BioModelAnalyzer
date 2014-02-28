/*
 * main.cpp
 *
 *  Created on: 14 Feb 2014
 *      Author: np183
 */


#include <iostream>
#include <iomanip>
#include <fstream>
#include <string>
#include <vector>
#include <utility>
#include <iterator>
#include <numeric>
#include <math.h>
#include <algorithm>
#include <tuple>
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


			auto statistics=[&](vector<float>& vec) -> vector<float> {
				std::sort(vec.begin(),vec.end());
				float min(vec[0]);
				float max(vec[vec.size()-1]);
				float mean(std::accumulate(vec.begin(),vec.end(),0.0)/vec.size());
				float accum = 0.0;
				std::for_each (vec.begin(), vec.end(), [&](const double d) {
				    accum += (d - mean) * (d - mean);
				});
				float stdev = std::sqrt(accum / (vec.size()-1));
				float q1(vec.size()%4 ? (vec[vec.size()/4-1]+vec[vec.size()/4])/2 : vec[vec.size()/4]);
				float median(vec.size()%2 ? (vec[vec.size()/2-1]+vec[vec.size()/2])/2: vec[vec.size()/2]);
				float q3(vec.size()%4 ? (vec[3*vec.size()/4-1]+vec[3*vec.size()/4])/2 : vec[3*vec.size()/4]);
				return vector<float> {min,max,mean,stdev,q1,median,q3};
			};

			vector<string> names{"q1","min","median","max","q3","mean+std","mean-std","mean","stdev"};
			auto print_entry=[&](const string& name, const float& val) {
				cout << "| " << std::setw(8) << std::left << name << " | " << std::setw(6) << std::fixed << std::setprecision(2) << std::right << val << " |" << endl;
			};
			auto print_sep=[](unsigned int len) {
				std::cout.fill('-');
				std::cout.width(len);
				std::cout << "" << endl;
				std::cout.fill(' ');
			};
			auto print_all=[&](vector<float>& vec) {
				// std::ostream_iterator<float> out_it (std::cout,", ");
				// std::copy (results1.begin(), results1.end(),out_it);
				// cout << endl;


				vector<float> stat{statistics(vec)};

				print_sep(8+7+6);

				unsigned int oldprecision(cout.precision());
				cout.precision(5);
				vector<float> vals{stat[4],stat[0],stat[5],stat[1],stat[6],stat[2]+stat[3],stat[2]-stat[3],stat[2],stat[3]};
				for (unsigned int i=0 ; i<names.size() ; ++i) {
					print_entry(names[i],vals[i]);
				}
				cout.precision(oldprecision);
				print_sep(8+7+6);
			};

			if (results1.size()) {
				cout << name1 << " born before " << name2 << ":" << endl;
				print_all(results1);
			}

			if (results2.size()) {
				cout << name2 << " born before " << name1 << ":" << endl;
				print_all(results2);
			}
			break;
		}
		default:
			cout << "Bad option. Please try again." << endl;
		}
	}
	}

