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
#include <memory>
#include <map>
#include "Directive/Divide.h"
#include "State.h"
#include "Simulation.h"
#include "HelperFunctions.h"

using std::map;
using std::string;
using std::stringstream;
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
using std::unique_ptr;
// using boost::lexical_cast;

const int NUMBER_OF_LINES_TO_SKIP{1};

bool Simulation::EventPtrComparison::operator() (Event* lhs, Event* rhs) const {
	return (rhs->operator<(*lhs));
}

Simulation::Simulation() : _currentTime(0.0) {
}

Simulation::Simulation(const string& filename) : _currentTime(0.0) {
	readFile(filename);
}


Simulation::~Simulation() {
	clear();
	for (auto prog : _programs) {
		delete prog.second;
	}
}

void Simulation::run(const string& initial) {
	auto cellState=_parseCellWithState(initial);
	const string cellName{cellState.first};
	// This is unique pointer so that I can use throw.
	unique_ptr<State> state(cellState.second);

	auto firstProg(_programs.find(cellName));
	if (firstProg==_programs.end()) {
		string err{"Could not find the initial cell: "};
		err+=cellName;
		throw err;
	}

	CellProgram* cellProg(firstProg->second);
	vector<Event*> events(cellProg->firstEvent(_currentTime,state.get()));

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
		vector<Event*> nextEvents{current->execute()};
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
	for (auto event : _log) {
		delete event;
	}
	_log.clear();

	for (auto cell : _cells) {
		delete cell.second;
	}
	_cells.clear();

	_currentTime=0.0;
}

// There's an implicit assumption here that the first time a name is
// mentioned the cell is born and that the last time the the cell is mentioned
// it dies.
pair<float,bool> Simulation::overlap(const string& name1, const string& name2) const {
	float b1{-1.0},d1{-1.0},b2{-1.0},d2{-1.0};
	for (auto event : _log) {
		if (event->concerns(name1)) {
			if (b1<0.0) {
				b1 = event->execTime();
			}
			else {
				d1 = event->execTime();
			}
		}

		if (event->concerns(name2)) {
			if (b2<0.0) {
				b2 = event->execTime();
			}
			else {
				d2 = event->execTime();
			}
		}
	}

	// TODO: What happens if d1==-1.0 or d2==-1.0 ?????

	// b1 ... d1 ... b2 ... d2 --> d1-b2
	// b1 ... b2 ... d1 ... d2 --> d1-b2
	// b1 ... b2 ... d2 ... d1 --> d2-b2
	return make_pair(min(d1,d2)-max(b2,b1),b1<b2);
}

map<string,unsigned int> Simulation::cellCount() const {
	map<string,unsigned int> ret{};

	for (auto nameCellP : _cells) {
		if (ret.find(nameCellP.first) != ret.end()) {
			ret[nameCellP.first]+=1;
		}
		else {
			ret.insert(make_pair(nameCellP.first,1));
		}
	}
	return ret;
}

void Simulation::readFile(const string& filename) {
	ifstream infile(filename);
	if (infile)
		infile >> *this;
	else {
		string err{"Failed to open file "};
		err+=filename;
		throw err;
	}
}

void Simulation::addCell(Cell* c) {
	_cells.insert(make_pair(c->name(),c));
}

vector<Cell*> Simulation::cells(const string& name) const {
	auto beginEnd=_cells.equal_range(name);
	auto begin=beginEnd.first;
	auto end=beginEnd.second;
	vector<Cell*> ret{};
	for (auto it=begin ; it!= end ; ++it) {
		if (it->second->alive()) {
			ret.push_back(it->second);
		}
	}
	return ret;
}

CellProgram* Simulation::program(const string& name) {
	auto prog(_programs.find(name));
	if (prog == _programs.end()) {
		return nullptr;
	}
	return prog->second;
}

unsigned int Simulation::numPrograms() const {
	return _programs.size();
}

