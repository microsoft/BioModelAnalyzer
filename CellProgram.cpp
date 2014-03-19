/*
 * CellProgram.cpp
 *
 *  Created on: 14 Feb 2014
 *      Author: np183
 */

#include <iostream>
#include <random>
#include "CellProgram.h"
#include "HelperFunctions.h"
#include "Division.h"

using std::string;
using std::ostream;
using std::vector;
using std::make_pair;

std::random_device CellProgram::_randomDev{};
std::mt19937 CellProgram::_randomGen{CellProgram::_randomDev()};


CellProgram::CellProgram(const std::string& n, Simulation* s)
: _name(n), _sim(s)
{
}

CellProgram::~CellProgram() {
	for (auto thing : _program) {
		delete thing.second;
	}
	// TODO: Auto-generated destructor stub
}

vector<Event*> CellProgram::firstEvent(float currentTime) const {
	std::normal_distribution<> d(_meanTime,_sd);
	float time(d(_randomGen));
	// TODO: handle Cell
	Event* nextEvent(new Division(_name,_daughter1,_daughter2,time,currentTime+time,_sim,nullptr));
	return vector<Event*>{nextEvent};
}

vector<Event*> CellProgram::nextEvent(float currentTime, Event* lastEvent) const {
	// TODO implement this
	return vector<Event*>{};
}

vector<string> CellProgram::otherPrograms() const {
	return vector<string>{_daughter1,_daughter2};
}

void CellProgram::addCondition(const string& cond, Event* e)
{
	Condition newCond{cond};
	if (_conditionExists(newCond)) {
		const string error{"A program created with the same condition twice"};
		throw error;
	}

	_program.insert(make_pair(newCond,e));
	// TODO: implement this
}

ostream& operator<<(ostream& out, const CellProgram& c) {
	out << c._name;
	out << ":(";
	out << c._meanTime;
	out << ",";
	out << c._sd;
	out << ") ";
	out << c._daughter1;
	out << " ";
	out << c._daughter2;
	return out;
}

bool CellProgram::_conditionExists(const Condition& newCond) const {
	for (auto thing : _program) {
		if (thing.first == newCond) {
			return true;
		}
	}
	return false;
}
