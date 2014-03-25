/*
 * State.h
 *
 *  Created on: 19 Mar 2014
 *      Author: np183
 */

#ifndef STATE_H_
#define STATE_H_

#include <map>
#include <string>

class State;

#include "Condition.h"

class State {
public:
	State()=delete;
	State(const std::string& initializer);
	~State();

	std::pair<bool,unsigned int> evaluate(const Condition&) const;
	std::pair<bool,bool> value(const std::string& var) const;
	bool update(const std::string& var,bool val);
private:
	std::map<std::string, bool> _varVals;
};

#endif /* STATE_H_ */