bool Simulation::expressed(const string& cond) const {
	if (cond.find('[')==std::string::npos ||
		cond.find(']')==std::string::npos ||
		cond.find(']') < cond.find('[')) {
		 const string err{"Trying to evaluate a local condition on the simulation."};
		 throw err;
	}

	string var{cond.substr(0,cond.find('['))};
	string cellName{cond.substr(cond.find('[')+1,cond.find(']')-cond.find('[')-1)};

	vector<Cell*> matchingCells{cells(cellName)};
	for (auto cell : matchingCells) {
		if (cell->expressed(var)) {
			return true;
		}
	}
	return false;
}

string Simulation::toString(unsigned int num) const {
	stringstream temp;
	for (auto ev : _log) {
		temp << num << "," << ev->toString() << "\n";
	}

	return temp.str();
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
	int lineNo{1};
	string buffer;

	// Skip the first two lines
	for (; lineNo < NUMBER_OF_LINES_TO_SKIP+1 ; ++lineNo) {
//		cout << "Skipping line number " << lineNo << "\n";
		getline(in,buffer);
	}

	// Every line is a comma separated thing that includes:
	// Cell Name (string), Cell Cycle Length (float), Standard Deviation (float)
	// Daughter1 (string), Daughter2 (string), some irrelevant mutation info
	while (getline(in,buffer) && buffer.size()!=0) {
		 try {
			 sim._sanitize(buffer);
			 sim._parseLine(buffer);
			 ++lineNo;
		 }
		 catch (const string& err) {
			 stringstream error;
			 error << "Error: on line " << lineNo << ":" << err;
			 const string withLineNum{error.str()};
			 throw withLineNum;
		 }
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

void Simulation::_parseLine(const string& line) {

	string name{};
	string condition{};
	float mean{};
	float sd{};
	string action{};
	string d1{};
	string d2{};

	const vector<string> fields{splitOn(',',line)};

	// cout << "Read: " << buffer << endl;

	for (unsigned int i=0 ; i<static_cast<unsigned int>(LASTDELIM) && i<fields.size() ;
		 ++ i) {
		const string piece{fields.at(i)};

		switch (static_cast<CsvFields>(i)) {
		case NAME:
		{
			name = piece;
		}
		break;
		case CONDITION: {
			condition = piece;
		}
		break;
		case MEANTIME: // Mean time
		{
			std::stringstream str(piece);
			str >> mean;
		}
		break;
		case STANDARDDEV: // Standard Deviation
		{
			std::stringstream str(piece);
			str >> sd;
		}
		break;
		case ACTION:
		{
			action = piece;
		}
		break;
		case DAUGHTER1: // Daughter 1
		{
			d1 = piece;
		}
		break;
		case DAUGHTER2:
		default: // Daughter 2
		{
			d2 = piece;
		}
		}
	}

	auto progIt=_programs.find(name);
	if (progIt==_programs.end()) {
		CellProgram* newProg=new CellProgram(name,this);
		_programs.insert(make_pair(name,newProg));
		progIt=_programs.find(name);
	}

	Condition* cond{new Condition(condition)};
	pair<string,State*> d1S1{_parseCellWithState(d1)};
	pair<string,State*> d2S2{_parseCellWithState(d2)};
	Directive* d{new Divide(mean,sd,progIt->second,d1S1.first,d1S1.second,d2S2.first,d2S2.second)};
	progIt->second->addCondition(cond,d);
}

bool notIsPrint (char c)
{
    return !isprint((unsigned)c);
}

void Simulation::_sanitize(string & str)
{
    str.erase(remove_if(str.begin(),str.end(), notIsPrint) , str.end());
}

pair<string,State*> Simulation::_parseCellWithState(const std::string& cell) const {
	if (cell.find('[')==string::npos) {
		return make_pair(cell,nullptr);
	}
	string stateStr{cell.substr(cell.find('[')+1,cell.find(']')-cell.find('[')-1)};
	State* state{new State(stateStr)};
	string cellName{cell.substr(0,cell.find('['))};

	return make_pair(cellName,state);
}
