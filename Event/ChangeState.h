/*
 * ChangeState.h
 *
 *  Created on: 14 Feb 2014
 *      Author: np183
 */

#ifndef CHANGESTATE_H_
#define CHANGESTATE_H_

#include <iosfwd>
#include <memory>
#include "Event.h"

class ChangeState: public Event {
public:
	ChangeState() =delete;
	ChangeState(float d, float t, State*, State*, /*Simulation* s,*/ Cell* c);
	virtual ~ChangeState();

//	virtual std::vector<Event*> execute() const;
	virtual void output(std::ostream&) const;
	virtual bool concerns(const std::string&) const;
	virtual bool expressed(const std::string& cell,const std::string& var) const;

	virtual std::string toString() const;
	friend std::ostream& operator<<(std::ostream&,const ChangeState&);
private:
	std::auto_ptr<State> _oldState;
	std::auto_ptr<State> _newState;
};

#endif /* CHANGESTATE_H_ */
