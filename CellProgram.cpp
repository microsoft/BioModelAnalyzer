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
#include "Directive/Divide.h"

using std::string;
using std::ostream;
using std::stringstream;
using std::vector;
using std::make_pair;

CellProgram::CellProgram(const std::string& n, Simulation* s)
: _name(n), _sim(s), _program([](Condition* first, Condition* second) { return *first<*second;})
{
}

CellProgram::~CellProgram() {
	for (auto condDir : _program) {
		delete condDir.first;
		delete condDir.second;
	}
}

string CellProgram::name() const {
	return _name;
}

Simulation* CellProgram::simulation() const {
	return _sim;
}

vector<Event*> CellProgram::firstEvent(float currentTime, State* currentState) const {
	State* currentCopy=(nullptr==currentState ? nullptr : new State*(currentState));
	Cell* cell{new Cell(this,currentCopy)};
	_sim->addCell(cell);

	return vector<Event*>{new Birth(name(),currentCopy,0,currentTime,simulation(),cell)};
}

//vector<Event*> CellProgram::nextEvent(float currentTime, Cell* cell) const {
//	// Search for the next event applicable to this Cell
//	Directive* best{_bestMatch(cell->state())};
//	if (nullptr==best) {
//		stringstream err;
//		err << "In Cell " << _name << ". ";
//		err << "Failed to match state: " << cell->state();
//		throw err.str();
//	}
//
//	// Notice that if more than one event is created then
//	// all events correspond to the same cell!
//	vector<Event*> res=best->nextEvents(currentTime,cell);
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

vector<string> CellProgram::otherPrograms() const {
	vector<string> ret{};
	for (auto condDir : _program) {
		vector<string> temp{condDir.second->programs()};
		ret.insert(ret.begin(),temp.begin(),temp.end());
	}
	return ret;
}

void CellProgram::addCondition(Condition* cond, Directive* d)
{
	if (_conditionExists(cond)) {
		const string error{"A program created with the same condition twice"};
		throw error;
	}

	_program.insert(make_pair(cond,d));
}

ostream& operator<<(ostream& out, const CellProgram& c) {
	out << c._name;
	out << ":(";
	bool first{true};
	for (auto condDir : c._program) {
		if (!first) {
			out << ",";
		}
		out << condDir.first << "->" << condDir.second;
	}
	out << ")";
	return out;
}

bool CellProgram::_conditionExists(Condition* newCond) const {
	for (auto condDir : _program) {
		if (*(condDir.first) == *newCond) {
			return true;
		}
	}
	return false;
}

Directive* CellProgram::_bestMatch(const State st) const {
	Directive* best{nullptr};
	unsigned int val{0};
	for (auto condDir : _program) {
		Condition* cond{condDir.first};
		Directive* dir{condDir.second};
		auto satVal = cond->evaluate(st,_sim);
		if (satVal.first &&
			((nullptr==best && satVal.second==0) || // default hasn't been found
			 satVal.second>val)) { // some real condition (in particular the value>0)
			best=dir;
			val=satVal.second;
		}
	}
	return best;
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

