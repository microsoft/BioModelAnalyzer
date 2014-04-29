/*
 * Birth.cpp
 *
 *  Created on: 25 Apr 2014
 *      Author: np183
 */


#include <iostream>
#include "Birth.h"

using std::string;
using std::vector;
using std::ostream;

Birth::Birth(const string name, State* st, float d, float t, Simulation* s, Cell* c)
: Event(d,t,s,c), _cellName(name), _st(st) {}

Birth::~Birth() {
	if (_st != nullptr)
		delete _st;
}

string Birth::cellName() const {
	return _cellName;
}

void Birth::setCell(const string& c) {
	_cellName=c;
}

vector<Event*> Birth::execute() const {
	Simulation* sim(simulation());
	CellProgram* prog(sim->program(_cellName));
	return prog->nextEvent(execTime(),cell());
}

void Birth::output(ostream& out) const {
	out << " " << _cellName;
}

bool Birth::concerns(const string& name) const {
	return name==_cellName;
}

string Birth::toString() const {
	string ret{};
	ret+=Event::toString();
	ret+=",";
	ret+= _cellName;
	ret+=",";
	if (_st!=nullptr) {
		ret+=_st->toString();
	}
	return ret;
}

ostream& operator<<(ostream& out, const Birth& b) {
	b.output(out);
	return out;
}
