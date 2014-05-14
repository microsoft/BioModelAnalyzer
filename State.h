/*
 * State.h
 *
 *  Created on: 19 Mar 2014
 *      Author: np183
 */

#ifndef STATE_H_
#define STATE_H_

#include <iosfwd>
#include <map>
#include <string>

class State;

#include "Simulation.h"
#include "Condition.h"

class State {
public:
	State()=delete;
	State(const std::string& initializer);
	State(const State&);
	~State();

	// std::pair<bool,unsigned int> evaluate(const Condition&) const;

	// Returns if the variable exists and its polarity
	std::pair<bool,bool> value(const std::string& var) const;
	void set(const std::string& var, bool val);
	bool update(const std::string& var,bool val);
	bool update(const State* other);

	State* copyOverwrite(const State*) const;

	std::string toString() const;
	friend std::ostream& operator<< (std::ostream&, const State&);
private:
	std::map<std::string, bool> _varVals;
};

#endif /* STATE_H_ */
