/*
 * CellProgram.cpp
 *
 *  Created on: 14 Feb 2014
 *      Author: np183
 */

#include <iostream>
#include "CellProgram.h"
#include "HelperFunctions.h"
#include "Event/Division.h"
#include "Directive/Divide.h"

using std::string;
using std::ostream;
using std::vector;
using std::make_pair;

CellProgram::CellProgram(const std::string& n, Simulation* s)
: _name(n), _sim(s)
{
}

CellProgram::~CellProgram() {
	for (auto condDir : _program) {
		delete condDir.second;
	}
	// TODO: Auto-generated destructor stub
}

vector<Event*> CellProgram::firstEvent(float currentTime) const {
//	Event* nextEvent(new Division(_name,_daughter1,_daughter2,time,currentTime+time,_sim,nullptr));
//	return vector<Event*>{nextEvent};


	// TODO: implement this
	return vector<Event*>{};
}

vector<Event*> CellProgram::nextEvent(float currentTime, Event* lastEvent) const {
	// TODO implement this
	return vector<Event*>{};
}

vector<string> CellProgram::otherPrograms() const {
	vector<string> ret{};
	for (auto condDir : _program) {
		vector<string> temp{condDir.second->programs()};
		ret.insert(ret.begin(),temp.begin(),temp.end());
	}
	return ret;
}

void CellProgram::addCondition(const string& cond, Directive* d)
{
	Condition newCond{cond};
	if (_conditionExists(newCond)) {
		const string error{"A program created with the same condition twice"};
		throw error;
	}

	_program.insert(make_pair(newCond,d));
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

bool CellProgram::_conditionExists(const Condition& newCond) const {
	for (auto condDir : _program) {
		if (condDir.first == newCond) {
			return true;
		}
	}
	return false;
}
