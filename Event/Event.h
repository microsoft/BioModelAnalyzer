/*
 * Event.h
 *
 *  Created on: 14 Feb 2014
 *      Author: np183
 */

class Event;

#ifndef EVENT_H_
#define EVENT_H_

#include <vector>
#include <string>
#include <iosfwd>
#include "../Cell.h"
#include "../Simulation.h"

class Event {
public:
	Event() =delete;
	Event(float d, float t, /*Simulation* s,*/ Cell* c);

	virtual ~Event();

	Cell* cell() const;
	//Simulation* simulation() const;
	float duration() const;
	float execTime() const;

//	void setDuration(float t);
//	void setExecTime(float t);
	void setCell(Cell*);

	// virtual std::vector<Event*> execute() const=0;
	virtual void output(std::ostream&) const;
	virtual bool concerns(const std::string&) const=0;

	virtual std::string toString() const;
	friend std::ostream& operator<<(std::ostream&,const Event&);
private:
	float _duration;
	float _execTime;
	//Simulation* _sim;
	Cell* _cell;
};

#endif /* EVENT_H_ */
