/*
 * ChangeState.h
 *
 *  Created on: 14 Feb 2014
 *      Author: np183
 */

#ifndef CHANGESTATE_H_
#define CHANGESTATE_H_

#include <iosfwd>
#include "Event.h"

class ChangeState: public Event {
public:
	ChangeState(Simulation* s=nullptr);
	virtual ~ChangeState();

	virtual std::vector<Event*> execute() const;
//	virtual void output(std::ostream&) const;
	virtual bool concerns(const std::string&) const;

	friend std::ostream& operator<<(std::ostream&,const ChangeState&);
};

#endif /* CHANGESTATE_H_ */
