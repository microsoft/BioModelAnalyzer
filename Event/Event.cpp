/*
 * Event.cpp
 *
 *  Created on: 14 Feb 2014
 *      Author: np183
 */

#include <iostream>
#include <sstream>
#include "Event.h"

using std::ostream;
using std::string;
using std::stringstream;


Event::Event(float d, float t, /*Simulation* s,*/ Cell* c)
: _duration(d),
  _execTime(t),
  // _sim(s),
  _cell(c)
  {}

Event::~Event() {
}

Cell* Event::cell() const
{
	return _cell;
}

//Simulation* Event::simulation() const
//{
//	return _sim;
//}
//
float Event::duration() const
{
	return _duration;
}

float Event::execTime() const
{
	return _execTime;
}

//void Event::setDuration(float t) {
//	_duration=t;
//}
//
//void Event::setExecTime(float t) {
//	_execTime=t;
//}

void Event::setCell(Cell* c) {
	_cell=c;
}

void Event::output(ostream& out) const {
	out << "@" << _execTime << ":";
}

string Event::toString() const {
	stringstream temp{};
	temp << _execTime;
	return temp.str();
}
ostream& operator<<(ostream& out, const Event& ev) {
	ev.output(out);
	return out;
}
