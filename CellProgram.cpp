/*
 * CellProgram.cpp
 *
 *  Created on: 14 Feb 2014
 *      Author: np183
 */

#include <iostream>
#include <sstream>
#include "CellProgram.h"
#include "HelperFunctions.h"
#include "Event/Division.h"
#include "Event/Event.h"
#include "Event/ChangeState.h"
#include "Directive/Divide.h"

using std::string;
using std::ostream;
using std::stringstream;
using std::vector;
using std::pair;
using std::make_pair;

CellProgram::CellProgram(const std::string& n, Simulation* s)
: _name(n), _sim(s),
  _program([](Condition* first, Condition* second) { return *first<*second;}),
  _defState(nullptr), _defMean(-1.0), _defSD(-1.0)
{
}

CellProgram::~CellProgram() {
	for (pair<Condition*,Directive*> condDir : _program) {
		if (condDir.first!=nullptr)
			delete condDir.first;
		if (condDir.second!=nullptr)
			delete condDir.second;
	}
	if (_defState!=nullptr) {
		delete _defState;
	}
}

string CellProgram::name() const {
	return _name;
}

Simulation* CellProgram::simulation() const {
	return _sim;
}


vector<Happening*> CellProgram::firstEvent(float currentTime,
									   const string& initialState,
									   float initialMean,
									   float initialSD) const {
	// Find the right mean time of the first operation
	if (initialMean<=0.0) {
		if (_defMean <= 0.0) {
			const string err{"Cell "+_name+" does not have a default mean time."};
			throw err;
		}
		else {
			initialMean=_defMean;
		}
	}

	// Find the right standard deviation of the first operation
	if (initialSD < 0.0) {
		if (_defSD < 0.0) {
			const string err{"Cell "+_name+" does not have a default standard devisation."};
		}
		else {
			initialSD=_defSD;
		}
	}

	// Find the right initial state
	// using unique pointer so that I can use throw.
	// Notice that it is OK to start with no state!
	State* state{nullptr};
	if (initialState.size()==0) {
		if (_defState!=nullptr) {
			state=new State(*(_defState));
		}
	}
	else {
		state=new State(initialState);
	}

	Cell* cell{new Cell(this,state)};
	_sim->addCell(cell);
	Happening* h{new Happening(currentTime, initialMean, initialSD, _sim, cell)};
	return vector<Happening*>{h};
}

//vector<Event*> CellProgram::firstEvent(float currentTime, State* currentState) const {
//	State* currentCopy=(nullptr==currentState ? nullptr : new State(*currentState));
//	Cell* cell{new Cell(this,currentCopy)};
//	_sim->addCell(cell);
//
//	Birth* b{new Birth(name(),currentCopy,0,currentTime,simulation(),cell)};
//	return vector<Event*>{b};
//}
//
//vector<Event*> CellProgram::nextEvent(float currentTime, Cell* cell) const {
////	// Search for the next event applicable to this Cell
////	Directive* best{_bestMatch(cell->state())};
////	if (nullptr==best) {
////		stringstream err;
////		err << "In Cell " << _name << ". ";
////		err << "Failed to match state: " << cell->state();
////		throw err.str();
////	}
////
////	// Notice that if more than one event is created then
////	// all events correspond to the same cell!
////	vector<Event*> res=best->nextEvents(currentTime,cell);
//	vector<Event*> res{};
//	return res;
//}
//
//

//	Directive* best{_bestMatch(*currentState)};
//	if (nullptr==best) {
//		stringstream err{"in Cell "};
//		err << _name << ". Failed to match state: " << *currentState;
//		throw err.str();
//	}
//	vector<Event*> res=best->nextEvents(currentTime,currentState);
//	return res;
//}

const Directive* CellProgram::bestDirective(const State* state) const {
	Directive* best{nullptr};
	unsigned int val{0};
	for (pair<Condition*,Directive*> condDir : _program) {
		Condition* cond{condDir.first};
		Directive* dir{condDir.second};
		auto satVal = cond->evaluate(state,_sim);
		if (satVal.first &&
				((nullptr==best && satVal.second==0) || // default hasn't been found
						satVal.second>val)) { // some real condition (in particular the value>0)
			best=dir;
			val=satVal.second;
		}
	}
	return best;
}

vector<string> CellProgram::otherPrograms() const {
	vector<string> ret{};
	for (auto condDir : _program) {
		vector<string> temp{condDir.second->programs()};
		ret.insert(ret.begin(),temp.begin(),temp.end());
	}
	return ret;
}

void CellProgram::setDefaults(State* st, float mean, float sd) {
	_defState=st;
	_defMean=mean;
	_defSD=sd;
}

void CellProgram::addCondition(Condition* cond, Directive* d)
{
	if (_conditionExists(cond)) {
		const string error{"A program created with the same condition twice"};
		throw error;
	}

	_program.insert(make_pair(cond,d));
}

const State* CellProgram::defState() const {
	return _defState;
}

float CellProgram::defMean() const {
	return _defMean;
}

float CellProgram::defSD() const {
	return _defSD;
}

ostream& operator<<(ostream& out, const CellProgram& c) {
	out << c._name;
	out << ":(";
	bool first{true};
	for (pair<Condition*,Directive*> condDir : c._program) {
		if (!first) {
			out << ",";
		}
		out << condDir.first << "->" << condDir.second;
	}
	out << ")";
	return out;
}

bool CellProgram::_conditionExists(Condition* newCond) const {
	for (pair<Condition*,Directive*> condDir : _program) {
		if (*(condDir.first) == *newCond) {
			return true;
		}
	}
	return false;
}

CellProgram::iterator::iterator() {}

CellProgram::iterator::iterator(const CellProgram::iterator& it) {
	_it=it._it;
}

CellProgram::iterator::iterator(CellProgram::iterator&& it) {
	_it=it._it;
}

CellProgram::iterator::~iterator() {}

CellProgram::iterator CellProgram::begin() {
	iterator ret{};
	ret._it=_program.begin();
	return ret;
}

CellProgram::iterator CellProgram::end() {
	iterator ret{};
	ret._it=_program.end();
	return ret;
}

bool CellProgram::iterator::operator==(const iterator& other) const {
	return _it==other._it;
}

bool CellProgram::iterator::operator!=(const iterator& other) const {
	return !(*this==other);
}

CellProgram::iterator CellProgram::iterator::operator++() {
	++_it;
	return *this;
}

CellProgram::iterator CellProgram::iterator::operator++(int i) {
	++_it;
	return *this;
}
Condition* CellProgram::iterator::operator->() const {
	std::pair<Condition*,Directive*> elem{*_it};
	return elem.first;
}

Condition CellProgram::iterator::operator*() const {
	std::pair<Condition*,Directive*> elem{*_it};
	return *(elem.first);
}

