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
	Directive* best{_bestMatch(currentState)};
	if (nullptr==best) {
		stringstream err;
		err << "In Cell " << _name << ". ";
		if (currentState) {
			err << "Failed to match state: " << *currentState;
		}
		else {
			err << "There is no default.";
		}
		throw err.str();
	}

	// TODO:
	// Notice that if more than one event is created then
	// all events correspond to the same cell!
	State* currentCopy=(nullptr==currentState ? nullptr : new State(*currentState));
	Cell* cell{new Cell(this,currentCopy)};
	_sim->addCell(cell);
	vector<Event*> res=best->nextEvents(currentTime,cell,currentState);
	return res;
}

//vector<Event*> CellProgram::nextEvent(float currentTime, State* currentState) const {
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

Directive* CellProgram::_bestMatch(const State* st) const {
	Directive* best{nullptr};
	unsigned int val{0};
	for (auto condDir : _program) {
		Condition* cond{condDir.first};
		Directive* dir{condDir.second};
		auto satVal = cond->evaluate(st);
		if (satVal.first &&
			((nullptr==best && satVal.second==0) || // default hasn't been found
			 satVal.second>val)) { // some real condition (in particular the value>0)
			best=dir;
			val=satVal.second;
		}
	}
	return best;
}
