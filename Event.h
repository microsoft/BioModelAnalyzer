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
#include <iosfwd>
#include "Simulation.h"

class Event {
public:
	Event() =delete;
	Event(float d, float t, Simulation* s);

	virtual ~Event();

	Simulation* simulation() const;
	float duration() const;
	float execTime() const;

//	void setDuration(float t);
//	void setExecTime(float t);

	virtual std::vector<Event*> execute() const=0;
	virtual void output(std::ostream&) const;
	virtual bool concerns(const std::string&) const=0;

	bool operator<(const Event& other) const;

	friend std::ostream& operator<<(std::ostream&,const Event&);

private:
	float _duration;
	float _execTime;
	Simulation* _sim;
};

#endif /* EVENT_H_ */
