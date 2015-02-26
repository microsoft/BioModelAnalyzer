/*
 * Simulation.cpp
 *
 *  Created on: 14 Feb 2014
 *      Author: np183
 */

#include <iostream>
#include <sstream>
#include <fstream>
#include <stack>
#include <vector>
#include <queue>
#include <algorithm>
#include <memory>
#include <map>
#include "Event/Event.h"
#include "Directive/Divide.h"
#include "Directive/StateTransition.h"
#include "Variable/Type.h"
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
using std::map;
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


// Initialize static members
const string Simulation::G1_PHASE="G1";
const string Simulation::G2_PHASE = "G2";
const string Simulation::S_PHASE = "S";
const string Simulation::G0_PHASE = "G0";


const int NUMBER_OF_LINES_TO_SKIP{ 1 };
const string CELL_CYCLE{ "CellCycle" };
const string DIVIDE{ "divide" };
const string CHANGE_STATE{ "change_state" };
const string DEF_INIT{ "default_init" };
const string DEF_LENGTH{ "default_length" };



bool Simulation::EventPtrComparison::operator() (Happening* lhs, Happening* rhs) const {
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
	for (auto type : _types) {
		delete type.second;
	}
}

void Simulation::run(const string& initialProg,
	const string& initialState,
	float initialMean, float initialSD) {

	// Find the right program
	auto firstProg(_programs.find(initialProg));
	if (_programs.end() == firstProg) {
		string err{ "Could not find the initial cell: " };
		err += initialProg;
		throw err;
	}
	CellProgram* cellProg(firstProg->second);

	vector<Happening*> happenings(cellProg->firstEvent(_currentTime, initialState, initialMean, initialSD));

	std::priority_queue<Happening*, deque<Happening*>, EventPtrComparison> pending;

	for (auto h : happenings) {
		pending.push(h);
	}

	try {
		while (!pending.empty()) {
			Happening* current = pending.top();

			_currentTime = current->execTime();

			pair<Event*, vector<Happening*>> nextEvents{ current->execute() };
			// If an exception is thrown by nextEvents then
			// current is still on the queue and will be destroyed by the catch
			// below
			pending.pop();
			delete current;

			if (nextEvents.first != nullptr) {
				_log.push_back(nextEvents.first);
			}

			for (auto h : nextEvents.second) {
				pending.push(h);
			}
		}
	}
	catch (const string& err) {
		while (!pending.empty()) {
			Happening* current = pending.top();
			pending.pop();
			delete current;
		}
		throw err;
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

	_currentTime = 0.0;
}

// There's an implicit assumption here that the first time a name is
// mentioned the cell is born and that the last time the the cell is mentioned
// it dies.
pair<float, bool> Simulation::overlap(const string& name1, const string& name2) const {
	float b1{ -1.0 }, d1{ -1.0 }, b2{ -1.0 }, d2{ -1.0 };
	for (auto event : _log) {
		if (event->concerns(name1)) {
			if (b1 < 0.0) {
				b1 = event->execTime();
			}
			else {
				d1 = event->execTime();
			}
		}

		if (event->concerns(name2)) {
			if (b2 < 0.0) {
				b2 = event->execTime();
			}
			else {
				d2 = event->execTime();
			}
		}
	}

	// TODO: What happens if d1==-1.0 or d2==-1.0 ?????

	// b1 ... d1 ... b2 ... d2 --> (0,true)
	// b1 ... b2 ... d1 ... d2 --> (d1-b2,true)
	// b1 ... b2 ... d2 ... d1 --> (d2-b2,true)
	// b2 ... d2 ... b1 ... d1 --> (0,false)
	// b2 ... b1 ... d2 ... d1 --> (d2-b1,false)
	// b2 ... b1 ... d1 ... d2 --> (d1-b1,false)
	// b1=b2 ... d1 ... d2 --> (d1-b1,false)
	// b1=b2 ... d2 ... d1 --> (d2-b1,false)
	const float zero{ 0.0 };
	return make_pair(max(min(d1, d2) - max(b2, b1), zero), b1 < b2);
}

map<string, unsigned int> Simulation::cellCount() const {
	map<string, unsigned int> ret{};

	for (auto nameCellP : _cells) {
		if (ret.find(nameCellP.first) != ret.end()) {
			ret[nameCellP.first] += 1;
		}
		else {
			ret.insert(make_pair(nameCellP.first, 1));
		}
	}
	return ret;
}

void Simulation::readFile(const string& filename) {
	ifstream infile(filename);
	if (infile)
		infile >> *this;
	else {
		string err{ "Failed to open file " };
		err += filename;
		throw err;
	}
}

void Simulation::addCell(Cell* c) {
	_cells.insert(make_pair(c->name(), c));
}

vector<Cell*> Simulation::cells(const string& name) const {
	auto beginEnd = _cells.equal_range(name);
	auto begin = beginEnd.first;
	auto end = beginEnd.second;
	vector<Cell*> ret{};
	for (auto it = begin; it != end; ++it) {
		if (it->second->alive()) {
			ret.push_back(it->second);
		}
	}
	return ret;
}

CellProgram* Simulation::program(const string& name) {
	auto prog(_programs.find(name));
	if (_programs.end() == prog) {
		return nullptr;
	}
	return prog->second;
}

unsigned int Simulation::numPrograms() const {
	return _programs.size();
}

vector<string> Simulation::programs() const {
	vector<string> res;

	for (auto strProg : _programs) {
		res.push_back(strProg.first);
	}
	return res;
}
const Type* Simulation::type(const string& name) const {
	auto t(_types.find(name));
	if (_types.end() == t) {
		return nullptr;
	}
	return t->second;
}

const EnumType* Simulation::cellCycleType() const {
	return dynamic_cast<const EnumType*>(type(CELL_CYCLE));
}

bool Simulation::expressed(const string& cond, float from, float to) const {
	if (cond.find('[') == std::string::npos ||
		cond.find(']') == std::string::npos ||
		cond.find(']') < cond.find('[')) {
		const string err{ "Trying to evaluate a local condition on the simulation." };
		throw err;
	}

	string cellName{ cond.substr(0, cond.find('[')) };
	string var{ cond.substr(cond.find('[') + 1, cond.find(']') - cond.find('[') - 1) };

	auto rit = _log.rbegin();

	// Skip events that happen after to
	while (rit != _log.rend() && (*rit)->execTime() >= to) {
		++rit;
	}

	// Search events that happen before to and after from
	while (rit != _log.rend() && (*rit)->execTime() > from) {
		if ((var.size() == 0 && (*rit)->concerns(cellName)) ||
			(var.size() != 0 && (*rit)->expressed(cellName, var))) {
			return true;
		}
		++rit;
	}

	if (_currentTime <= to && _currentTime >= from) {
		vector<Cell*> matchingCells{ cells(cellName) };
		for (auto cell : matchingCells) {
			if (var.size() == 0 || cell->expressed(var)) {
				return true;
			}
		}
	}
	else {
		const string err{ "Asking to evalute expression level in the past. This is currently not supported." };
		throw err;
	}
	return false;
}

pair<float, float> Simulation::defTime(const string& phase) const {
	auto t = _defaults.find(phase);
	if (t != _defaults.end()) {
		return t->second;
	}
	return make_pair(-1.0, -1.0);
}


string Simulation::toString(unsigned int num) const {
	stringstream temp;
	for (auto ev : _log) {
		temp << num << "," << ev->toString() << "\n";
	}

	return temp.str();
}

string Simulation::toJson() const {

	if (0 == _log.size()) {
		return "";
	}

	map<string, string> nameJsonMap;
	unsigned int id{ 0 };
	for (auto it = _log.crbegin(); it != _log.crend(); ++it) {
		Event* ev = *it;
		nameJsonMap.insert(make_pair(ev->cell()->name(), ev->toJson(id++, nameJsonMap)));
	}
	string firstCell = _log.at(0)->cell()->name();
	return nameJsonMap[firstCell];
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
	int lineNo{ 1 };
	string buffer;

	sim._addCellCycleType();

	// Skip the first two lines
	for (; lineNo < NUMBER_OF_LINES_TO_SKIP + 1; ++lineNo) {
		//		cout << "Skipping line number " << lineNo << "\n";
		getline(in, buffer);
	}

	// The basic structure of a line is:
	// Cell Name (string), Cell Cycle (G1/S/G2/G0), Condition (string), action (string),
	// Daughter 1 (string), State 1 (string), mean time (float), standard deviation (float),
	// Daughter 2 (string), State 2 (string), mean time (float), standard deviation (float)
	//
	// Currently this can be used in various possible ways:
	// 1. No cell name, cell cycle, empty condition, default_length, mean1, sd1
	//    This will set the basic cell cycle length for all cells
	// 2. Cell name, _, _, default_init, _, initial state, mean1, sd1
	//    This sets the intial state and the mean and sd TOTAL time for this cell (i.e., G1+S+G2)
	// 3. Cell name, start cycle, condition, next cycle, ...
	//    The next cycle phase can be omitted (G1->S, S->G2, G2->divide)
	//    Another option is to have an action called change_state that just changes the state
	//    and does not advance the cell cycle.
	while (getline(in, buffer) && buffer.size() != 0) {
		try {
			sim._sanitize(buffer);
			sim._parseLine(buffer);
			++lineNo;
		}
		catch (const string& err) {
			stringstream error;
			error << "On line " << lineNo << ": " << err;
			const string withLineNum{ error.str() };
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

float readFloat(const string& input) {
	if (0 == input.size()) {
		return -1.0;
	}
	std::stringstream str(input);
	float ret{};
	str >> ret;
	if (!str) {
		const string err{ "Expecting a number for mean time." };
		throw err;
	}
	return ret;
}

enum CsvFields {
	NAME, CELLCYCLE, CONDITION, ACTION,
	DAUGHTER1, STATE1, MEANTIME1, STANDARDDEV1,
	DAUGHTER2, STATE2, MEANTIME2, STANDARDDEV2,
	LASTDELIM
};

enum CellCyclePhases { G1, S, G2, G0 };

struct LineComponents {
	string cellName;
	string cellCycle;
	string condition;
	string action;
	string d1;
	string state1;
	float mean1;
	float sd1;
	string d2;
	string state2;
	float mean2;
	float sd2;

	LineComponents(const vector<string>& fields) {
		for (unsigned int i = 0; i < static_cast<unsigned int>(LASTDELIM) && i < fields.size();
			++i) {
			const string piece{ fields.at(i) };

			switch (static_cast<CsvFields>(i)) {
			case NAME:
				cellName = piece;
				break;
			case CELLCYCLE:
				cellCycle = piece;
				break;
			case CONDITION:
				condition = piece;
				break;
			case ACTION:
				action = piece;
				break;
			case DAUGHTER1: // Daughter 1
				d1 = piece;
				break;
			case STATE1:
				state1 = piece;
				break;
			case MEANTIME1: // Mean time
				mean1 = readFloat(piece);
				break;
			case STANDARDDEV1: // Standard Deviation
				sd1 = readFloat(piece);
				break;
			case DAUGHTER2:
				d2 = piece;
				break;
			case STATE2:
				state2 = piece;
				break;
			case MEANTIME2: // Mean time
				mean2 = readFloat(piece);
				break;
			case STANDARDDEV2: // Standard Deviation
				sd2 = readFloat(piece);
				break;
			default:
				const string err{ "Too many fields." };
				throw err;
			}
		}
	}
};


void Simulation::_parseLine(const string& line) {

	// Cell Name (string), Cell Cycle (string), 
	// Condition (string), action (string),
	// Daughter 1 (string), State 1 (string), mean1 time (float), standard deviation (float),
	// Daughter 2 (string), State 2 (string), mean1 time (float), standard deviation (float),

	const vector<string> fields{ splitOn(',', line) };


	LineComponents lc(fields);
	// cout << "Read: " << buffer << endl;

	// Handle default length first as it does not require a valid program name
	if (DEF_LENGTH == lc.action) {
		if (!_validCellCycle(lc.cellCycle) || 0 == lc.cellCycle.size() || 
			lc.condition.size() != 0 ||
			lc.d1.size() != 0 || lc.state1.size() != 0 || lc.d2.size() != 0 ||
			lc.state2.size() != 0 || lc.mean2 != -1.0 || lc.sd2 != -1.0) {
			const string err{ "Badly structured default length." };
			throw err;
		}
		_setDefaultTime(lc.cellCycle, lc.mean1, lc.sd1);
		return;
	}

	// Check for the existence of a program name and create a new one 
	// if it does not exist
	if (0 == lc.cellName.size()) {
		const string err{ "Empty cell name." };
		throw err;
	}

	auto progIt = _programs.find(lc.cellName);
	if (_programs.end() == progIt) {
		CellProgram* newProg = new CellProgram(lc.cellName, this);
		_programs.insert(make_pair(lc.cellName, newProg));
		progIt = _programs.find(lc.cellName);
	}

	// Default init
	if (DEF_INIT == lc.action) {
		if (lc.cellCycle.size() != 0 || lc.condition.size() != 0 || lc.d1.size() != 0 ||
			lc.d2.size() != 0 || lc.state2.size() != 0 || lc.mean2 != -1.0 || lc.sd2 != -1.0) {
			const string err{ "Badly structured default initialization." };
			throw err;
		}
		State* st1{ (0 == lc.state1.size() ? nullptr : new State(lc.state1, this)) };
		progIt->second->setDefaults(st1, lc.mean1, lc.sd1);
		return;
	}

	unique_ptr<Condition> cond{ (0 == lc.condition.size() ? nullptr : new Condition(lc.condition)) };
	const EnumType* cellCycleType = dynamic_cast<const EnumType*>(type(CELL_CYCLE));
	_addTypesFromConjunction(lc.state1);
	_addTypesFromConjunction(lc.state2);

	// For change state it is OK for the cellCycle to be empty.
	// In all other cases cell cycle has to be valid
	if (!_validCellCycle(lc.cellCycle) &&
		(lc.action != CHANGE_STATE || lc.cellCycle.size() != 0)) {
		const string err{ "No cell cycle or wrong cell cycle." };
		throw err;
	}

	// Set next default action
	if (0 == lc.action.size()) {
		lc.action = _setDefaultNextAction(lc.cellCycle);
	}

	// Handle remaining possible actions (DIVIDE and different CellCycle phases)
	if (DIVIDE == lc.action) {
		if (0 == lc.d1.size() || 0 == lc.d2.size()) {
			const string err{ "Badly structured divide instruction" };
			throw err;
		}
		cond->addCellCycle(lc.cellCycle);
		// Empty initializer will create the empty state
		State* st1{ new State(lc.state1,this) };
		State* st2{ new State(lc.state2,this) };
		EnumType::Value g1(*cellCycleType, G1_PHASE);
		st1->addCellCycle(g1);
		st2->addCellCycle(g1);
		Directive* d{ new Divide(progIt->second, lc.d1, st1, lc.mean1, lc.sd1, lc.d2, st2, lc.mean2, lc.sd2) };
		progIt->second->addCondition(cond.release(), d);
	}
	else if (G1_PHASE == lc.action || S_PHASE == lc.action || 
			G2_PHASE == lc.action || G0_PHASE == lc.action ||
			CHANGE_STATE == lc.action) {
		if (lc.d1.size() != 0 || lc.d2.size() != 0 || lc.mean2 != -1.0 || lc.sd2 != -1.0) {
			string err{ "Badly structured " };
			err += lc.action;
			err += " instruction";
			throw err;
		}
		if (lc.cellCycle.size() != 0) {
			if (!cond) {
				cond.reset(new Condition("DEFAULT"));
			}
			cond->addCellCycle(lc.cellCycle);
		}
		State* st1{ (0 == lc.state1.size() ? nullptr : new State(lc.state1,this)) };
		if (lc.action != CHANGE_STATE) {
			if (!st1) {
				st1 = new State();
			}
			EnumType::Value p(*cellCycleType, lc.action);
			st1->addCellCycle(p);
		}
		Directive* d{ new StateTransition(progIt->second, lc.mean1, lc.sd1, st1) };
		progIt->second->addCondition(cond.release(), d);
	}
	else {
		const string err{ "Unexpected action." };
		throw err;
	}
}

bool notIsPrint(char c)
{
	return !isprint((unsigned)c);
}

void Simulation::_sanitize(string & str)
{
	str.erase(remove_if(str.begin(), str.end(), notIsPrint), str.end());
}

bool Simulation::_validCellCycle(const string& c) const
{
	if (0 == c.length() ||
		(c != G1_PHASE && c != G2_PHASE && c != S_PHASE)) {
		return false;
	}
	return true;
}

void Simulation::_addTypesFromConjunction(const string& cond) {
	if (cond.size() == 0) {
		return;
	}

	vector<string> fields{ splitOn('&', cond) };
	for (string field : fields) {
		field = removeSpace(field);

		// It is not permitted to write !(var=val)
		// This will be caught later on.
		if (field.at(0) == '!') {
			continue;
		}

		if (field.find('=') != std::string::npos) {
			unsigned int skip = 1;
			std::string::size_type l;
			if ((l = field.find("!=")) != std::string::npos) {
				skip = 2;
			}
			else {
				l = field.find("=");
			}
			string varname = field.substr(0, l);
			string value = field.substr(l + skip, field.length() - l - skip);

			auto it = _types.find(varname);
			if (it == _types.end()) {
				EnumType* e = new EnumType();
				e->addElem(value);
				_types.insert(make_pair(varname, e));
			} 
			else {
				Type* t = it->second;
				// This is not permitted (boolvar=true or boolvar=false)
				// But it will be caught and error reported later
				if (t->type() != Type::Types::ENUM) {
					continue;
				}
				EnumType* e = dynamic_cast<EnumType*>(t);
				e->addElem(value);
			}
		}
	}
}

void Simulation::_addCellCycleType() {
	auto t = _types.find(CELL_CYCLE);
	if (t != _types.end()) {
		return;
	}

	EnumType* cc = new EnumType();
	cc->addElem(G0_PHASE);
	cc->addElem(G1_PHASE);
	cc->addElem(G2_PHASE);
	cc->addElem(S_PHASE);

	_types.insert(make_pair(CELL_CYCLE, cc));
}


void Simulation::_setDefaultTime(const string& phase, float mean, float sd) {
	auto t = _defaults.find(phase);
	if (_defaults.end() == t) {
		_defaults.insert(make_pair(phase, make_pair(mean, sd)));
	}
	else {
		t->second = make_pair(mean, sd);
	}
}

string Simulation::_setDefaultNextAction(const string& c) const {
	if (G1_PHASE == c) {
		return S_PHASE;
	}
	if (S_PHASE == c) {
		return G2_PHASE;
	}
	if (G2_PHASE == c) {
		return DIVIDE;
	}
	if (G0_PHASE == c) {
		return G0_PHASE;
	}
	const string err{ "No action and no set default action" };
	throw err;
}
//pair<string,State*> Simulation::_parseCellWithState(const std::string& cell) const {
//	if (cell.find('[')==string::npos) {
//		return make_pair(cell,nullptr);
//	}
//	string stateStr{cell.substr(cell.find('[')+1,cell.find(']')-cell.find('[')-1)};
//	State* state{new State(stateStr)};
//	string cellName{cell.substr(0,cell.find('['))};
//
//	return make_pair(cellName,state);
//}
