/*
 * Division.cpp
 *
 *  Created on: 14 Feb 2014
 *      Author: np183
 */

#include <iostream>
#include "Division.h"

using std::ostream;
using std::string;
using std::vector;

// Division::Division() : Event(0.0,0.0), _parent(), _daughter1(), _daughter2() {}

Division::Division(const std::string& p, const std::string& d1, State* st1,
		                                 const std::string& d2, State* st2,
		                                 float d, float t,
		                                 Simulation* s, Cell* c)
: Event(d,t,s,c), _parent(p), _daughter1(d1), _st1(st1), _daughter2(d2), _st2(st2) {}

Division::~Division() {
	delete _st1;
	delete _st2;
}


string Division::parent() const {
	return _parent;
}
string Division::daughter1() const {
	return _daughter1;
}
string Division::dauthger2() const {
	return _daughter2;
}

void Division::setParent(const string& p) {
	_parent=p;
}

void Division::setDaughter1(const string& d1) {
	_daughter1=d1;
}

void Division::setDaughter2(const string& d2) {
	_daughter2=d2;
}

vector<Event*> Division::execute() const {
	Simulation* sim(simulation());
	CellProgram* d1(sim->program(_daughter1));
	CellProgram* d2(sim->program(_daughter2));
	vector<Event*> events{},events1{},events2{};
	this->cell()->kill();
	if (d1)
		events1=d1->firstEvent(this->execTime(),_st1);
	if (d2)
		events2=d2->firstEvent(this->execTime(),_st2);
	for (auto event : events1) {
		events.push_back(event);
	}
	for (auto event : events2) {
		events.push_back(event);
	}
	return events;
}

void Division::output(ostream& out) const {
	Event::output(out);
	out << " " << _parent << " -> (" << _daughter1;
	if (_st1!=nullptr) {
		out << "[" << *_st1 << "]";
	}
	out << "," << _daughter2;
	if (_st2!=nullptr) {
		out << "[" << *_st2 << "]";
	}
	out << ")";
	// out << "(" << _daughter1.size() << "," << _daughter2.size() << ")";
}


bool Division::concerns(const string& name) const {
	return (_parent==name || _daughter1==name || _daughter2==name);
}
ostream& operator<<(ostream& out, const Division& d) {
	d.output(out);
	return out;
}


