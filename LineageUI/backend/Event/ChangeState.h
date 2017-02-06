// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
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
	void output(std::ostream&) const override;
	bool concerns(const std::string&) const override;
	bool expressed(const std::string& cell,const std::string& var) const override;

	std::string toString() const override;
	friend std::ostream& operator<<(std::ostream&,const ChangeState&);
private:
	std::unique_ptr<State> _oldState;
	std::unique_ptr<State> _newState;
};

#endif /* CHANGESTATE_H_ */
