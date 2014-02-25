/*
 * Cell.cpp
 *
 *  Created on: 14 Feb 2014
 *      Author: np183
 */

#include <iostream>
#include <random>
#include "Cell.h"
#include "Division.h"

using std::string;
using std::ostream;
using std::vector;

std::random_device Cell::_randomDev{};
std::mt19937 Cell::_randomGen{Cell::_randomDev()};


Cell::Cell(const std::string& n, const float& m, const float& sd, const std::string& d1, const std::string& d2, Simulation* s)
: _name(n),
//  _plan(p),
  _meanTime(m),
  _sd(sd),
  _daughter1(d1),
  _daughter2(d2),
  _sim(s)// ,
//  _pre(false),
//  _dead(false)
{}

Cell::~Cell() {
	// TODO Auto-generated destructor stub
}

vector<Event*> Cell::firstEvent(float currentTime) const {
	std::normal_distribution<> d(_meanTime,_sd);
	float time(d(_randomGen));
	Event* nextEvent(new Division(_name,_daughter1,_daughter2,time,currentTime+time,_sim));
	return vector<Event*>{nextEvent};
}

vector<Event*> Cell::nextEvent(float currentTime, Event* lastEvent) const {
	// TODO implement this
	return vector<Event*>{};
}

vector<string> Cell::otherPrograms() const {
	return vector<string>{_daughter1,_daughter2};
}

ostream& operator<<(ostream& out, const Cell& c) {
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
