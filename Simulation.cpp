/*
 * Simulation.cpp
 *
 *  Created on: 14 Feb 2014
 *      Author: np183
 */

#include <iostream>
#include <sstream>
#include <fstream>
#include <queue>
#include <vector>
#include <deque>
#include <algorithm>
#include "Simulation.h"

using std::string;
using std::pair;
using std::make_pair;
using std::vector;
using std::deque;
using std::ifstream;
using std::istream;
using std::ostream;
using std::getline;
using std::cerr;
using std::cout;
using std::endl;
using std::size_t;
using std::make_pair;
using std::min;
using std::max;
// using boost::lexical_cast;

Simulation::Simulation() : _currentTime(0.0) {
	// TODO Auto-generated constructor stub
}

Simulation::Simulation(const string& filename) : _currentTime(0.0) {
	readFile(filename);
}


Simulation::~Simulation() {
	for (auto event : _log) {
		delete event;
	}

	for (auto prog : _programs) {
		delete prog.second;
	}
}

void Simulation::run(const string& initial) {
	auto firstProg(_programs.find(initial));
	if (firstProg==_programs.end()) {
		cerr << "Could not find the initial cell" << endl;
		return;
	}

	string name(firstProg->first);
	Cell* cell(firstProg->second);
	vector<Event*> events(cell->firstEvent(_currentTime));

	class EventPtrComparison
	{
	public:
	  bool operator() (Event* lhs,  Event* rhs) const
	  {
		  return ((*rhs)<(*lhs));
	  }
	};

	std::priority_queue<Event*, deque<Event*>, EventPtrComparison> _pending;

	for (auto event : events) {
		_pending.push(event);
	}

	while (!_pending.empty()) {
		Event* current=_pending.top();
		_pending.pop();

		_currentTime = current->execTime();

		// TODO:
		// If you want events to fail then they should throw
		// an exception!
		vector<Event*> nextEvents(current->execute());
		// cout << *current << endl;
		_log.push_back(current);

		for (auto ev : nextEvents) {
			_pending.push(ev);
		}
		// TODO:
		// An event (change state) could have the state of the program
		// and then actually apply the state change
		// The events are actually the things that are alive!
		// Event should have the relative time and the absolute time
		// When it is created it has the relative time (for the p-q) and
		// when it is executed it logs the absolute time when
		// it happened.
		// What about activeCells and allCells?
	}
}

void Simulation::clear() {
	_log.clear();
	_currentTime=0.0;
}

pair<float,bool> Simulation::overlap(const string& name1, const string& name2) const {
	float b1{-1.0},d1{-1.0},b2{-1.0},d2{-1.0};
	for (auto event : _log) {
		if (event->concerns(name1)) {
			if (b1<0.0) {
				b1 = event->execTime();
			}
			d1 = event->execTime();
		}

		if (event->concerns(name2)) {
			if (b2<0.0) {
				b2 = event->execTime();
			}
			d2 = event->execTime();
		}
	}

	cout << "Birth1 " << b1 << " death1 " << d1 << endl;
	cout << "Birth2 " << b2 << " death2 " << d2 << endl;
	// b1 ... d1 ... b2 ... d2 --> d1-b2
	// b1 ... b2 ... d1 ... d2 --> d1-b2
	// b1 ... b2 ... d2 ... d1 --> d2-b2
	return make_pair(min(d1,d2)-max(b2,b1),b1<b2);
}

void Simulation::readFile(const string& filename) {
	ifstream infile(filename);
	if (infile)
		infile >> *this;
	else
		cerr << "Failed to open file " << filename << endl;
}


Cell* Simulation::program(const string& name) {
	auto prog(_programs.find(name));
	if (prog == _programs.end()) {
		return nullptr;
	}
	return prog->second;
}

unsigned int Simulation::numPrograms() const {
	return _programs.size();
}


ostream& operator<< (ostream& out, const Simulation& sim) {
//	for (auto prog : sim._programs) {
//		out << *prog.second << endl;
//	}

	for (auto ev : sim._log) {
		out << *ev << endl;
	}

	return out;
}

istream& operator>> (istream& in, Simulation& sim) {
	string buffer;
	// Every line is a comma separated thing that includes:
	// Cell Name (string), Cell Cycle Length (float), Standard Deviation (float)
	// Daughter1 (string), Daughter2 (string), some irrelevant mutation info
	while (getline(in,buffer) && buffer.size()!=0) {
		 string name,d1,d2;
		 float meanCycle{0.0}, sd{0.0};
		 sim._parseLine(buffer,name,meanCycle,sd,d1,d2);
		 Cell* newCell(new Cell(name,meanCycle,sd,d1,d2,&sim));
		 (sim._programs).insert(make_pair(name,newCell));
//		 cerr << "Read cell: " << *newCell << endl;
	}
	if (!in.eof()) {
		cerr << "Something went wrong with reading" << endl;
		return in;
	}

//  // This part of the code checks that every program mentioned is defined
//	// But actually, not all programs are defined
//	// The last cells that have no descendants are not defined ...
//	for (auto prog : sim._programs) {
//		for (auto otherprog : prog.second->otherPrograms()) {
//			if (sim._programs.find(otherprog)==sim._programs.end()) {
//				cerr << "Program " << otherprog << " mentioned but does not exist!" << endl;
//				in.setstate(std::ios::failbit);
//				return in;
//			}
//		}
//	}

	in.clear();
	return in;
}


void Simulation::_parseLine(const string& line,string& name,float& mean, float& sd, string& d1,string& d2) {
	const string whitespace=" \n\t";
	const string comma=",";
	const string wsc=whitespace+comma;

	// cout << "Read: " << buffer << endl;
	size_t current(line.find_first_not_of(whitespace));
	size_t next(line.find_first_of(wsc,current+1));
	for (unsigned int i=0 ; i<5 ; ++ i) {
		const string piece(line.substr(current,next-current));

		switch (i) {
		case 0: {
			name = piece;
		}
		break;
		case 1:
		{
			std::stringstream str(piece);
			str >> mean;
		}

		break;
		case 2: {
			std::stringstream str(piece);
			str >> sd;
		}
		break;
		case 3: {
			d1 = piece;
		}
		break;
		case 4:
		default: {
			d2 = piece;
		}
		}

		current = line.find_first_not_of(wsc,next+1);
		next=line.find_first_of(wsc,current+1);
	}


//			 cout << "Name: |" << name << "|" << endl;
//			 cout << "Mean: |" << mean << "|" << endl;
//			 cout << "SD: |" << sd << "|" << endl;
//			 cout << "D1: |" << d1 << "|" << endl;
//			 cout << "D2: |" << d2 << "|" << endl;
}
